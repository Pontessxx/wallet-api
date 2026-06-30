using Application.Interfaces;
using Auth.Domain;

namespace Application.Services;

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _hasher;

    public UserService(IUserRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<User> CreateAsync(string username, string password)
    {
        if (await _repository.ExistsByUsernameAsync(username))
            throw new InvalidOperationException("Já existe um usuário com esse nome.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = _hasher.Hash(password),
            CreatedAt = DateTime.UtcNow,
            Role = RoleUser.User
        };

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        return user;
    }
}