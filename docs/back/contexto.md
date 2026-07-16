# Backend — Contexto do Projeto

## O que é

API REST em ASP.NET Core 8 que serve de backend para uma aplicação de gestão financeira pessoal (carteiras, transações, bolsa de valores). Usa autenticação JWT + Refresh Token via cookie HttpOnly.

## Objetivo

Permitir que um usuário:
- Gerencie múltiplas **carteiras** (corrente e investimento)
- Lance **transações** (receita, despesa, transferência entre carteiras)
- Registre **operações de bolsa** (compra/venda de ativos)
- Organize transações por **categorias** customizadas
- Autentique-se com segurança usando JWT de curta duração + refresh token rotativo

## Tecnologias

| Tech | Uso |
|---|---|
| ASP.NET Core 8 | Framework web |
| Entity Framework Core | ORM |
| PostgreSQL | Banco de dados |
| BCrypt | Hash de senha |
| JWT Bearer | Autenticação stateless |
| xUnit | Testes |

## Acesso rápido ao banco (Docker)

Com o ambiente em execução, para abrir o Postgres no terminal:

```powershell
docker compose exec postgres psql -U postgres -d authdb
```

Comandos úteis dentro do `psql`:

```sql
\dt         -- tabelas do schema atual
\dt *.*     -- tabelas de todos os schemas
\dn         -- schemas
\d users    -- estrutura de uma tabela
\q          -- sair
```

Se os valores no `.env` forem diferentes, substitua `postgres` e `authdb` pelos valores de `POSTGRES_USER` e `POSTGRES_DB`.

## Convenções Importantes

- **Versioning**: mudanças de contrato sempre na versão mais nova (atualmente V2). V1 existe apenas para compatibilidade.
- **Autenticação**: o `userId` vem sempre dos **claims** do JWT (`NameIdentifier` / `sub`), nunca do body ou query.
- **Namespace raiz**: `Auth.Domain` (domínio), `Application.Services` (serviços), `Infrastructure` (repos/dados).
- **Soft delete**: `User.DeletedAt` indica usuário removido.

## Fluxo de Versioning

```
V1 → deprecated
V2 → versão ativa, recebe novos endpoints e contratos
```

Hoje os controllers V1 de Auth, User e Category foram removidos; só existem rotas V2 para essas entidades.
