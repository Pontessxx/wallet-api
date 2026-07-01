namespace Application.Services;

public class AuthService
{
    private const int ResetCodeLength = 6;
    private const int MaxResetCodeAttempts = 5;
    private static readonly TimeSpan ResetCodeLifetime = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IResetCodeValidator _resetCodeValidator;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IResetCodeValidator resetCodeValidator)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _resetCodeValidator = resetCodeValidator;
    }

    public async Task<string> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        return _tokenService.GenerateToken(user);
    }

    public async Task<(string Code, DateTime ExpiresAt)> GeneratePasswordResetCodeAsync(string username, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString($"D{ResetCodeLength}");
        var expiresAt = DateTime.UtcNow.Add(ResetCodeLifetime);

        user.ResetCodeHash = _passwordHasher.Hash(code);
        user.ResetCodeExpiresAt = expiresAt;
        user.ResetCodeFailedAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(ct);

        return (code, expiresAt);
    }

    public async Task ChangePasswordAsync(string username, string resetCode, string newPassword, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (!_resetCodeValidator.IsValid(resetCode))
            throw new UnauthorizedAccessException("Código de redefinição inválido ou expirado.");

        if (user.ResetCodeHash is null || user.ResetCodeExpiresAt is null || user.ResetCodeExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Código de redefinição inválido ou expirado.");

        if (user.ResetCodeFailedAttempts >= MaxResetCodeAttempts)
            throw new UnauthorizedAccessException("Código de redefinição bloqueado. Gere um novo código.");

        if (!_passwordHasher.Verify(resetCode, user.ResetCodeHash))
        {
            user.ResetCodeFailedAttempts++;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Código de redefinição inválido ou expirado.");
        }

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.ResetCodeHash = null;
        user.ResetCodeExpiresAt = null;
        user.ResetCodeFailedAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(ct);
    }
}