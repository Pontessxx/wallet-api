namespace Application.Interfaces;

public interface ICategoryRepository
{
    Task AddDefaultCategoriesAsync(Guid userId, CancellationToken ct = default);
}
