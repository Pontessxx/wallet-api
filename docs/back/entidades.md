# Backend — Entidades e Enums

## Entidades (`Domain/Entities/`)

### `User`
```
Id              : Guid
Username        : string
PasswordHash    : string
ResetCodeHash   : string?
ResetCodeExpiresAt : DateTime?
ResetCodeFailedAttempts : int
CreatedAt       : DateTime
UpdatedAt       : DateTime?
DeletedAt       : DateTime?   ← soft delete
Role            : RoleUser
RefreshTokens   : ICollection<RefreshToken>
Categories      : ICollection<Category>
```

---

### `Carteira`
```
Id              : Guid
UserId          : Guid (FK → User)
Categoria       : WalletCategory
Nome            : string
SaldoInicial    : decimal
Receitas        : decimal      ← saldo agregado
Despesas        : decimal      ← saldo agregado
Transferencias  : decimal      ← saldo agregado
Saldo           : decimal      ← computado: SaldoInicial + Receitas - Despesas + Transferencias
SaldoProjetado  : decimal
TransacoesBolsa : ICollection<TransacaoBolsa>
TransferenciasSaida  : ICollection<TransferenciaCarteira>
TransferenciasEntrada: ICollection<TransferenciaCarteira>
```

> ⚠️ O `Saldo` é calculado pelo mapper, não armazenado diretamente como valor final.

---

### `TransacaoBase` (abstract)
Base compartilhada por todas as transações:
```
Id              : Guid
Valor           : decimal
Encargos        : decimal
ValorTotal      : decimal
Efetivada       : bool
DataLancamento  : DateTime
DataVencimento  : DateTime?
DataEfetivacao  : DateTime?
Observacoes     : string?
CriadaEm        : DateTime
AtualizadaEm    : DateTime?
```

---

### `Transacoes` : TransacaoBase
Receitas e Despesas comuns:
```
CarteiraId  : Guid (FK → Carteira)
Tipo        : TipoTransacoes   ← Despesa | Receita
CategoriaId : Guid?            ← FK → Category (nullable)
Carteira    : Carteira
Categoria   : Category?
```
> Tabela: `transactions`

---

### `TransacaoBolsa` : TransacaoBase
Operações de renda variável:
```
CarteiraId      : Guid (FK → Carteira)
CodigoAtivo     : string        ← ex: "PETR4"
Lado            : TipoTransacaoBolsa   ← Compra | Venda
Quantidade      : decimal
PrecoUnitario   : decimal
Carteira        : Carteira
```
> Tabela: `transactions_bolsa`

---

### `TransferenciaCarteira` : TransacaoBase
Movimentação entre carteiras do mesmo usuário:
```
CarteiraOrigemId   : Guid (FK → Carteira)
CarteiraDestinoId  : Guid (FK → Carteira)
CarteiraOrigem     : Carteira
CarteiraDestino    : Carteira
```
> Tabela: `transfers`

---

### `Category`
Categorias de transação criadas pelo usuário:
```
Id          : Guid
UserId      : Guid (FK → User)
Nome        : string
IconKey     : string   ← default: "tag"
ColorHex    : string   ← default: "#64748B"
CriadaEm   : DateTime
AtualizadaEm: DateTime?
Transacoes  : ICollection<Transacoes>
```
> Tabela: `categories`. Não pode ser deletada se houver transações vinculadas.

---

### `RefreshToken`
```
Id           : Guid
UserId       : Guid (FK → User)
Token        : string
CreatedAt    : DateTime
ExpiresAt    : DateTime
RevokedAt    : DateTime?
CreatedByIp  : string?
RevokedByIp  : string?
IsRevoked    : bool  (calculado: RevokedAt.HasValue)
IsExpired    : bool  (calculado: ExpiresAt <= UtcNow)
```

---

## Enums (`Domain/Enum/`)

### `WalletCategory`
```
Investimento
Corrente
```
> Enviado via header `X-WalletType` na criação de carteira.

---

### `TipoTransacoes`
```
Despesa
Receita
Transferencia
```

---

### `TipoTransacaoBolsa`
```
Compra
Venda
```
> Enviado via campo `lado` no body de operações bolsa.

---

### `CategoriaTransacao` (enum legado — substituído por FK)
```
Alimentacao | Moradia | Transporte | Saude | Educacao
Lazer | Salario | Investimentos | Impostos | Outros
```
> Este enum foi substituído pela entidade `Category` com FK. Mantido apenas como referência.

---

### `RoleUser`
```
User
Admin
```

---

### `TicketValidationType`
Controla o tipo de ticket gerado no login.
```
JwtOnly
```

---

## Diagrama de Relacionamentos (simplificado)

```
User ──< Carteira ──< Transacoes >── Category
                  ──< TransacaoBolsa
                  ──< TransferenciaCarteira >── Carteira (destino)
User ──< RefreshToken
User ──< Category
```
