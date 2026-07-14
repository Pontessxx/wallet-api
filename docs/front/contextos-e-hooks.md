# Frontend — Contextos e Hooks

## AuthContext (`contexts/AuthContext.tsx`)

**Estado global de autenticação.**

| Valor / Função | Tipo | Descrição |
|---|---|---|
| `user` | `User \| null` | Dados do usuário autenticado |
| `accessToken` | `string \| null` | Token JWT em sessionStorage |
| `isAuthenticated` | `boolean` | Atalho para `user !== null` |
| `login(username, password)` | `async fn` | Chama authService.login, salva token e user |
| `logout()` | `async fn` | Chama authService.logout, limpa estado e redireciona |
| `isLoading` | `boolean` | Enquanto verifica sessão existente no mount |

**Comportamento no mount**: tenta `authService.validate()` para restaurar sessão de uma aba reaberta (token ainda em sessionStorage).

---

## CarteiraContext (`contexts/CarteiraContext.tsx`)

**Gerencia a lista de carteiras do usuário.**

| Valor / Função | Descrição |
|---|---|
| `carteiras` | Array com todas as carteiras |
| `saldoTotal` | Soma dos saldos de todas as carteiras |
| `fetchCarteiras()` | Recarrega a lista da API |
| `createCarteira(data)` | Cria carteira e atualiza estado |
| `updateCarteira(data)` | Atualiza carteira e atualiza estado |
| `removeCarteira(id)` | Remove carteira e atualiza estado |
| `isLoading` | Loading state das operações |

---

## CategoriaContext (`contexts/CategoriaContext.tsx`)

**Gerencia as categorias do usuário.**

| Valor / Função | Descrição |
|---|---|
| `categorias` | Array com todas as categorias |
| `fetchCategorias()` | Recarrega da API |
| `createCategoria(data)` | Cria e atualiza estado |
| `removeCategoria(id)` | Remove e atualiza estado |
| `isLoading` | Loading state |

---

## ThemeContext (`contexts/ThemeContext.tsx`)

**Controla o tema visual da aplicação.**

| Valor / Função | Descrição |
|---|---|
| `theme` | `'light' \| 'dark'` |
| `toggleTheme()` | Alterna entre light e dark, persiste em `localStorage` |

O tema é aplicado via classe CSS no `<html>` ou `<body>` (`data-theme="dark"`).

---

## VisibilityContext (`contexts/VisibilityContext.tsx`)

**Controla a visibilidade de valores monetários (recurso "ocultar saldos").**

| Valor / Função | Descrição |
|---|---|
| `isVisible` | `boolean` — se os valores devem ser exibidos |
| `toggleVisibility()` | Alterna visibilidade |

Usado pelo componente `Money` para exibir `•••` no lugar do valor quando `isVisible = false`.

---

## Hooks Customizados

### `useDropdownMenu` (`hooks/useDropdownMenu.ts`)

Gerencia estado de abertura/fechamento de dropdowns/menus.

```typescript
const { isOpen, open, close, toggle, ref } = useDropdownMenu()
```

- `ref`: attach no container do dropdown para detectar clique fora (fecha automaticamente).
- Trata clique fora via `mousedown` listener no document.

---

## Padrões de Uso

- Sempre consumir contextos via hook customizado (ex: `useAuth()`, `useCarteira()`), nunca via `useContext(AuthContext)` diretamente — os hooks já validam se o provider está presente.
- Estado derivado (ex: `isAuthenticated`) calculado dentro do context, não no componente consumidor.
- Mutations (`create`, `update`, `remove`) devem atualizar o estado local **otimisticamente** ou refazer o fetch após sucesso, evitando inconsistências.
