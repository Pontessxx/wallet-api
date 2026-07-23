namespace Auth.Domain
{
    public class TransferenciaCarteira : TransacaoBase
    {
        public Guid CarteiraOrigemId { get; set; }
        public Guid CarteiraDestinoId { get; set; }
        public decimal? TaxaCambio { get; set; }
        public decimal? ValorConvertido { get; set; }

        public Carteira CarteiraOrigem { get; set; } = null!;
        public Carteira CarteiraDestino { get; set; } = null!;
    }
}