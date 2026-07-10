namespace Application.Interfaces;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();

    Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    Task<(RefreshToken RefreshToken, string RawToken)> IssueRefreshTokenAsync(
        Guid userId,
        string? createdByIp,
        CancellationToken ct = default);

    Task<(RefreshToken RefreshToken, string RawToken)> RotateRefreshTokenAsync(
        RefreshToken currentToken,
        string? revokedByIp,
        CancellationToken ct = default);

    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? revokedByIp, CancellationToken ct = default);

    Task RevokeActiveRefreshTokensByUserAsync(Guid userId, string? revokedByIp, CancellationToken ct = default);

    Task RemoveExpiredTokensAsync(CancellationToken ct = default);
}