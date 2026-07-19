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

## Goal (`/goal/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/goal/v2/list?id=&carteiraId=&periodType=&startDate=&endDate=&year=&month=` | ✅ | Lista objetivos do usuário (filtros opcionais via query, incluindo período) |
| POST | `/goal/v2/new?carteiraId=` | ✅ | Cria objetivo; `carteiraId` opcional em query |
| PUT | `/goal/v2/edit?id=` | ✅ | Atualiza objetivo |
| DELETE | `/goal/v2/remove?id=` | ✅ | Remove objetivo |
| POST | `/goal/v2/aporte/new?id=` | ✅ | Registra um depósito avulso no histórico do objetivo (só sem carteira vinculada) |
| GET | `/goal/v2/aporte/list?id=` | ✅ | Lista o histórico de depósitos do objetivo |
| DELETE | `/goal/v2/aporte/remove?id=` | ✅ | Remove um depósito do histórico; `id` é o id do depósito (aporte), não do objetivo |

### Regras principais de Objetivo

- No create (`POST /goal/v2/new`): `carteiraId` opcional na query; não vai no payload.
- No payload do create: enviar `nome`, `valorTotal`, `meses` e `iconKey` (opcional; ver lista
  de ícones permitidos abaixo, default `target`).
- Se `carteiraId` for enviado, a carteira precisa pertencer ao usuário autenticado.
- Parcela mensal ideal (`valorMensal`) é recalculada a cada leitura (`list`), não persistida
  como valor fixo: `valorMensal = valorRestante / mesesRestantes` (arredondado para 2 casas).
  `mesesRestantes` usa o mesmo critério de calendário do frontend (ano/mês da data-alvo
  `criadaEm + meses` menos ano/mês atual, mínimo 1). Isso faz a parcela cair quando o usuário
  deposita e subir conforme o tempo passa sem depósito.
- Com carteira vinculada: progresso usa saldo atual da carteira (`valorAportado`).
- Sem carteira vinculada: progresso usa a soma dos depósitos registrados via
  `POST /goal/v2/aporte/new` (histórico completo, tabela `goal_contributions`).
- `PUT /goal/v2/edit` ainda aceita o campo opcional `aporteManual` (soma direta, sem
  registro no histórico) por compatibilidade, mas a via recomendada para novos
  depósitos é `POST /goal/v2/aporte/new`, que grava valor/data/observação/recorrente
  e é retornado depois por `GET /goal/v2/aporte/list?id=`.
- `POST /goal/v2/aporte/new` retorna 400 se o objetivo tiver `carteiraId` (aporte é
  automático via saldo da carteira nesse caso).
- `DELETE /goal/v2/aporte/remove?id=` remove o depósito e subtrai o valor de
  `AporteManualAcumulado` do objetivo (mínimo 0), retornando o objetivo atualizado.
- Ícones permitidos (`iconKey`): `target`, `plane`, `graduation-cap`, `footprints`,
  `watch`, `home`, `car`, `gift`, `piggy-bank`, `heart`, `laptop`, `smartphone`,
  `camera`, `book-open`, `briefcase`, `dumbbell`, `gamepad-2`, `umbrella`, `star`,
  `wallet`.
- `V2GoalResult` agora também retorna `iconKey`, `carteiraNome` (nome da carteira
  vinculada, se houver) e `criadaEm` (usado pelo frontend para calcular a data-alvo
  como `criadaEm + meses`).

---

## Wallet (`/wallet/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/wallet/v2/accounts?categoria=` | ✅ | Lista carteiras do usuário com filtro opcional por categoria |
| GET | `/wallet/v2/summary?categoria=&periodType=&startDate=&endDate=&year=&month=` | ✅ | Resumo das carteiras com filtro opcional por categoria e período |

---

## Transfer (`/transfer/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/transfer/v2/list?id=` | ✅ | Busca transação de receita/despesa por ID |
| GET | `/transfer/v2/history?periodType=&startDate=&endDate=&year=&month=&tipo=&categoriaId=` | ✅ | Histórico com filtros por período + tipo/categoria |

---

