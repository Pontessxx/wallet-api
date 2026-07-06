namespace Auth.Domain
{
    public class ContaCarteira
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public WalletCategory Categoria { get; set; }
        public string Nome { get; set; } = null!;

        public User User { get; set; } = null!;
        public Carteira Carteira { get; set; } = null!;
        public ICollection<TransacaoBolsa> TransacoesBolsa { get; set; } = new List<TransacaoBolsa>();
        public ICollection<TransferenciaCarteira> TransferenciasSaida { get; set; } = new List<TransferenciaCarteira>();
        public ICollection<TransferenciaCarteira> TransferenciasEntrada { get; set; } = new List<TransferenciaCarteira>();
    }
}