namespace Auth.Domain
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public Guid WalletAccountId { get; set; }
        public string Description { get; set; } = null!;
        public decimal InitialBalance { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Transfers { get; set; }
        public decimal Balance { get; set; }
        public decimal Projected { get; set; }

        public WalletAccount WalletAccount { get; set; } = null!;
    }
}