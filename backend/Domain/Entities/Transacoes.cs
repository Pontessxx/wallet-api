namespace Auth.Domain
{
    public class Transacoes : TransacaoBase
    {
        public Guid CarteiraId { get; set; }
        public TipoTransacoes Tipo { get; set; }
        public CategoriaTransacao? Categoria { get; set; }

        public Carteira Carteira { get; set; } = null!;
    }
}