using Auth.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, IConfiguration config, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var adminUsername = _config["Seed:AdminUsername"] ?? "admin";
        var adminPassword = _config["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword não configurado.");

        var exists = await _context.Users.AnyAsync(u => u.Username == adminUsername);
        if (exists)
        {
            _logger.LogInformation("Admin já existe, seed ignorado.");
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = adminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = RoleUser.Admin,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin criado com sucesso.");
    }
}