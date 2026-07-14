# Frontend — Serviços e Camada de API

## Instâncias Axios (`api/api.ts`)

### `publicApi`
- Sem token de autorização.
- Interceptor de erro: converte erros de rede em mensagem amigável (`Não foi possível conectar ao servidor.`).
- Usado por: `authService` (login, register, refresh).

### `privateApi`
- **Request interceptor**: injeta `Authorization: Bearer <token>` lendo `sessionStorage.getItem('accessToken')`.
- **Response interceptor**: ao receber 401, tenta renovar o token via `/auth/v2/refresh`.
  - Usa fila `pendingQueue` para não fazer múltiplas chamadas de refresh simultaneamente.
  - Após renovar, reprocessa todos os requests que falharam com 401.
- Usado por: todos os services de domínio autenticado.

### Cookie `withCredentials: true`
Ambas as instâncias têm `withCredentials: true` para que o browser envie/receba cookies `HttpOnly` (necessário para o refresh token funcionar).

---

## Services

### `authService.ts`

| Função | Método | Rota | Instância |
|---|---|---|---|
| `login(username, password)` | POST | `/auth/v2/login` | `publicApi` |
| `register(username, password)` | POST | `/auth/v2/register` | `publicApi` |
| `refresh()` | POST | `/auth/v2/refresh` | `publicApi` |
| `validate()` | GET | `/auth/v2/validate` | `privateApi` |
| `logout()` | DELETE | `/auth/v2/logout` | `privateApi` |
| `resetCode(username)` | POST | `/auth/v2/reset-code` | `publicApi` |
| `changePassword(...)` | POST | `/auth/v2/change-password` | `publicApi` |

---

### `carteiraService.ts`

| Função | Método | Rota |
|---|---|---|
| `list()` | GET | `/wallet/v1/accounts` |
| `summary()` | GET | `/wallet/v1/summary` |
| `create(data, tipo)` | POST | `/wallet/v1/accounts/create` + header `X-WalletType` |
| `update(data)` | PUT | `/wallet/v1/accounts/update` |
| `remove(id)` | DELETE | `/wallet/v1/accounts/delete?id=` |

Todas as funções usam `privateApi`.

---

### `categoriaService.ts`

| Função | Método | Rota |
|---|---|---|
| `list()` | GET | `/category/v2/list` |
| `create(data)` | POST | `/category/v2/new` |
| `remove(id)` | DELETE | `/category/v2/remove?id=` |

> ⚠️ `list()` normaliza a resposta: a API retorna `{ categorias: [...] }` (ou PascalCase) — o service mapeia para um array simples.
> ⚠️ `remove()` usa `id` em **query string**, não no body.

---

### `userService.ts`

| Função | Método | Rota |
|---|---|---|
| `me()` | GET | `/user/v2/me` |
| `edit(data)` | PUT | `/user/v2/edit` |
| `editPassword(data)` | PUT | `/user/v2/edit-password` |
| `remove()` | DELETE | `/user/v2/remove` |

---

## Armazenamento de Token

| Item | Local | Motivo |
|---|---|---|
| `accessToken` | `sessionStorage` | Limpo ao fechar a aba/browser |
| `refreshToken` | Cookie `HttpOnly` | Inacessível ao JS — segurança contra XSS |

---

## Mock com MSW

Quando `VITE_ENABLE_MSW=true`, os handlers em `mocks/` interceptam as chamadas antes de chegarem ao Axios, permitindo desenvolvimento sem backend.

O service worker é registrado no `main.tsx` antes da montagem do React.
O arquivo `public/mockServiceWorker.js` é o service worker gerado pelo MSW.
