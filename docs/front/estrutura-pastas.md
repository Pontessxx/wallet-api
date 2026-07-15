# Frontend — Estrutura de Pastas (`wallet-web/src/`)

```
src/
├── api/            ← instâncias Axios + interceptors
├── assets/         ← imagens, fontes, SVGs estáticos
├── components/     ← componentes reutilizáveis (UI genérico)
├── contexts/       ← React Contexts (estado global)
├── hooks/          ← hooks customizados
├── mocks/          ← handlers do MSW para desenvolvimento offline
├── routes/         ← componentes de rota (ex: ProtectedRoute)
├── services/       ← chamadas de API organizadas por domínio
├── styles/         ← SCSS global, variáveis, reset
├── templates/      ← layouts de página (estrutura de grid/sidebar)
├── types/          ← tipos e interfaces TypeScript globais
├── utils/          ← funções utilitárias puras
├── App.tsx         ← raiz da aplicação, define as rotas com React Router
└── main.tsx        ← entry point, monta o React + MSW
```

---

## `api/`

| Arquivo | Descrição |
|---|---|
| `api.ts` | Cria `publicApi` e `privateApi` (instâncias Axios). O `privateApi` injeta o `accessToken` no header e tem interceptor de refresh automático em caso de 401. |

> `publicApi` → endpoints sem autenticação (login, register, refresh)
> `privateApi` → endpoints que exigem `Authorization: Bearer <token>`

---

## `components/`

Componentes de **UI reutilizáveis**, sem lógica de negócio pesada.

| Componente | Descrição |
|---|---|
| `SideBar.tsx` | Menu lateral colapsável com seções (Principal, Transações, Análises, Organização) |
| `Header.tsx` | Cabeçalho das páginas |
| `Modal.tsx` | Modal genérico reutilizável |
| `Money.tsx` | Formatação e exibição de valores monetários |
| `BankCombobox.tsx` | Combobox de seleção de banco/conta |
| `BankLogo.tsx` | Exibe logo do banco pelo código |
| `CarteiraTable.tsx` | Tabela de carteiras |
| `CarteiraActionsMenu.tsx` | Menu de ações da carteira (editar/excluir) |
| `CategoriaTable.tsx` | Tabela de categorias |
| `CategoriaActionsMenu.tsx` | Menu de ações da categoria |
| `TableShell.tsx` | Estrutura base de tabela (cabeçalho + corpo) |
| `TableEmptyState.tsx` | Estado vazio para tabelas |
| `TableActionsCell.tsx` | Célula com botões de ação na última coluna |

---

## `contexts/`

Estado global via React Context API.

| Context | Descrição |
|---|---|
| `AuthContext.tsx` | Usuário autenticado, token, funções de login/logout |
| `CarteiraContext.tsx` | Lista de carteiras do usuário, operações de CRUD |
| `CategoriaContext.tsx` | Lista de categorias, operações de CRUD |
| `ThemeContext.tsx` | Tema atual (light/dark) + `toggleTheme()` |
| `VisibilityContext.tsx` | Controla visibilidade de valores monetários (ocultar saldos) |

---

## `hooks/`

| Hook | Descrição |
|---|---|
| `useDropdownMenu.ts` | Gerencia estado de abertura/fechamento de dropdowns |

---

## `services/`

Chamadas à API organizadas por domínio.

| Arquivo | Descrição |
|---|---|
| `authService.ts` | login, register, logout, refresh, validate |
| `carteiraService.ts` | listar, criar, atualizar, remover carteiras |
| `categoriaService.ts` | listar, criar, remover categorias |
| `userService.ts` | me, edit, editPassword, remove |

> Cada service usa `publicApi` ou `privateApi` conforme necessário.
> `categoriaService` normaliza a resposta `{ categorias: [...] }` para um array simples.

---

## `routes/`

| Componente | Descrição |
|---|---|
| `ProtectedRoute.tsx` | Redireciona para `/login` se não autenticado |

---

## `templates/`

Layouts de página (estrutura visual com Sidebar + conteúdo principal).

---

## `mocks/`

Handlers MSW para simular a API durante desenvolvimento sem backend.
Ativado via `VITE_ENABLE_MSW=true`.

---

## `types/`

| Arquivo | Descrição |
|---|---|
| `lucide-react.d.ts` | Declaração de módulo para `lucide-react` (fix TS7016) |

---

## `styles/`

SCSS global: variáveis de cor, tipografia, reset, temas light/dark.

---

## `utils/`

Funções puras: formatação de data, moeda, etc.
