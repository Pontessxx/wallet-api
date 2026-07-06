namespace Auth.Domain
{
    public class TransferenciaCarteira
    {
        public Guid Id { get; set; }
        public Guid ContaCarteiraOrigemId { get; set; }
        public Guid ContaCarteiraDestinoId { get; set; }
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

        public ContaCarteira ContaCarteiraOrigem { get; set; } = null!;
        public ContaCarteira ContaCarteiraDestino { get; set; } = null!;
    }
}