namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, DateTime utcNow, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > utcNow)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RefreshToken>> GetExpiredAsync(DateTime utcNow, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= utcNow)
            .ToListAsync(ct);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
        => await _context.RefreshTokens.AddAsync(refreshToken, ct);

    public void RemoveRange(IEnumerable<RefreshToken> refreshTokens)
        => _context.RefreshTokens.RemoveRange(refreshTokens);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}