# Wallet API — Documentação do Projeto

> Pasta ignorada pelo git. Arquivos para uso no Obsidian.

## Estrutura

```
docs/
├── back/
│   ├── contexto.md           ← visão geral do backend
│   ├── estrutura-pastas.md   ← o que cada projeto/pasta faz
│   ├── entidades.md          ← todas as entidades e enums do domínio
│   ├── endpoints.md          ← todos os endpoints da API (V2)
│   ├── autenticacao.md       ← fluxo de auth, JWT e refresh token
│   └── boas-praticas.md      ← convenções e regras do projeto
└── front/
    ├── contexto.md           ← visão geral do frontend
    ├── estrutura-pastas.md   ← o que cada pasta do src faz
    ├── componentes.md        ← componentes reutilizáveis
    ├── servicos-e-api.md     ← camada de serviços e instâncias Axios
    ├── contextos-e-hooks.md  ← Contexts, hooks e estado global
    └── boas-praticas.md      ← convenções e regras do projeto
```

## Visão Geral

| Camada | Tech |
|---|---|
| Backend | ASP.NET Core 8, C#, EF Core, PostgreSQL |
| Frontend | React 18, TypeScript, Vite, React Router v7 |
| Infra | Docker Compose |
