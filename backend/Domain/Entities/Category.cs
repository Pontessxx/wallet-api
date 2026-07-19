namespace Auth.Domain
{
    public class Category
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Nome { get; set; } = null!;
        public string IconKey { get; set; } = "tag";
        public string ColorHex { get; set; } = "#64748B";
        public CategoriaTipo Tipo { get; set; } = CategoriaTipo.Despesa;
        public DateTime CriadaEm { get; set; }
        public DateTime? AtualizadaEm { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Transacoes> Transacoes { get; set; } = new List<Transacoes>();
    }
}