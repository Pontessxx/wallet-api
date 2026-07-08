namespace Auth.Domain
{
    public class Transacoes : TransacaoBase
    {
        public Guid CarteiraId { get; set; }
        public string Tipo { get; set; } = null!;

        public Carteira Carteira { get; set; } = null!;
    }
}