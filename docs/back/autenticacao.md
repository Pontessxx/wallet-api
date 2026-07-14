# Backend — Autenticação

## Visão Geral

O sistema usa **dois tokens**:

| Token | Onde fica | Duração | Renovável |
|---|---|---|---|
| `accessToken` (JWT) | `sessionStorage` do browser | 15 minutos | Não, mas renovado via refresh |
| `refreshToken` | Cookie `HttpOnly` (SameSite=Strict) | Longa duração | Rotativo |

---

## Fluxo de Login

```
1. POST /auth/v2/login { username, password }
2. Backend valida credenciais
3. Gera accessToken JWT (15min) + refreshToken (persistido no BD)
4. Resposta: { accessToken, user } + Set-Cookie: refreshToken=xxx; HttpOnly
5. Frontend salva accessToken em sessionStorage
```

---

## Fluxo de Refresh

```
1. Request autenticado → 401 (accessToken expirado)
2. Frontend detecta 401 via interceptor do Axios (privateApi)
3. POST /auth/v2/refresh (cookie é enviado automaticamente pelo browser)
4. Backend valida refreshToken, revoga o antigo, emite novo par
5. Frontend atualiza accessToken e reprocessa a fila de requests pendentes
```

> O interceptor em `api.ts` usa uma fila (`pendingQueue`) para não disparar múltiplos refreshes simultâneos.

---

## Fluxo de Logout

```
1. DELETE /auth/v2/logout (com accessToken no header)
2. Backend revoga todos os refreshTokens ativos do usuário
3. Apaga o cookie refreshToken
4. accessToken continua válido por até 15min (stateless — sem blacklist)
5. Frontend limpa sessionStorage e redireciona para /login
```

---

## JWT Claims

| Claim | Valor |
|---|---|
| `sub` / `NameIdentifier` | `user.Id` (Guid) |
| `name` | `user.Username` |
| `role` | `user.Role` (User/Admin) |
| `exp` | expiração (15 min) |

O `userId` é extraído dos claims em **todos** os controllers autenticados.

---

## Reset de Senha

```
1. POST /auth/v2/reset-code { username } → gera hash do código, salva em User.ResetCodeHash
2. (O código é retornado na response para fins de dev — em prod seria enviado por e-mail)
3. POST /auth/v2/change-password { username, resetCode, newPassword }
4. Valida código, aplica nova senha, limpa ResetCodeHash
```

- Máximo de tentativas erradas: controlado por `User.ResetCodeFailedAttempts`
- Expiração do código: `User.ResetCodeExpiresAt`
- Validação delegada a `IResetCodeValidator`

---

## Segurança de Senhas

- Hash com **BCrypt** via `IPasswordHasher`
- Implementado em `Infrastructure/Security/`

---

## Tokens no Banco

A tabela `refresh_tokens` guarda:
- O token em si (hash ou valor opaco)
- IP de criação/revogação
- Data de expiração e revogação

Um usuário pode ter múltiplos refresh tokens ativos (multi-device). O logout revoga **todos**.
