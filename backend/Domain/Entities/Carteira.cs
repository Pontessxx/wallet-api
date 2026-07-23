namespace Auth.Domain
{
    public class Carteira
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public WalletCategory Categoria { get; set; }
        public WalletOrigin Origem { get; set; }
        public string Nome { get; set; } = null!;

        // saldos agregados
        public decimal SaldoInicial { get; set; }
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Transferencias { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoProjetado { get; set; }

        public User User { get; set; } = null!;
        public ICollection<TransacaoBolsa> TransacoesBolsa { get; set; } = new List<TransacaoBolsa>();
        public ICollection<TransferenciaCarteira> TransferenciasSaida { get; set; } = new List<TransferenciaCarteira>();
        public ICollection<TransferenciaCarteira> TransferenciasEntrada { get; set; } = new List<TransferenciaCarteira>();
    }
}