namespace Auth.Domain
{
    public class Objetivo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? CarteiraId { get; set; }
        public string Nome { get; set; } = null!;
        public string IconKey { get; set; } = "target";
        public decimal ValorTotal { get; set; }
        public int Meses { get; set; }
        public decimal ValorMensal { get; set; }
        public decimal AporteManualAcumulado { get; set; }
        public DateTime CriadaEm { get; set; }
        public DateTime? AtualizadaEm { get; set; }

        public User User { get; set; } = null!;
        public Carteira? Carteira { get; set; }
        public ICollection<ObjetivoAporte> Aportes { get; set; } = new List<ObjetivoAporte>();
    }
}