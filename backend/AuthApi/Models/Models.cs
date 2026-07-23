namespace AuthApi.Models;

public record LoginRequest(
    string Username,
    string Password);

public record LoginResponse(
    Guid Id,
    string Token,
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
    string Username);

public record EditUserRequest(
    Guid Id,
    string Username);

public record EditPasswordRequest(
    string CurrentPassword,
    string NewPassword);

public record CreateCarteiraRequest(
    string Nome,
    decimal SaldoInicial,
    WalletOrigin Origem);

public record EditCarteiraRequest(
    Guid Id,
    string Nome,
    WalletCategory Categoria,
    WalletOrigin Origem);

public record RemoveCarteiraRequest(Guid Id);

public abstract record TransferUpsertRequest(
    Guid CarteiraId,
    Guid CarteiraDestinoId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    decimal? TaxaCambio);

public record CreateTransferRequest(
    Guid CarteiraId,
    Guid CarteiraDestinoId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    decimal? TaxaCambio)
    : TransferUpsertRequest(
        CarteiraId,
        CarteiraDestinoId,
        Valor,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes,
        TaxaCambio);

public record UpdateTransferRequest(
    Guid CarteiraId,
    Guid CarteiraDestinoId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    decimal? TaxaCambio)
    : TransferUpsertRequest(
        CarteiraId,
        CarteiraDestinoId,
        Valor,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes,
        TaxaCambio);

public abstract record EntryUpsertRequest(
    Guid CarteiraId,
    TipoTransacoes Tipo,
    Guid CategoriaId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    Guid? ObjetivoId);

public record CreateEntryRequest(
    Guid CarteiraId,
    TipoTransacoes Tipo,
    Guid CategoriaId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    Guid? ObjetivoId)
    : EntryUpsertRequest(
        CarteiraId,
        Tipo,
        CategoriaId,
        Valor,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes,
        ObjetivoId);

public record UpdateEntryRequest(
    Guid CarteiraId,
    TipoTransacoes Tipo,
    Guid CategoriaId,
    decimal Valor,
    decimal Encargos,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    Guid? ObjetivoId)
    : EntryUpsertRequest(
        CarteiraId,
        Tipo,
        CategoriaId,
        Valor,
        Encargos,
        Efetivada,
        DataLancamento,
        DataVencimento,
        DataEfetivacao,
        Observacoes,
        ObjetivoId);

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
    Guid? CategoriaId,
    string? CategoriaNome,
    decimal Valor,
    decimal Encargos,
    decimal ValorTotal,
    bool Efetivada,
    DateTime DataLancamento,
    DateTime? DataVencimento,
    DateTime? DataEfetivacao,
    string? Observacoes,
    DateTime CriadaEm,
    DateTime? AtualizadaEm,
    Guid? ObjetivoId,
    decimal? TaxaCambio = null,
    decimal? ValorConvertido = null)
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

public record CreateCategoryRequest(
    string Nome);

public record CategoryResult(
    Guid Id,
    string Nome,
    DateTime CriadaEm,
    DateTime? AtualizadaEm);

public record CategoryListResult(
    List<CategoryResult> Categorias);

public abstract record ExchangeUpsertRequest(
    Guid CarteiraId,
    TipoTransacaoBolsa? Lado,
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
    TipoTransacaoBolsa? Lado,
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
        Lado,
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
    TipoTransacaoBolsa? Lado,
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
        Lado,
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