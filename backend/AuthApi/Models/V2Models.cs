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

public record V2PasswordResetCodeResponse(
    string ResetCode,
    DateTime ExpiresAt);

public record V2ResetCodeRequest(
    string Username);

public record V2ChangePasswordRequest(
    string Username,
    string ResetCode,
    string NewPassword);

public record V2ChangePasswordResponse(
    string Message);

public record V2CreateCategoryRequest(
    string Nome,
    string? IconKey,
    string? ColorHex);

public record V2CategoryResult(
    Guid Id,
    string Nome,
    string IconKey,
    string ColorHex,
    DateTime CriadaEm,
    DateTime? AtualizadaEm);

public record V2CategoryListResult(
    List<V2CategoryResult> Categorias);

public record V2CreateGoalRequest(
    string Nome,
    decimal ValorTotal,
    int Meses);

public record V2UpdateGoalRequest(
    string Nome,
    decimal ValorTotal,
    int Meses,
    Guid? CarteiraId,
    decimal? AporteManual);

public record V2GoalResult(
    Guid Id,
    string Nome,
    decimal ValorTotal,
    int Meses,
    decimal ValorMensal,
    decimal ValorAportado,
    decimal ValorRestante,
    decimal PercentualConcluido,
    bool UsaAporteManual,
    Guid? CarteiraId);

public record V2GoalListResult(
    List<V2GoalResult> Objetivos);
