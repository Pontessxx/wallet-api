# Wallet API + Wallet Web

Projeto de gestão financeira pessoal com:

- Backend em ASP.NET Core (AuthApi)
- Banco PostgreSQL
- Frontend em React + Vite

## Como rodar com Docker

### 1) Pré-requisitos

- Docker Desktop instalado e em execução
- Porta 3000 livre (frontend)
- Porta 8080 livre (backend)
- Porta 5433 livre (PostgreSQL)

### 2) Configurar variáveis de ambiente

Na raiz do projeto, crie o arquivo `.env` a partir do exemplo:

```powershell
Copy-Item .env.example .env
```

Se preferir, copie manualmente e ajuste os valores do arquivo `.env`.

### 3) Subir todos os serviços

Ainda na raiz do projeto:

```powershell
docker compose up --build -d
```

Isso sobe:

- `postgres` em `localhost:5433`
- `backend` em `http://localhost:8080`
- `frontend` em `http://localhost:3000`

### 4) Acessar a aplicação

- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:8080`
- Swagger (se habilitado): `http://localhost:8080/swagger`

### 5) Ver logs

```powershell
docker compose logs -f backend
docker compose logs -f postgres
docker compose logs -f frontend
```

### 6) Acessar o PostgreSQL e ver tabelas

Com os containers em execução, entre no `psql` do Postgres:

```powershell
docker compose exec postgres psql -U postgres -d authdb
```

Se você alterou o arquivo `.env`, use os valores de `POSTGRES_USER` e `POSTGRES_DB` no comando acima.

Dentro do `psql`, use:

```sql
\dt         -- lista tabelas do schema atual
\dt *.*     -- lista tabelas de todos os schemas
\dn         -- lista schemas
\d users    -- descreve uma tabela (troque pelo nome desejado)
\q          -- sai do psql
```

### 7) Parar o ambiente

```powershell
docker compose down
```

Para remover também o volume do banco:

```powershell
docker compose down -v
```

## Como é o projeto de carteira (Backend)

O backend segue arquitetura em camadas:

```text
backend/
├── Domain/         # Entidades e enums de domínio (sem dependências externas)
├── Application/    # Regras de negócio, serviços e interfaces
├── Infrastructure/ # EF Core, repositórios, migrações e segurança (JWT/hash)
└── AuthApi/        # API ASP.NET (controllers, configuração HTTP, DI e startup)
```

### Papel de cada camada

- `Domain`: representa o núcleo da regra de negócio (entidades como usuário, carteira, transações).
- `Application`: contém serviços de negócio e contratos (interfaces).
- `Infrastructure`: implementa persistência e segurança (PostgreSQL, repositórios, tokens, hash de senha).
- `AuthApi`: expõe os endpoints REST e faz o bootstrap da aplicação.

### Fluxo de inicialização do backend

No startup (`Program.cs`), a API:

1. Configura controllers, CORS e OpenAPI.
2. Registra `Application` e `Infrastructure` no DI.
3. Aplica migrations automaticamente no banco.
4. Executa seed inicial (usuário admin via variáveis de ambiente).
5. Habilita autenticação JWT e autorização.

### Módulos de endpoint

- Auth (V2): login, registro, refresh token, validação, logout, reset de senha.
- User (V2): perfil, edição de dados e senha, remoção de conta.
- Category (V2): CRUD de categorias do usuário.
- Wallet (V1): carteiras e resumo de saldo.
- Transaction (V1): lançamentos, transferências e operações de bolsa.

## Rodar backend localmente (sem Docker)

Se quiser executar só a API:

```powershell
cd backend
dotnet restore AuthApi.slnx
dotnet run --project AuthApi/AuthApi.csproj
```

Observações:

- O backend espera PostgreSQL configurado na connection string.
- Se usar banco local fora do Docker, ajuste as variáveis/`appsettings`.

## Comandos úteis

```powershell
# Build da solução backend
dotnet build backend/AuthApi.slnx

# Testes
dotnet test backend/test/AuthApi.Tests/AuthApi.Tests.csproj
dotnet test backend/test/Domain.Tests/Domain.Tests.csproj
dotnet test backend/test/Infrastructure.Tests/Infrastructure.Tests.csproj
```
