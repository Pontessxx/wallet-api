namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null, ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => _context.Users
            .AnyAsync(u => u.Username == username && u.DeletedAt == null, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}