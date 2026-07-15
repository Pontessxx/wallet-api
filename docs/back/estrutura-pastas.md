# Backend — Estrutura de Pastas

## Solução: `backend/AuthApi.slnx`

```
backend/
├── Domain/             ← entidades e enums puros, zero dependência externa
├── Application/        ← serviços de negócio e interfaces (contratos)
├── Infrastructure/     ← EF Core, repositórios, migrações, segurança
├── AuthApi/            ← ASP.NET app: controllers, middlewares, configurações
└── test/
    ├── Domain.Tests/
    ├── Infrastructure.Tests/
    └── AuthApi.Tests/
```

---

## `Domain/`

Camada mais interna. **Zero dependência** de pacotes externos.

| Subpasta | Conteúdo |
|---|---|
| `Entities/` | Classes de domínio (User, Carteira, Transacoes, etc.) |
| `Enum/` | Enumeradores de domínio (TipoTransacoes, WalletCategory, etc.) |

---

## `Application/`

Contém a **lógica de negócio** e os **contratos** (interfaces).

| Subpasta | Conteúdo |
|---|---|
| `Interfaces/` | Contratos de repositório e serviço (IUserRepository, ITokenService, etc.) |
| `Services/` | Implementação dos serviços (AuthV2Service, UserService, ContaCarteiraService, etc.) |
| `Mappers/` | Mapeamentos de entidade → modelo de resposta (CarteiraMapper) |
| `Models/` | DTOs/models compartilhados entre serviços (WalletModels) |
| `DependencyInjection.cs` | Registro dos serviços no DI container |

---

## `Infrastructure/`

Camada de **dados e segurança**.

| Subpasta | Conteúdo |
|---|---|
| `Data/` | `ApplicationDbContext` (EF Core) — mapeamento das tabelas |
| `Repositories/` | Implementações concretas dos repositórios |
| `Migrations/` | Migrations do EF Core |
| `Security/` | Implementações de segurança (hash, JWT, refresh token) |
| `DependencyInjection.cs` | Registro de repositórios e contexto no DI |

---

## `AuthApi/`

**Projeto de entrada** — startup, controllers, configuração HTTP.

| Subpasta | Conteúdo |
|---|---|
| `Controllers/V1/` | Controllers legados (deprecated) |
| `Controllers/V2/` | Controllers ativos: Auth, User, Category |
| `Models/` | Request/Response models da API (V2Models, Models, ResponseError) |
| `Mappers/` | TransactionMapper (entidade ↔ response) |
| `Extensions/` | OpenAPI config, filtros de autorização, error handling |
| `Properties/` | launchSettings.json |
| `Program.cs` | Bootstrap da aplicação |
| `appsettings.json` | Configurações (JWT secret, DB connection, etc.) |

---

## Tabelas no Banco (PostgreSQL)

| Tabela | Entidade |
|---|---|
| `users` | User |
| `wallets` | Carteira |
| `transactions` | Transacoes |
| `transactions_bolsa` | TransacaoBolsa |
| `transfers` | TransferenciaCarteira |
| `refresh_tokens` | RefreshToken |
| `categories` | Category |

---

## Comando de Build

```bash
dotnet build backend/AuthApi.slnx
```

## Migrações

```bash
# verificar pendências
dotnet ef migrations has-pending-model-changes \
  --project backend/Infrastructure/Infrastructure.csproj \
  --startup-project backend/AuthApi/AuthApi.csproj

# aplicar
dotnet ef database update \
  --project backend/Infrastructure/Infrastructure.csproj \
  --startup-project backend/AuthApi/AuthApi.csproj
```
