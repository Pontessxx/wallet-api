# Backend — Endpoints da API

> Todos os endpoints ativos estão na versão **V2**. V1 está deprecated.
> Base URL configurada em `VITE_API_URL` no frontend / `appsettings.json` no backend.

---

## Auth (`/auth/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/auth/v2/login` | ❌ | Login → retorna accessToken + seta cookie refreshToken |
| POST | `/auth/v2/register` | ❌ | Registro de novo usuário |
| POST | `/auth/v2/refresh` | ❌ (cookie) | Renova accessToken usando refreshToken do cookie |
| GET | `/auth/v2/validate` | ✅ | Valida se o accessToken ainda é válido |
| DELETE | `/auth/v2/logout` | ✅ | Limpa cookie e revoga todos os refreshTokens ativos |
| POST | `/auth/v2/reset-code` | ❌ | Gera código de redefinição de senha |
| POST | `/auth/v2/change-password` | ❌ | Troca senha usando o reset code |

---

## User (`/user/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/user/v2/me` | ✅ | Retorna dados do usuário autenticado |
| PUT | `/user/v2/edit` | ✅ | Edita username |
| PUT | `/user/v2/edit-password` | ✅ | Troca senha (autenticado) |
| DELETE | `/user/v2/remove` | ✅ | Remove conta (soft delete) |

---

## Category (`/category/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/category/v2/list` | ✅ | Lista categorias do usuário → `{ categorias: [...] }` |
| POST | `/category/v2/new` | ✅ | Cria nova categoria |
| DELETE | `/category/v2/remove?id=` | ✅ | Remove categoria (bloqueado se houver transações vinculadas) |

---

## Wallet (`/wallet/v1/`)

> Ainda na V1, sem remoção prevista pois não tem contrato quebrado.

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/wallet/v1/accounts` | ✅ | Lista carteiras → `{ carteiras: [...] }` |
| GET | `/wallet/v1/summary` | ✅ | Resumo com `saldoTotal` + carteiras |
| POST | `/wallet/v1/accounts/create` | ✅ | Cria carteira; tipo via header `X-WalletType` |
| PUT | `/wallet/v1/accounts/update` | ✅ | Atualiza carteira |
| DELETE | `/wallet/v1/accounts/delete?id=` | ✅ | Remove carteira |

---

## Transaction (`/transaction/v1/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/transaction/v1/history` | ✅ | Histórico; filtro opcional `?tipo=` |
| POST | `/transaction/v1/lancamento` | ✅ | Cria Receita ou Despesa; `tipo` no body |
| PUT | `/transaction/v1/lancamento` | ✅ | Atualiza lançamento |
| DELETE | `/transaction/v1/lancamento?id=` | ✅ | Remove lançamento |
| POST | `/transaction/v1/transfer` | ✅ | Cria transferência entre carteiras |
| PUT | `/transaction/v1/transfer` | ✅ | Atualiza transferência |
| DELETE | `/transaction/v1/transfer?id=` | ✅ | Remove transferência |
| POST | `/transaction/v1/exchange` | ✅ | Cria operação de bolsa |
| PUT | `/transaction/v1/exchange` | ✅ | Atualiza operação de bolsa |
| DELETE | `/transaction/v1/exchange?id=` | ✅ | Remove operação de bolsa |

> Operação de bolsa usa header `X-TipoTransacaoBolsa` (Compra/Venda).

---

## Notas de Contrato

- `userId` nunca é enviado pelo cliente — vem sempre dos **claims** do JWT.
- Responses de erro seguem o modelo `ResponseError` (`{ title, detail, status }`).
- `accessToken` armazenado em `sessionStorage` no frontend.
- `refreshToken` viaja apenas via cookie `HttpOnly` — nunca exposto ao JS.
