# Frontend — Contexto do Projeto

## O que é

SPA React + TypeScript que consome a Wallet API. Permite que o usuário gerencie suas finanças pessoais: carteiras, transações, categorias, operações de bolsa.

## Tecnologias

| Tech | Uso |
|---|---|
| React 18 | UI |
| TypeScript | Tipagem |
| Vite | Build e dev server |
| React Router v7 | Roteamento |
| Axios | HTTP client |
| Lucide React | Ícones |
| SCSS Modules / global SCSS | Estilização |
| MSW (Mock Service Worker) | Mocks para desenvolvimento offline |

## Rotas Existentes

| Rota | Descrição |
|---|---|
| `/login` | Tela de login |
| `/dashboard` | Painel principal |
| `/carteira` | Listagem e gestão de carteiras |
| `/transacoes` | Histórico de lançamentos (receita/despesa) |
| `/operacoes-bolsa` | Operações de renda variável |
| `/relatorios` | Relatórios financeiros |
| `/graficos` | Gráficos e análises |
| `/categorias` | Gestão de categorias |
| `/calendario` | Visualização por calendário |
| `/objetivos` | Metas financeiras |
| `/configuracoes` | Configurações do usuário |

## Fluxo de Autenticação

1. Usuário faz login → `accessToken` salvo em `sessionStorage`
2. Todas as rotas privadas passam pelo `<ProtectedRoute>` que verifica o contexto de auth
3. Quando o `accessToken` expira (401), o interceptor do `privateApi` faz refresh automático
4. Logout: limpa `sessionStorage` e redireciona para `/login`

## Variáveis de Ambiente (`.env`)

```
VITE_API_URL=http://localhost:5000
VITE_ENABLE_MSW=false   # true para usar mocks locais
```
