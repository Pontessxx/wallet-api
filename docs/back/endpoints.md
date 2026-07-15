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
| POST | `/transaction/v1/new` | ✅ | Cria transferência entre carteiras |
| PUT | `/transaction/v1/edit?id=` | ✅ | Atualiza transferência |

> No estado atual do código, o controller `transaction/v1` está dedicado ao fluxo de transferência entre carteiras.

---

## Transfer (`/transfer/v1/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/transfer/v1/list?id=` | ✅ | Busca transação de receita/despesa por ID |
| POST | `/transfer/v1/new` | ✅ | Cria lançamento (Receita/Despesa) |
| PUT | `/transfer/v1/edit?id=` | ✅ | Atualiza lançamento |
| GET | `/transfer/v1/history` | ✅ | Histórico com filtro opcional `?tipo=` |
| DELETE | `/transfer/v1/remove?id=` | ✅ | Remove lançamento |

---

## Exchange (`/exchange/v1/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/exchange/v1/list?id=` | ✅ | Busca operação de bolsa por ID |
| POST | `/exchange/v1/new` | ✅ | Cria operação de bolsa |
| PUT | `/exchange/v1/edit?id=` | ✅ | Atualiza operação de bolsa |
| GET | `/exchange/v1/history` | ✅ | Histórico com filtro opcional `?lado=` |
| DELETE | `/exchange/v1/remove?id=` | ✅ | Remove operação de bolsa |

---

## Contrato Fechado — Filtros de Relatório (GET)

> Objetivo: suportar filtros por período no frontend (datepicker) com datas sem hora (`YYYY-MM-DD`).
> Recomendação: manter em query params (GET), sem payload/body.
> Regra de versionamento: a implementação desses filtros deve ser feita na V2. A V1 deve permanecer apenas para compatibilidade/depreciação.

### Versão alvo (obrigatória)

- Implementar em rotas V2 (ex.: `/transfer/v2/history` e `/exchange/v2/history`, ou endpoint consolidado de relatório em `/report/v2/...`).
- Não evoluir contrato novo em V1.

### Endpoints de histórico

- Transfer: `/transfer/v1/history`
- Exchange: `/exchange/v1/history`

### Parâmetros por `periodType`

| `periodType` | Obrigatórios | Opcionais |
|---|---|---|
| `range` | `startDate`, `endDate` | `tipo`, `categoriaId` (transfer), `lado` (exchange) |
| `monthly` | `year`, `month` | `tipo`, `categoriaId` (transfer), `lado` (exchange) |
| `yearly` | `year` | `tipo`, `categoriaId` (transfer), `lado` (exchange) |

### Definição dos parâmetros

| Parâmetro | Tipo | Formato | Regra |
|---|---|---|---|
| `periodType` | string | `range \| monthly \| yearly` | Obrigatório |
| `startDate` | string | `YYYY-MM-DD` | Obrigatório quando `periodType=range` |
| `endDate` | string | `YYYY-MM-DD` | Obrigatório quando `periodType=range` |
| `year` | number | `YYYY` | Obrigatório quando `periodType=monthly` ou `yearly` |
| `month` | number | `1..12` | Obrigatório quando `periodType=monthly` |
| `tipo` | string | `Receita \| Despesa` | Opcional em `/transfer/v1/history` |
| `categoriaId` | string | `GUID` | Opcional em `/transfer/v1/history` |
| `lado` | string | `Compra \| Venda` | Opcional em `/exchange/v1/history` |

### Exemplos de URL

Transfer:

- `/transfer/v1/history?periodType=range&startDate=2026-07-01&endDate=2026-07-31`
- `/transfer/v1/history?periodType=monthly&year=2026&month=7&tipo=Despesa&categoriaId=11111111-1111-1111-1111-111111111111`
- `/transfer/v1/history?periodType=yearly&year=2026&tipo=Receita&categoriaId=11111111-1111-1111-1111-111111111111`

Exchange:

- `/exchange/v1/history?periodType=range&startDate=2026-07-01&endDate=2026-07-31`
- `/exchange/v1/history?periodType=monthly&year=2026&month=7&lado=Compra`
- `/exchange/v1/history?periodType=yearly&year=2026&lado=Venda`

### Formato de resposta

Transfer:

```json
{
	"transacoes": [
		{
			"id": "guid",
			"carteiraId": "guid",
			"carteiraDestinoId": null,
			"tipo": "Receita",
			"categoriaId": "guid",
			"categoriaNome": "Salario",
			"valor": 1000.0,
			"encargos": 0.0,
			"valorTotal": 1000.0,
			"efetivada": true,
			"dataLancamento": "2026-07-10T00:00:00",
			"dataVencimento": null,
			"dataEfetivacao": "2026-07-10T00:00:00",
			"observacoes": null,
			"criadaEm": "2026-07-10T12:00:00",
			"atualizadaEm": null
		}
	]
}
```

Exchange:

```json
{
	"transacoes": [
		{
			"id": "guid",
			"carteiraId": "guid",
			"codigoAtivo": "PETR4",
			"lado": "Compra",
			"quantidade": 10.0,
			"precoUnitario": 30.5,
			"valor": 305.0,
			"encargos": 1.5,
			"valorTotal": 306.5,
			"efetivada": true,
			"dataLancamento": "2026-07-10T00:00:00",
			"dataVencimento": null,
			"dataEfetivacao": "2026-07-10T00:00:00",
			"observacoes": null,
			"criadaEm": "2026-07-10T12:00:00",
			"atualizadaEm": null
		}
	]
}
```

### Regras de validação

- Datas sem hora: `YYYY-MM-DD`.
- `range`: exige `startDate` e `endDate`.
- `monthly`: exige `year` e `month`.
- `yearly`: exige `year`.
- `startDate` não pode ser maior que `endDate`.
- `month` fora de `1..12` retorna `400`.
- `tipo`/`lado` inválidos retornam `400`.
- `categoriaId` inválido (não GUID) retorna `400`.
- Regra recomendada no backend: início inclusivo e fim exclusivo (`endDate + 1 dia`).

---

## Notas de Contrato

- `userId` nunca é enviado pelo cliente — vem sempre dos **claims** do JWT.
- Responses de erro seguem o modelo `ResponseError` (`{ title, detail, status }`).
- `accessToken` armazenado em `sessionStorage` no frontend.
- `refreshToken` viaja apenas via cookie `HttpOnly` — nunca exposto ao JS.
