namespace Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    Task<IReadOnlyList<RefreshToken>> GetExpiredAsync(DateTime utcNow, CancellationToken ct = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);

    void RemoveRange(IEnumerable<RefreshToken> refreshTokens);

    Task SaveChangesAsync(CancellationToken ct = default);
}