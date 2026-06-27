using Application.Interfaces;

namespace Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}