namespace Auth.Domain
{
    public class TransferenciaCarteira
    {
        public Guid Id { get; set; }
        public Guid CarteiraOrigemId { get; set; }
        public Guid CarteiraDestinoId { get; set; }
        public decimal Valor { get; set; }
        public decimal Encargos { get; set; }
        public decimal ValorTotal { get; set; }
        public bool Efetivada { get; set; }
        public DateTime DataLancamento { get; set; }
        public DateTime? DataVencimento { get; set; }
        public DateTime? DataEfetivacao { get; set; }
        public string? Observacoes { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? AtualizadaEm { get; set; }

        public Carteira CarteiraOrigem { get; set; } = null!;
        public Carteira CarteiraDestino { get; set; } = null!;
    }
}