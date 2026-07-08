namespace AuthApi.Models;

public record LoginRequest(
    string Username,
    string Password);

public record LoginResponse(
    Guid Id,
    string Token,
    string Role,
    string Username
    );

public record ChangePasswordResponse(
    string Message);

public record PasswordResetCodeResponse(
    string ResetCode,
    DateTime ExpiresAt);

public record ResetCodeRequest(
    string Username);

public record ChangePasswordRequest(
    string Username,
    string ResetCode,
    string NewPassword);

public record CreateUserRequest(
    string Username,
    string Password
    );

public record UserResponse(
    Guid Id,
    string Username,
    string Role);

public record CreateCarteiraRequest(
    string Nome,
    decimal SaldoInicial);

public record EditCarteiraRequest(
    Guid Id,
    string Nome,
    WalletCategory Categoria);

public record RemoveCarteiraRequest(Guid Id);

public abstract record TransactionUpsertRequest(
    Guid CarteiraId,
    Guid? CarteiraDestinoId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes);

public record CreateTransactionRequest(
    Guid CarteiraId,
    Guid? CarteiraDestinoId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes)
    : TransactionUpsertRequest(
        CarteiraId,
        CarteiraDestinoId,
        Valor,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes);

public record UpdateTransactionRequest(
    Guid CarteiraId,
    Guid? CarteiraDestinoId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes)
    : TransactionUpsertRequest(
        CarteiraId,
        CarteiraDestinoId,
        Valor,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes);

public abstract record TransactionBaseResult(
    Guid Id,
    Guid CarteiraId,
    decimal Valor,
    decimal Encargos,
    decimal ValorTotal,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    DateTime CriadaEm,
    DateTime? AtualizadaEm);

public record TransactionResult(
    Guid Id,
    Guid CarteiraId,
    Guid? CarteiraDestinoId,
    TipoTransacoes Tipo,
    decimal Valor,
    decimal Encargos,
    decimal ValorTotal,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    DateTime CriadaEm,
    DateTime? AtualizadaEm)
    : TransactionBaseResult(
        Id,
        CarteiraId,
        Valor,
        Encargos,
        ValorTotal,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes,
        CriadaEm,
        AtualizadaEm);

public record TransactionHistoryResult(
    List<TransactionResult> Transacoes);

public abstract record ExchangeUpsertRequest(
    Guid CarteiraId,
    string CodigoAtivo,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes);

public record CreateExchangeRequest(
    Guid CarteiraId,
    string CodigoAtivo,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes)
    : ExchangeUpsertRequest(
        CarteiraId,
        CodigoAtivo,
        Quantidade,
        PrecoUnitario,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes);

public record UpdateExchangeRequest(
    Guid CarteiraId,
    string CodigoAtivo,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes)
    : ExchangeUpsertRequest(
        CarteiraId,
        CodigoAtivo,
        Quantidade,
        PrecoUnitario,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes);

public record ExchangeTransactionResult(
    Guid Id,
    Guid CarteiraId,
    string CodigoAtivo,
    TipoTransacaoBolsa Lado,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal Valor,
    decimal Encargos,
    decimal ValorTotal,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    DateTime CriadaEm,
    DateTime? AtualizadaEm)
    : TransactionBaseResult(
        Id,
        CarteiraId,
        Valor,
        Encargos,
        ValorTotal,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes,
        CriadaEm,
        AtualizadaEm);

public record ExchangeHistoryResult(
    List<ExchangeTransactionResult> Transacoes);