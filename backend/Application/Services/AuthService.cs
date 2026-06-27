using Application.Interfaces;
using Auth.Domain;

namespace Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        return _tokenService.GenerateToken(user);
    }
}