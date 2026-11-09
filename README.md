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

**E-mail ("Esqueci minha senha"):** em desenvolvimento, sem SMTP configurado, a API não envia nada. Ela grava cada e-mail como HTML em `api/EduCore.API/emails-dev/` (ignorada pelo Git) e escreve o link no log. Para enviar de verdade, configure `Email:Smtp:Host`, `Porta`, `Usuario`, `Email:RemetenteEndereco` e `RedefinicaoSenha:UrlFront` (endereço do front usado no link). A senha do SMTP vai em User Secrets:

```bash
dotnet user-secrets set "Email:Smtp:Senha" "<senha-do-smtp>" --project api/EduCore.API
```

> Para ver o sistema com dados, aplique também o `002_dados_demonstracao.sql`: ele cria uma turma completa com professores, alunos, notas e chamadas. Os e-mails e a senha de acesso estão no [README dos scripts](database/scripts/README.md#acesso-de-demonstração).

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
- [x] Admin: atribuição de professores na tela da turma
- [x] Dados de demonstração (turma completa com notas e chamadas)
- [x] Lançamento de notas por avaliação e chamada em lote, boletim com média ponderada e frequência, endpoints `/me` e acesso restrito ao que cada professor leciona (API)
- [x] Professor: minhas turmas, diário de classe (avaliações com pesos por bimestre), lançamento de notas e chamada
- [x] Aluno: desempenho em gráficos (média e frequência por disciplina, com faixas de risco/atenção) e boletim completo
- [x] Admin: ficha do aluno (desempenho e boletim) e ficha do professor (pendências de lançamento, desempenho das turmas, projeção de reforço e reposição e diário em consulta)
- [x] Esqueci minha senha: link por e-mail de uso único (30 minutos, só o hash fica no banco), resposta igual para e-mail existente ou não e limite de tentativas por IP no login e na redefinição
- [ ] Dashboards, CI no GitHub Actions e deploy gratuito
- [ ] Front em React + TypeScript

---

# 👩‍💻 Desenvolvido por

Monalisa Rodrigues
