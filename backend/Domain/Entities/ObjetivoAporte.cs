namespace Auth.Domain
{
    public class ObjetivoAporte
    {
        public Guid Id { get; set; }
        public Guid ObjetivoId { get; set; }
        public Guid? TransacaoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Observacao { get; set; }
        public bool Recorrente { get; set; }
        public DateTime CriadoEm { get; set; }

        public Objetivo Objetivo { get; set; } = null!;
        public Transacoes? Transacao { get; set; }
    }
}
