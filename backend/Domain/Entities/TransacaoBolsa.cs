namespace Auth.Domain
{
    public class TransacaoBolsa : TransacaoBase
    {
        public Guid CarteiraId { get; set; }
        public string CodigoAtivo { get; set; } = null!;
        public TipoTransacaoBolsa Lado { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public Carteira Carteira { get; set; } = null!;
    }
}