## Exchange (`/exchange/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/exchange/v2/list?id=` | ✅ | Busca operação de bolsa por ID |
| GET | `/exchange/v2/history?periodType=&startDate=&endDate=&year=&month=&lado=` | ✅ | Histórico com filtros por período + lado |

---

## Wallet (`/wallet/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/wallet/v2/accounts?categoria=` | ✅ | Lista carteiras com filtro opcional por categoria |
| GET | `/wallet/v2/summary?categoria=&periodType=&startDate=&endDate=&year=&month=` | ✅ | Resumo com filtro opcional por categoria e período |
| POST | `/wallet/v2/accounts/create` | ✅ | Cria carteira; tipo via header `X-WalletType` |
| PUT | `/wallet/v2/accounts/edit` | ✅ | Atualiza carteira |
| DELETE | `/wallet/v2/accounts/remove` | ✅ | Remove carteira via body `{ id }` |

---

## Transaction (`/transaction/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/transaction/v2/new` | ✅ | Cria transferência entre carteiras |
| PUT | `/transaction/v2/edit?id=` | ✅ | Atualiza transferência |

---

## Transfer (`/transfer/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/transfer/v2/list?id=` | ✅ | Busca transação de receita/despesa por ID |
| POST | `/transfer/v2/new` | ✅ | Cria lançamento (Receita/Despesa) |
| PUT | `/transfer/v2/edit?id=` | ✅ | Atualiza lançamento |
| GET | `/transfer/v2/history?periodType=&startDate=&endDate=&year=&month=&tipo=&categoriaId=` | ✅ | Histórico com filtros por período/tipo/categoria |
| DELETE | `/transfer/v2/remove?id=` | ✅ | Remove lançamento |

---

## Exchange (`/exchange/v2/`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/exchange/v2/list?id=` | ✅ | Busca operação de bolsa por ID |
| POST | `/exchange/v2/new` | ✅ | Cria operação de bolsa |
| PUT | `/exchange/v2/edit?id=` | ✅ | Atualiza operação de bolsa |
| GET | `/exchange/v2/history?periodType=&startDate=&endDate=&year=&month=&lado=` | ✅ | Histórico com filtros por período/lado |
| DELETE | `/exchange/v2/remove?id=` | ✅ | Remove operação de bolsa |

---

## Contrato Fechado — Filtros de Relatório (GET)

> Objetivo: suportar filtros por período no frontend (datepicker) com datas sem hora (`YYYY-MM-DD`).
> Recomendação: manter em query params (GET), sem payload/body.
> Regra de versionamento: a implementação desses filtros deve ser feita na V2. A V1 deve permanecer apenas para compatibilidade/depreciação.

### Versão alvo (obrigatória)

- Implementar em rotas V2 (ex.: `/transfer/v2/history` e `/exchange/v2/history`, ou endpoint consolidado de relatório em `/report/v2/...`).
- Não evoluir contrato novo em V1.

### Endpoints de histórico

- Transfer: `/transfer/v2/history`
- Exchange: `/exchange/v2/history`

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
| `tipo` | string | `Receita \| Despesa` | Opcional em `/transfer/v2/history` |
| `categoriaId` | string | `GUID` | Opcional em `/transfer/v2/history` |
| `lado` | string | `Compra \| Venda` | Opcional em `/exchange/v2/history` |

### Exemplos de URL

Transfer:

- `/transfer/v2/history?periodType=range&startDate=2026-07-01&endDate=2026-07-31`
- `/transfer/v2/history?periodType=monthly&year=2026&month=7&tipo=Despesa&categoriaId=11111111-1111-1111-1111-111111111111`
- `/transfer/v2/history?periodType=yearly&year=2026&tipo=Receita&categoriaId=11111111-1111-1111-1111-111111111111`

Exchange:

- `/exchange/v2/history?periodType=range&startDate=2026-07-01&endDate=2026-07-31`
- `/exchange/v2/history?periodType=monthly&year=2026&month=7&lado=Compra`
- `/exchange/v2/history?periodType=yearly&year=2026&lado=Venda`

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
