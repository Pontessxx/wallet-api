namespace Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, TimeSpan? expiresIn = null);
}