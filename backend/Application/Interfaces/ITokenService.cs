using Auth.Domain;

namespace Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}