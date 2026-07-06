namespace Auth.Domain
{
    public class Carteira
    {
        public Guid Id { get; set; }
        public Guid ContaCarteiraId { get; set; }
        public string Descricao { get; set; } = null!;
        public decimal SaldoInicial { get; set; }
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Transferencias { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoProjetado { get; set; }

        public ContaCarteira ContaCarteira { get; set; } = null!;
    }
}