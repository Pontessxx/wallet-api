namespace AuthApi.Models;

public record V2AuthSessionResponse(
    string AccessToken,
    int ExpiresIn,
    Guid UserId,
    string Username);

public record V2RefreshResponse(
    string AccessToken,
    int ExpiresIn);

public record V2RefreshValidationResponse(
    bool IsValid,
    Guid UserId,
    DateTime ExpiresAt);

public record V2CreateCategoryRequest(
    string Nome);

public record V2CategoryResult(
    Guid Id,
    string Nome,
    DateTime CriadaEm,
    DateTime? AtualizadaEm);

public record V2CategoryListResult(
    List<V2CategoryResult> Categorias);