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

    public async Task<User> CreateAsync(string username, string password, CancellationToken ct = default)
    {
        if (await _repository.ExistsByUsernameAsync(username, ct))
            throw new InvalidOperationException("Já existe um usuário com esse nome.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = _hasher.Hash(password),
            CreatedAt = DateTime.UtcNow,
            Role = RoleUser.User
        };

        await _repository.AddAsync(user, ct);
        await _repository.SaveChangesAsync(ct);

        return user;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _repository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(ct);
    }
}