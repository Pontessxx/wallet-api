namespace Application.Interfaces;

public interface IPasswordHasher
{
    bool Verify(string password, string hash);
}