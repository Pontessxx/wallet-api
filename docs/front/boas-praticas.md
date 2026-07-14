# Frontend — Boas Práticas e Convenções

## Estrutura de Código

- **Páginas** ficam em `templates/` (layouts) ou em pastas por feature — nunca misturar com componentes genéricos de `components/`.
- **Componentes** em `components/` são UI genérico e reutilizável. Não devem fazer chamadas de API diretamente — usam contexts ou recebem dados por props.
- **Services** em `services/` são a única camada que chama a API. Nunca use `privateApi`/`publicApi` diretamente em um componente ou context.
- **Contexts** consomem services e expõem estado + mutations para a árvore de componentes.

---

## TypeScript

- Preferir `interface` para objetos, `type` para unions e aliases.
- Nunca usar `any` — se necessário, usar `unknown` e fazer narrowing.
- Tipar responses de API em `types/` ou próximo ao service correspondente.
- Se a lib não tiver tipos (`lucide-react`): adicionar declaração em `src/types/<lib>.d.ts`.

---

## Axios

- Usar `publicApi` para endpoints **sem autenticação**.
- Usar `privateApi` para endpoints **autenticados** — o interceptor injeta o token automaticamente.
- Nunca adicionar `Authorization` manualmente num componente.
- Tratar erros no service (ex: extrair `error.response.data`) antes de propagar para o contexto.

---

## Estilização

- Padrão de nomenclatura CSS: **BEM** (`bloco__elemento--modificador`).
- Arquivos SCSS associados ao componente ficam em `styles/` com mesmo nome (`SideBar.scss`).
- Variáveis de cor/espaçamento em `styles/_variables.scss`.
- Suporte a tema light/dark via atributo `data-theme` no `<html>` + CSS custom properties.

### `_variables.scss` — Variáveis de Cor

O tema é definido por seletores `[data-theme="dark"]` e `[data-theme="light"]` com CSS custom properties. Para usar nos arquivos `.scss`, importar as variáveis SCSS (`$var`) que encapsulam as custom properties.

#### Fundos

| Variável SCSS | Custom Property | Uso |
|---|---|---|
| `$bg` | `--color-bg` | Fundo da página |
| `$surface` | `--color-surface` | Cards, painéis, tabelas |
| `$surface-hover` | `--color-surface-hover` | Estado hover de superfícies |
| `$surface-active` | `--color-surface-active` | Estado active/selecionado |

#### Texto

| Variável SCSS | Custom Property | Uso |
|---|---|---|
| `$text` | `--color-text` | Texto principal |
| `$text-secondary` | `--color-text-secondary` | Texto secundário / subtítulos |
| `$text-disabled` | `--color-text-disabled` | Texto desabilitado |
| `$text-inverse` | `--color-text-inverse` | Texto sobre fundos coloridos |
| `$edit` | `--color-edit` | Cor de destaque para ações de edição |

#### Bordas e Sombras

| Variável SCSS | Custom Property | Uso |
|---|---|---|
| `$border` | `--color-border` | Bordas de cards, inputs, divisores |
| `$box-shadow` | `--color-box-shadow` | Sombra padrão |
| `$box-shadow-hover` | `--color-box-shadow-hover` | Sombra no hover |
| `$box-shadow-active` | `--color-box-shadow-active` | Sombra no active |

#### Estados / Semânticas

| Variável SCSS | Custom Property | Dark | Light |
|---|---|---|---|
| `$success` | `--color-success` | `#22c55e` | `#22c55e` |
| `$warning` | `--color-warning` | `#fbbf24` | `#f59e0b` |
| `$error` | `--color-error` | `#f87171` | `#ef4444` |
| `$info` | `--color-info` | `#38bdf8` | `#0ea5e9` |
| `$disabled` | `--color-disabled` | `#475569` | `#cbd5e1` |

#### Outras

| Variável SCSS | Valor | Uso |
|---|---|---|
| `$header-height` | `60px` | Altura fixa do Header (usar em cálculos de layout) |

#### Exemplo de uso

```scss
@use '../styles/variables' as *;  // ou @import dependendo da config Vite

.meu-card {
  background-color: $surface;
  border: 1px solid $border;
  color: $text;
  box-shadow: $box-shadow;

  &:hover {
    background-color: $surface-hover;
    box-shadow: $box-shadow-hover;
  }
}
```

> ⚠️ Nunca usar cores hardcoded (`#1e293b`, `white`, etc.) nos componentes — sempre referenciar as variáveis para garantir suporte ao tema.

---

## Acessibilidade

- Elementos interativos não-nativos (`<span>`, `<div>`) devem ter:
  - `role="button"`
  - `tabIndex={0}`
  - `onKeyDown` tratando Enter e Espaço
- Links de navegação: usar `<NavLink>` do React Router (não `<a href>`).

---

## Tratamento de Estado Assíncrono

- Sempre ter `isLoading`, `error` e dados separados no estado.
- Mostrar feedback visual (skeleton, spinner) durante loads.
- Limpar erros ao iniciar nova operação.

---

## Organização de Imports

Ordem recomendada:
1. React e hooks nativos
2. Bibliotecas externas (react-router, lucide-react, axios)
3. Contextos
4. Componentes
5. Services
6. Types / utils
7. Estilos (`.scss`)

---

## Nomenclatura

| Elemento | Convenção |
|---|---|
| Componentes | PascalCase (`CarteiraTable`) |
| Hooks | camelCase com prefixo `use` (`useDropdownMenu`) |
| Contextos | PascalCase com sufixo `Context` (`AuthContext`) |
| Services | camelCase com sufixo `Service` (`carteiraService`) |
| Arquivos | camelCase para services/hooks, PascalCase para componentes |

---

## Checklist ao Adicionar Nova Feature

- [ ] Service em `services/` com funções nomeadas por intenção
- [ ] Context atualizado (se precisar de estado global)
- [ ] Componente em `components/` se for reutilizável
- [ ] Rota registrada em `App.tsx`
- [ ] Link adicionado ao `SideBar.tsx` se for página principal
- [ ] MSW handler adicionado em `mocks/` se precisar de mock
- [ ] Tipos definidos em `types/` ou próximo ao uso
