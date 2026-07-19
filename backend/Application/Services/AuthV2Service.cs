namespace Application.Services;

public class AuthV2Service
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthV2Service(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthSessionResult> LoginAsync(
        string username,
        string password,
        TicketValidationType ticketValidation,
        string? ipAddress,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var accessToken = GenerateAccessToken(user, ticketValidation);
        var refreshToken = await _refreshTokenService.IssueRefreshTokenAsync(user.Id, ipAddress, ct);

        return BuildSessionResult(user, accessToken, refreshToken);
    }

    public async Task<AuthSessionResult> RegisterAsync(
        string username,
        string password,
        string? ipAddress,
        CancellationToken ct = default,
        TicketValidationType ticketValidation = TicketValidationType.JwtOnly)
    {
        if (await _userRepository.ExistsByUsernameAsync(username, ct))
            throw new InvalidOperationException("Já existe um usuário com esse nome.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = _passwordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow,
            Role = RoleUser.User
        };

        await _userRepository.AddAsync(user, ct);
        await _categoryRepository.AddDefaultCategoriesAsync(user.Id, ct);
        await _userRepository.SaveChangesAsync(ct);

        var accessToken = GenerateAccessToken(user, ticketValidation);
        var refreshToken = await _refreshTokenService.IssueRefreshTokenAsync(user.Id, ipAddress, ct);

        return BuildSessionResult(user, accessToken, refreshToken);
    }

    public async Task<AuthSessionResult> RefreshAsync(
        string refreshTokenValue,
        TicketValidationType ticketValidation,
        Guid authenticatedUserId,
        string? ipAddress,
        CancellationToken ct = default)
    {
        var currentRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenValue, ct);

        if (currentRefreshToken.UserId != authenticatedUserId)
            throw new UnauthorizedAccessException("Refresh token não pertence ao usuário autenticado.");

        var user = await _userRepository.GetByIdAsync(currentRefreshToken.UserId, ct)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        var nextRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(currentRefreshToken, ipAddress, ct);
        var accessToken = GenerateAccessToken(user, ticketValidation);

        return BuildSessionResult(user, accessToken, nextRefreshToken);
    }

    public async Task<RefreshTokenValidationResult> ValidateRefreshOwnershipAsync(
        string refreshTokenValue,
        Guid userId,
        CancellationToken ct = default)
    {
        var currentRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenValue, ct);

        if (currentRefreshToken.UserId != userId)
            throw new UnauthorizedAccessException("Refresh token não pertence ao usuário autenticado.");

        return new RefreshTokenValidationResult(
            IsValid: true,
            UserId: currentRefreshToken.UserId,
            ExpiresAt: currentRefreshToken.ExpiresAt);
    }

    public async Task LogoutAsync(
        Guid authenticatedUserId,
        string? refreshTokenValue,
        string? ipAddress,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            try
            {
                var currentRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenValue, ct);

                if (currentRefreshToken.UserId == authenticatedUserId)
                    await _refreshTokenService.RevokeRefreshTokenAsync(currentRefreshToken, ipAddress, ct);
            }
            catch (UnauthorizedAccessException)
            {
                // No-op: invalid or expired token should not block logout.
            }
        }

        await _refreshTokenService.RevokeActiveRefreshTokensByUserAsync(authenticatedUserId, ipAddress, ct);
    }

    private string GenerateAccessToken(User user, TicketValidationType ticketValidation)
        => ticketValidation switch
        {
            TicketValidationType.JwtOnly => _tokenService.GenerateToken(user, AccessTokenLifetime),
            _ => throw new InvalidOperationException("Tipo de validação de ticket não suportado.")
        };

    private static AuthSessionResult BuildSessionResult(
        User user,
        string accessToken,
        (RefreshToken RefreshToken, string RawToken) refreshToken)
        => new(
            AccessToken: accessToken,
            ExpiresIn: (int)AccessTokenLifetime.TotalSeconds,
            User: new AuthenticatedUserResult(user.Id, user.Username),
            RefreshToken: refreshToken.RawToken,
            RefreshTokenExpiresAt: refreshToken.RefreshToken.ExpiresAt);
}