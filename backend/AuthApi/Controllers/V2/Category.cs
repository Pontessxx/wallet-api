namespace AuthApi.Controllers.V2;

[ApiController]
[Route("category/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class CategoryController : ControllerBase
{
    private const string DefaultIconKey = "tag";
    private const string DefaultColorHex = "#64748B";
    private static readonly HashSet<string> AllowedIconKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "tag",
        "shopping-cart",
        "car",
        "house",
        "briefcase",
        "heart-pulse",
        "book-open",
        "gamepad-2",
        "plane",
        "utensils"
    };

    private readonly ApplicationDbContext _dbContext;

    public CategoryController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retorna todas as categorias do usuário autenticado.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista de categorias do usuário</returns>
    /// <response code="200">Categorias retornadas com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(V2CategoryListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var categorias = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Nome)
            .Select(c => MapCategory(c))
            .ToListAsync(ct);

        return Ok(new V2CategoryListResult(categorias));
    }

    /// <summary>
    /// Cria uma nova categoria para o usuário autenticado.
    /// </summary>
    /// <param name="request">Nome da nova categoria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Categoria criada</returns>
    /// <response code="201">Categoria criada com sucesso</response>
    /// <response code="400">Dados inválidos para a categoria</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpPost("new")]
    [ProducesResponseType(typeof(V2CategoryResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] V2CreateCategoryRequest request, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var nome = request.Nome?.Trim();
        if (string.IsNullOrWhiteSpace(nome))
            return this.BadRequestError("Nome da categoria é obrigatório.");

        if (nome.Length > 80)
            return this.BadRequestError("Nome da categoria deve ter no máximo 80 caracteres.");

        var iconKey = NormalizeIconKey(request.IconKey);
        if (!AllowedIconKeys.Contains(iconKey))
            return this.BadRequestError("Ícone de categoria inválido.");

        var colorHex = NormalizeColorHex(request.ColorHex);
        if (!IsValidHexColor(colorHex))
            return this.BadRequestError("Cor da categoria inválida. Use o formato hexadecimal #RRGGBB.");

        var categoryExists = await _dbContext.Categories
            .AnyAsync(c => c.UserId == userId && c.Nome.ToUpper() == nome.ToUpper(), ct);

        if (categoryExists)
            return this.BadRequestError("Já existe uma categoria com esse nome para o usuário autenticado.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Nome = nome,
            IconKey = iconKey,
            ColorHex = colorHex,
            CriadaEm = DateTime.UtcNow
        };

        await _dbContext.Categories.AddAsync(category, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapCategory(category));
    }

    /// <summary>
    /// Exclui uma categoria do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Categoria excluída com sucesso</response>
    /// <response code="400">Categoria possui transações vinculadas</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Categoria não encontrada</response>
    [HttpDelete("remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (category is null)
            return this.NotFoundError("Categoria não encontrada.");

        var hasTransactions = await _dbContext.Transacoes
            .AnyAsync(t => t.CategoriaId == id, ct);

        if (hasTransactions)
            return this.BadRequestError("Não é possível excluir uma categoria que possui transações vinculadas.");

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }

    private static string NormalizeIconKey(string? iconKey)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
            return DefaultIconKey;

        return iconKey.Trim().ToLowerInvariant();
    }

    private static string NormalizeColorHex(string? colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex))
            return DefaultColorHex;

        return colorHex.Trim().ToUpperInvariant();
    }

    private static bool IsValidHexColor(string colorHex)
    {
        if (colorHex.Length != 7 || colorHex[0] != '#')
            return false;

        for (var i = 1; i < colorHex.Length; i++)
        {
            if (!Uri.IsHexDigit(colorHex[i]))
                return false;
        }

        return true;
    }

    private static V2CategoryResult MapCategory(Category category)
        => new(
            category.Id,
            category.Nome,
            NormalizeIconKey(category.IconKey),
            NormalizeColorHex(category.ColorHex),
            category.CriadaEm,
            category.AtualizadaEm);
}
