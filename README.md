# 🎓 EduCore

Sistema de gestão escolar com **API REST em ASP.NET Core** e **SQL Server**, com dois fronts consumindo a mesma API:

- **Blazor** (em desenvolvimento): Blazor Web App com Fluent UI.
- **React + TypeScript** (planejado): será construído depois do Blazor, reaproveitando os mesmos endpoints.

O projeto contempla três perfis de acesso (administrador, professor e aluno) e cobre cadastro de usuários, estrutura acadêmica (disciplinas, subdisciplinas e turmas), avaliações, notas, frequência e painéis por perfil.

---

# 🚀 Tecnologias

## Back-end
- ASP.NET Core Web API (.NET 10 LTS)
- C# e Entity Framework Core
- SQL Server
- Autenticação JWT com autorização por perfil (roles)
- Swagger / OpenAPI
- Testes unitários com xUnit, Moq e FluentAssertions

## Front-end (Blazor)
- Blazor Web App (.NET 10 LTS), páginas autenticadas em WebAssembly
- Fluent UI Blazor (componentes Microsoft Fluent 2)
- Layout responsivo e preocupação com acessibilidade (rótulos ARIA, estados de carregamento e erro)

## Front-end (React, planejado)
- React e TypeScript

## Ferramentas
- Visual Studio e SQL Server Management Studio (SSMS)
- Git e GitHub

---

# 🗂️ Estrutura do repositório

```
EduCore/
├─ api/
│  ├─ EduCore.API/          API REST (controllers, services, repositories, DTOs)
│  └─ EduCore.Tests/        testes unitários dos services
├─ web-blazor/
│  ├─ EduCore.Web/          host do Blazor Web App
│  └─ EduCore.Web.Client/   páginas e componentes (WebAssembly)
├─ web-react/               front em React (planejado)
├─ database/scripts/        scripts SQL versionados (aplicados em ordem)
└─ EduCore.slnx             solution com API, testes e front Blazor
```

---

# 🏗️ Arquitetura

- Separação em camadas: Controllers → Services → Repositories → EF Core
- DTOs de entrada e saída, sem expor entidades na API
- Repository Pattern e injeção de dependência
- Erros padronizados com `ProblemDetails`
- Princípios SOLID; no front, serviços de API isolados por domínio (`IUsuarioApiClient`, `ITurmaApiClient`) e menu configurado por perfil
- Segredos fora do código (User Secrets em desenvolvimento)

---

# ▶️ Como executar

**Pré-requisitos:** .NET SDK 10 (a versão é fixada em `global.json`), SQL Server e Visual Studio 2026 ou superior.

1. Clone o repositório e abra `EduCore.slnx`.
2. Configure a connection string em `api/EduCore.API/appsettings.json` (banco `EscolaDB`) e aplique os scripts de `database/scripts` em ordem (veja o [README dos scripts](database/scripts/README.md)).
3. Defina a chave do JWT (mínimo de 32 caracteres) com User Secrets:
   ```bash
   dotnet user-secrets set "Jwt:Key" "<sua-chave-secreta>" --project api/EduCore.API
   ```
4. No Visual Studio: **Configure Startup Projects** → **Multiple startup projects** → `EduCore.API` e `EduCore.Web`.
5. Execute (F5). A API publica o Swagger e o front abre na tela de login.

> Os dados de demonstração estão sendo organizados em `database/scripts`.

---

# 📌 Roadmap

- [x] API: usuários, alunos, professores, disciplinas, subdisciplinas, turmas e avaliações
- [x] Autenticação JWT com perfis Admin, Professor e Aluno
- [x] Front Blazor: login, página inicial por perfil e painel administrativo
- [x] Cadastro e edição de usuários (alunos com matrícula em turmas)
- [x] Menu lateral por perfil
- [x] Admin: disciplinas e subdisciplinas (cadastro, edição, inativação e reativação)
- [x] Admin: turmas por ano letivo, com os alunos matriculados
- [x] Modelo acadêmico: atribuição de professores (professor × turma × disciplina/subdisciplina) e avaliações por turma com pesos por bimestre somando até 100 (API)
- [ ] Admin: tela de atribuição de professores
- [x] Lançamento de notas por avaliação e chamada em lote, boletim com média ponderada e frequência, endpoints `/me` e acesso restrito ao que cada professor leciona (API)
- [ ] Professor: minhas turmas, avaliações, lançamento de notas e chamada
- [ ] Aluno: boletim e frequência
- [ ] Dashboards, CI no GitHub Actions e deploy gratuito
- [ ] Front em React + TypeScript

---

# 👩‍💻 Desenvolvido por

Monalisa Rodrigues
