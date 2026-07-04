namespace Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private const int RefreshTokenBytes = 64;

    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RefreshTokenBytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _refreshTokenRepository.GetByTokenAsync(tokenHash, ct)
            ?? throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        if (token.IsRevoked || token.IsExpired)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        return token;
    }

    public async Task<(RefreshToken RefreshToken, string RawToken)> IssueRefreshTokenAsync(
        Guid userId,
        string? createdByIp,
        CancellationToken ct = default)
    {
        var rawToken = GenerateRefreshToken();
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = HashToken(rawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedByIp = createdByIp
        };

        await _refreshTokenRepository.AddAsync(token, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return (token, rawToken);
    }

    public async Task<(RefreshToken RefreshToken, string RawToken)> RotateRefreshTokenAsync(
        RefreshToken currentToken,
        string? revokedByIp,
        CancellationToken ct = default)
    {
        await RevokeRefreshTokenAsync(currentToken, revokedByIp, ct);

        return await IssueRefreshTokenAsync(currentToken.UserId, revokedByIp, ct);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? revokedByIp, CancellationToken ct = default)
    {
        if (refreshToken.IsRevoked)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = revokedByIp;

        await _refreshTokenRepository.SaveChangesAsync(ct);
    }

    public async Task RemoveExpiredTokensAsync(CancellationToken ct = default)
    {
        var expiredTokens = await _refreshTokenRepository.GetExpiredAsync(DateTime.UtcNow, ct);

        if (expiredTokens.Count == 0)
            return;

        _refreshTokenRepository.RemoveRange(expiredTokens);
        await _refreshTokenRepository.SaveChangesAsync(ct);
    }

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}