namespace Auth.Domain
{
    public class WalletAccount
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public WalletCategory Category { get; set; }
        public string Name { get; set; } = null!;

        public User User { get; set; } = null!;
        public Wallet Wallet { get; set; } = null!;
    }
}