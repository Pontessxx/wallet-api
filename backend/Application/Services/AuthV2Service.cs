namespace Application.Services;

public class AuthV2Service
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthV2Service(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthSessionResult> LoginAsync(string username, string password, string? ipAddress, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var accessToken = _tokenService.GenerateToken(user, AccessTokenLifetime);
        var refreshToken = await _refreshTokenService.IssueRefreshTokenAsync(user.Id, ipAddress, ct);

        return BuildSessionResult(user, accessToken, refreshToken);
    }

    public async Task<AuthSessionResult> RegisterAsync(string username, string password, string? ipAddress, CancellationToken ct = default)
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
        await _userRepository.SaveChangesAsync(ct);

        var accessToken = _tokenService.GenerateToken(user, AccessTokenLifetime);
        var refreshToken = await _refreshTokenService.IssueRefreshTokenAsync(user.Id, ipAddress, ct);

        return BuildSessionResult(user, accessToken, refreshToken);
    }

    public async Task<AuthSessionResult> RefreshAsync(string refreshTokenValue, string? ipAddress, CancellationToken ct = default)
    {
        var currentRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenValue, ct);
        var user = await _userRepository.GetByIdAsync(currentRefreshToken.UserId, ct)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        var nextRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(currentRefreshToken, ipAddress, ct);
        var accessToken = _tokenService.GenerateToken(user, AccessTokenLifetime);

        return BuildSessionResult(user, accessToken, nextRefreshToken);
    }

    public async Task LogoutAsync(string refreshTokenValue, string? ipAddress, CancellationToken ct = default)
    {
        var currentRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshTokenValue, ct);
        await _refreshTokenService.RevokeRefreshTokenAsync(currentRefreshToken, ipAddress, ct);
    }

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