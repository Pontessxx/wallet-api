# Frontend — Componentes Reutilizáveis

## SideBar

**Arquivo**: `components/SideBar.tsx`

Menu lateral colapsável. Usa `useState` para controlar o estado collapsed/expanded.

**Seções de navegação**:
- **Principal**: Dashboard, Contas
- **Transações**: Transações, Operações Bolsa
- **Análises**: Relatórios, Gráficos
- **Organização**: Categorias, Calendário, Objetivos

**Footer**: link para Configurações + botão de toggle de tema (usa `ThemeContext`).

**Props**: nenhuma — tudo interno.

**CSS**: `styles/SideBar.scss` com classes BEM:
- `.sidebar` / `.sidebar--collapsed`
- `.sidebar__link` / `.sidebar__link--active`
- `.sidebar__toggle-icon` / `.sidebar__toggle-icon--rotated`

---

## Modal

**Arquivo**: `components/Modal.tsx`

Modal genérico reutilizável para formulários e confirmações.

---

## Money

**Arquivo**: `components/Money.tsx`

Exibe valores monetários com formatação. Integra com `VisibilityContext` para ocultar/mostrar saldos.

---

## TableShell

**Arquivo**: `components/TableShell.tsx`

Estrutura base para tabelas. Recebe cabeçalhos e renderiza o corpo via children ou render props.

---

## TableEmptyState

**Arquivo**: `components/TableEmptyState.tsx`

Estado vazio padronizado quando não há dados na tabela.

---

## TableActionsCell

**Arquivo**: `components/TableActionsCell.tsx`

Célula com ícones de ação (editar, excluir). Usada como última coluna das tabelas.

---

## CarteiraTable / CarteiraActionsMenu

**Arquivo**: `components/CarteiraTable.tsx`, `components/CarteiraActionsMenu.tsx`

Tabela de carteiras com menu contextual de ações. Consome `CarteiraContext`.

---

## CategoriaTable / CategoriaActionsMenu

**Arquivo**: `components/CategoriaTable.tsx`, `components/CategoriaActionsMenu.tsx`

Tabela de categorias com menu contextual. Consome `CategoriaContext`.

---

## BankCombobox / BankLogo

**Arquivo**: `components/BankCombobox.tsx`, `components/BankLogo.tsx`

Combobox para seleção de banco. `BankLogo` exibe o logo pelo código do banco.

---

## Padrões de Componente

- Componentes **sem lógica de negócio** — apenas UI e estado local simples.
- Lógica de dados fica nos **Contexts** ou **Services**.
- Estilização via **SCSS** com convenção BEM.
- Ícones via **Lucide React** (`size={20}` como padrão).
- Acessibilidade: elementos interativos não-nativos recebem `role="button"`, `tabIndex={0}`, `onKeyDown` com Enter/Espaço.
