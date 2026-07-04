namespace Auth.Domain
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }

        public User User { get; set; } = null!;

        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    }
}