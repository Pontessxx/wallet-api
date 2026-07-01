namespace Auth.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ResetCodeHash { get; set; }
        public DateTime? ResetCodeExpiresAt { get; set; }
        public int ResetCodeFailedAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public RoleUser Role { get; set; } = RoleUser.User;
    }
}
