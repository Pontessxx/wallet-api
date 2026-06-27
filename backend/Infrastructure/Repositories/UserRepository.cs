using Application.Interfaces;
using Auth.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
}