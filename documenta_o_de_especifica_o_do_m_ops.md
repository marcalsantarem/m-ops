# 🚀 Prompt Mestre de Especificação: M-Ops (DevOps Pessoal & Gestão de Projetos)

## 📌 Contexto & Propósito
O **M-Ops** é uma aplicação web pessoal de gestão de projetos e engenharia de software no estilo Azure DevOps / Jira, mas desenhada para uso estritamente individual ("Solo Developer"). Ele tem como objetivo centralizar o gerenciamento de projetos, backlogs, sprints, quadros Kanban, relatórios de bugs e histórico de commits do Git local, além de oferecer suporte nativo a agentes de IA via **Model Context Protocol (MCP)**.

---

## 🎯 Requisitos Globais & Funcionalidades

### 1. Gestão Ágil de Projetos
- **Projetos:** Múltiplos projetos com escopos isolados, repositórios vinculados e metadados.
- **Work Items (Itens de Trabalho):** Tipagem distinta para `UserStory`, `Task` e `Bug`. Cada item possui título, descrição em Markdown, prioridade (`Low`, `Medium`, `High`, `Urgent`), status e vínculo com Sprints.
- **Sprints & Backlog:** Criação de sprints com datas de início/fim e gerenciamento do backlog do projeto.
- **Quadro Kanban Interativo:** Painel visual dinâmico com suporte a *drag and drop* entre colunas personalizáveis (*To Do*, *In Progress*, *In Review*, *Done*).

### 2. Integração com Git Local
- Rastreamento automático de commits via mensagens padronizadas contendo a hash da tarefa (ex: `feat: ajusta layout do menu (#42)`).
- Mecanismo de recepção via **Git Hook local (`post-commit`)** que envia via HTTP `POST` a mensagem, hash, autor e data do commit para a API online do M-Ops hospedada no Render.
- Visualização do histórico de commits vinculados diretamente na página de detalhes da Task/Bug.

### 3. Integração Nativa com IA via MCP (Model Context Protocol)
- Servidor MCP integrado para permitir que assistentes de código (Claude Code, Cursor, Codex) interajam diretamente com o M-Ops via `stdio` (executável CLI local) ou `SSE` (endpoint HTTP na API do Render).
- **Ferramentas (Tools) MCP expostas:**
  - `list_projects` / `get_project_context`: Retorna o contexto do projeto e da sprint ativa.
  - `create_task`: Cria novas tasks ou bugs no backlog diretamente do terminal.
  - `update_task_status`: Altera o status da tarefa no Kanban (ex: mover para *InProgress* ou *Done*).
  - `close_task_with_commit`: Encerra a tarefa anexando o resumo da resolução e a hash do commit.
  - `search_backlog`: Busca itens no backlog por palavra-chave ou status.

### 4. Acessibilidade Web & Multi-dispositivo (100% Cloud / Zero Docker Local)
- Sem dependência de Docker local.
- Banco de dados **PostgreSQL serverless gerenciado no Neon**.
- Backend hospedado no **Render** (com SSL nativo e deploy via GitHub).
- Frontend hospedado no **Netlify** (com suporte a SPA routing e HTTPS nativo).

---

## 🛠️ Stack Tecnológica Definida

| Camada | Tecnologia / Ferramenta | Detalhes / Bibliotecas |
| :--- | :--- | :--- |
| **Backend** | **C# / .NET 9** | ASP.NET Core Web API, Entity Framework Core (`Npgsql`) |
| **Protocolo IA** | **ModelContextProtocol.NET** | SDK oficial em C# para MCP (transporte `stdio` e `SSE`) |
| **Banco de Dados** | **Neon (Serverless Postgres)** | Conexão via `Npgsql` com SSL habilitado, Connection Pooling e Branching |
| **Frontend Web** | **React + Vite** | SPA responsiva em TypeScript, Tailwind CSS, Lucide Icons, `@dnd-kit` |
| **Git Sync** | **Git Hooks (Bash/PowerShell)** | Script `post-commit` disparando requisições REST para a API no Render |
| **Hospedagem Web** | **Netlify (Front) + Render (API)** | Deploys automáticos a cada `git push` via GitHub |

---

## 🏗️ Arquitetura e Estrutura da Solução .NET

```text
m-ops/
├── src/
│   ├── MOps.Core/             # Entidades de Domínio, Enums, Interfaces de Repositório
│   ├── MOps.Infrastructure/   # DbContext (EF Core), Migrations e Npgsql configurado para o Neon
│   ├── MOps.Api/              # Endpoints REST, Webhooks para Git e Controllers
│   ├── MOps.Mcp/              # Servidor MCP expondo as Tools de integração para Claude/Codex
│   └── MOps.Web/              # Interface Web em React + Vite + Tailwind CSS + Lucide Icons
├── hooks/
│   └── post-commit           # Script Git Hook local para sincronização de commits
└── README.md
```

---

## 🗄️ Modelo de Dados Mínimo (Entidades Principais)

1. **Project:** `Id`, `Name`, `Description`, `GitRepositoryUrl`, `CreatedAt`.
2. **Sprint:** `Id`, `ProjectId`, `Name`, `StartDate`, `EndDate`, `IsActive`.
3. **WorkItem:** `Id`, `ProjectId`, `SprintId` (null), `Title`, `Description`, `Type` (UserStory/Task/Bug), `Status` (ToDo/InProgress/InReview/Done), `Priority`, `CreatedAt`, `UpdatedAt`.
4. **CommitRecord:** `Id`, `WorkItemId`, `CommitHash`, `Message`, `Author`, `CommittedAt`.

---

## 📋 Instruções de Execução para a IA / Desenvolvedor

Ao implementar o **M-Ops**, siga estas diretrizes em ordem:

1. **Fase 1 - Core Backend & Banco de Dados Neon:**
   - Crie a Solution .NET 9 e configure o EF Core apontando para a string de conexão do **Neon Postgres** via `Npgsql`.
   - Monte as migrations e aplique-as via código (`Database.MigrateAsync()`) no `Program.cs` da API.

2. **Fase 2 - API REST & Endpoints Git:**
   - Crie os CRUDs de Projetos, Sprints e WorkItems.
   - Crie o endpoint `/api/commits/hook` preparado para receber payloads do script `post-commit` e associar o commit à tarefa correspondente através do ID na mensagem (ex: `#42`).

3. **Fase 3 - Servidor MCP:**
   - Configure o projeto `MOps.Mcp` utilizando a SDK do MCP para C#.
   - Registre as ferramentas `create_task`, `update_task_status`, `close_task_with_commit` e `get_project_context`.

4. **Fase 4 - Frontend Kanban Web (Netlify-ready):**
   - Monte a interface SPA em **React + Tailwind CSS** (com tema dark por padrão e ícones do **Lucide React**).
   - Implemente o painel Kanban utilizando a biblioteca `@dnd-kit` para movimentação ágil dos cards entre colunas.
   - Adicione o arquivo `_redirects` na pasta public do Vite (`/* /index.html 200`) para garantir o roteamento correto no Netlify.

5. **Fase 5 - Git Hooks & Publicação Cloud:**
   - Escreva o script de `post-commit` reutilizável em repositórios locais para enviar dados à API pública do Render.
   - Configure as variáveis de ambiente nos painéis do Render (`ConnectionStrings__DefaultConnection`) e Netlify (`VITE_API_URL`).