namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private static readonly (string Nome, string IconKey, string ColorHex, CategoriaTipo Tipo)[] DefaultCategories =
    {
        ("Alimentação", "utensils", "#F97316", CategoriaTipo.Despesa),
        ("Transporte", "car", "#3B82F6", CategoriaTipo.Despesa),
        ("Moradia", "house", "#22C55E", CategoriaTipo.Despesa),
        ("Lazer", "gamepad-2", "#8B5CF6", CategoriaTipo.Despesa),
        ("Salário", "briefcase", "#06B6D4", CategoriaTipo.Receita),
        ("Outras Receitas", "tag", "#64748B", CategoriaTipo.Receita),
    };

    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddDefaultCategoriesAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var categories = DefaultCategories.Select(c => new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Nome = c.Nome,
            IconKey = c.IconKey,
            ColorHex = c.ColorHex,
            Tipo = c.Tipo,
            CriadaEm = now
        });

        await _context.Categories.AddRangeAsync(categories, ct);
    }
}
