# Backend — Boas Práticas e Convenções

## Arquitetura

- Seguir **Clean Architecture**: dependências apontam para dentro (Domain ← Application ← Infrastructure ← AuthApi).
- O `Domain` **nunca** referencia Application, Infrastructure ou AuthApi.
- Serviços em `Application` só conhecem interfaces (`IRepository`), nunca classes concretas de infra.
- Controllers são **finos**: delegam toda lógica para serviços ou usam diretamente o `ApplicationDbContext` quando a operação é simples (casos de CRUD de transaction).

---

## Versioning de API

- **V2 é a versão ativa**. Todo novo endpoint ou mudança de contrato vai para V2.
- V1 existe apenas como rota de compatibilidade e **não deve** receber novas features.
- Quando V2 estabilizar um domínio, o controller V1 correspondente é removido.

---

## Autenticação nos Controllers

```csharp
// ✅ Correto: userId vem dos claims
var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

// ❌ Errado: userId no body ou query string
public async Task<IActionResult> Get([FromQuery] Guid userId) { ... }
```

---

## Nomenclatura

| Elemento | Convenção |
|---|---|
| Repositórios | `ICarteiraRepository` / `CarteiraRepository` |
| Serviços | `AuthV2Service`, `UserService`, `ContaCarteiraService` |
| Controllers | `Auth.cs` em `Controllers/V2/` |
| Migrations | PascalCase descritivo: `AddTransactionCategory`, `AddUserCategories` |
| Entidades PT | `Carteira`, `Transacoes`, `TransacaoBolsa` |
| Entidades EN | `User`, `Category`, `RefreshToken` |

---

## Tratamento de Erros

- Respostas de erro usam o modelo `ResponseError` (`{ title, detail, status }`).
- Exceções conhecidas (`UnauthorizedAccessException`, `KeyNotFoundException`) são capturadas e convertidas via `ErrorBaseExtensions`.
- Não expor stack trace ou detalhes internos em produção.

---

## EF Core

- Usar `ApplicationDbContext` diretamente no controller **somente** para operações que não têm lógica de negócio (ex: CRUD simples de transactions).
- Para lógica de negócio complexa, usar repositórios + serviços.
- Sempre checar migrações pendentes antes de subir: `has-pending-model-changes`.
- Se a migration atual funcionar, não tem problema apagar as outras

---

## Docker

- O Dockerfile do backend precisa instalar `libgssapi-krb5-2` para o Npgsql funcionar corretamente no container Linux.
- Configurações sensíveis (JWT secret, connection string) ficam em `.env` (ignorado pelo git).

---

## Testes

- Testes ficam em `backend/test/` separados por camada: `Domain.Tests`, `Infrastructure.Tests`, `AuthApi.Tests`.
- Usar `xUnit`.
- Nomear testes: `Metodo_Cenario_ResultadoEsperado`.

---

## Checklist ao Adicionar Novo Endpoint

- [ ] Implementar na versão correta (para novos contratos)
- [ ] `userId` extraído dos claims, não do body
- [ ] Retornar `ResponseError` padronizado em casos de erro
- [ ] Proteger com `[Authorize]` se necessário
- [ ] Adicionar migration se alterar o schema
- [ ] Registrar DI em `DependencyInjection.cs` se criar novo serviço/repositório
- [ ] Colocar `[Obsolete]` na versão antiga se houver daquele endpoint 
