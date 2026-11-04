# Backlog

Mantido pelo agente `backlog`.

## M1 — Core backend e banco Neon
Critério: `dotnet build` da solution passa; a API sobe localmente apontando para um branch do Neon, aplica as migrations no startup e as 4 tabelas (Project, Sprint, WorkItem, CommitRecord) existem no banco.
Status: concluído

| ID | Task | Agente | Depende de | Status |
| :--- | :--- | :--- | :--- | :--- |
| T-001 | Criar solution .NET 9 `MOps.sln` com projetos `MOps.Core` (classlib), `MOps.Infrastructure` (classlib), `MOps.Api` (webapi) e referências entre eles; `dotnet build` passa | backend-dotnet | — | feito |
| T-002 | Criar projeto Neon e um branch de desenvolvimento; guardar a connection string (SSL + pooling) como `ConnectionStrings__DefaultConnection` em user-secrets da `MOps.Api`, fora do Git | gerente | — | feito |
| T-003 | Entidades de domínio e enums em `MOps.Core` (Project, Sprint, WorkItem, CommitRecord; WorkItemType, WorkItemStatus, Priority); build passa | backend-dotnet | T-001 | feito |
| T-004 | Interfaces de repositório em `MOps.Core` para as 4 entidades; build passa | backend-dotnet | T-003 | feito |
| T-005 | `MOpsDbContext` em `MOps.Infrastructure` com Npgsql, mapeamentos (chaves, FKs, índices, enums) e extensão de DI que lê `DefaultConnection`; build passa | backend-dotnet | T-003 | feito |
| T-006 | Implementações dos repositórios em `MOps.Infrastructure` e registro na DI; build passa | backend-dotnet | T-004, T-005 | feito |
| T-007 | Migration inicial `InitialCreate` com as 4 tabelas + migration-reviewer | backend-dotnet | T-005 | feito |
| T-008 | `Program.cs` da API registra a Infrastructure e roda `Database.MigrateAsync()` no startup; endpoint `/health` responde 200 | backend-dotnet | T-006, T-007 | feito |
| T-009 | Subir a API local contra o branch Neon e conferir que as tabelas foram criadas | gerente | T-002, T-008 | feito |

## M2 — API REST e endpoint de commits
Critério: CRUDs de Projects, Sprints e WorkItems respondem corretamente; `POST /api/commits/hook` com mensagem contendo `#<id>` grava um CommitRecord vinculado ao WorkItem.
Status: concluído

| ID | Task | Agente | Depende de | Status |
| :--- | :--- | :--- | :--- | :--- |
| T-010 | CRUD de Projects (`/api/projects`) com DTOs e validação básica | backend-dotnet | T-009 | feito |
| T-011 | CRUD de Sprints aninhado ao projeto, incluindo marcar sprint ativa (uma por projeto) | backend-dotnet | T-010 | feito |
| T-012 | CRUD de WorkItems com filtros por projeto, sprint, status, tipo e palavra-chave | backend-dotnet | T-010 | feito |
| T-013 | Endpoint para alterar só o status de um WorkItem (uso do Kanban e do MCP) | backend-dotnet | T-012 | feito |
| T-014 | `POST /api/commits/hook`: recebe hash, mensagem, autor e data; extrai `#<id>` e grava CommitRecord; ignora/retorna 4xx sem ID válido; tratar `DbUpdateException` do índice único (WorkItemId, CommitHash) como duplicata | backend-dotnet | T-012 | feito |
| T-015 | `GET` de commits de um WorkItem | backend-dotnet | T-014 | feito |
| T-016 | Proteção do hook e da API por chave/token simples via variável de ambiente | backend-dotnet | T-014 | feito |
| T-017 | CORS configurável por variável de ambiente (origem do front local e do Netlify) | backend-dotnet | T-010 | feito |
| T-018 | Teste manual dos endpoints (arquivo `.http` ou curl) contra o branch Neon | gerente | T-011, T-013, T-015, T-016 | feito |

## M3 — Servidor MCP
Critério: `MOps.Mcp` roda via `stdio`, é registrado no Claude Code e as tools listam projetos, criam task, mudam status, fecham com commit e buscam no backlog com efeito real no banco.
Status: a fazer

| ID | Task | Agente | Depende de | Status |
| :--- | :--- | :--- | :--- | :--- |
| T-019 | Criar projeto `MOps.Mcp` (console) com SDK ModelContextProtocol, transporte `stdio` e cliente HTTP para a API (URL e token por config, D-02); build passa e servidor inicia | mcp-server | T-018 | a fazer |
| T-020 | Tools `list_projects` e `get_project_context` (projeto + sprint ativa + itens da sprint) | mcp-server | T-019 | a fazer |
| T-021 | Tool `create_task` (Task/Bug/UserStory, prioridade, sprint opcional) | mcp-server | T-019 | a fazer |
| T-022 | Tool `update_task_status` | mcp-server | T-019 | a fazer |
| T-023 | Tool `close_task_with_commit` (status Done + resumo + CommitRecord) | mcp-server | T-022 | a fazer |
| T-024 | Tool `search_backlog` por palavra-chave e status | mcp-server | T-019 | a fazer |
| T-025 | Transporte SSE exposto como endpoint HTTP na API | mcp-server | T-020, T-021, T-022, T-023, T-024 | a fazer |
| T-026 | Registrar o MCP `stdio` no Claude Code e testar cada tool | gerente | T-020, T-021, T-022, T-023, T-024 | a fazer |

## M4 — Frontend Kanban web
Critério: SPA roda local (`npm run dev`) consumindo a API; dá para criar projeto, sprint e work item, mover cards entre colunas por drag and drop (status persiste) e ver os commits no detalhe do item; `npm run build` passa.
Status: a fazer

| ID | Task | Agente | Depende de | Status |
| :--- | :--- | :--- | :--- | :--- |
| T-027 | Scaffold `MOps.Web` com Vite + React + TypeScript + Tailwind (dark padrão) + Lucide + React Router; `public/_redirects` com `/* /index.html 200`; build passa | frontend-web | — | feito |
| T-028 | Cliente HTTP usando `VITE_API_URL` e tipos dos DTOs da API | frontend-web | T-027, T-018 | a fazer |
| T-029 | Layout base (navegação, seletor de projeto) e tela de projetos (listar/criar/editar) | frontend-web | T-028 | a fazer |
| T-030 | Tela de sprints do projeto (listar/criar/ativar) | frontend-web | T-029 | a fazer |
| T-031 | Tela de backlog (lista de work items com filtros, criar/editar, mover para sprint) | frontend-web | T-029 | a fazer |
| T-032 | Quadro Kanban com `@dnd-kit` (To Do, In Progress, In Review, Done) persistindo status | frontend-web | T-031 | a fazer |
| T-033 | Detalhe do work item com descrição Markdown renderizada e histórico de commits | frontend-web | T-031 | a fazer |
| T-034 | Layout responsivo (uso no celular) revisado nas telas principais | frontend-web | T-032, T-033 | a fazer |

## M5 — Git hooks e publicação cloud
Critério: API no Render e front no Netlify publicados via push no GitHub; um commit local com `#<id>` em um repo com o hook instalado aparece no detalhe do work item na versão publicada.
Status: a fazer

| ID | Task | Agente | Depende de | Status |
| :--- | :--- | :--- | :--- | :--- |
| T-035 | Script `hooks/post-commit` (Bash) que envia hash, mensagem, autor e data para a API, com URL e token por variável/config | backend-dotnet | T-016 | feito |
| T-036 | Variante PowerShell do hook e instruções de instalação em outro repositório | backend-dotnet | T-035 | a fazer |
| T-037 | Preparar a API para o Render (porta via `PORT`, arquivo de build/start ou `render.yaml`) | backend-dotnet | T-018 | a fazer |
| T-038 | Criar serviço no Render ligado ao GitHub e configurar `ConnectionStrings__DefaultConnection` (branch principal do Neon), token e CORS | gerente | T-037 | a fazer |
| T-039 | Preparar o front para o Netlify (`netlify.toml` com base `src/MOps.Web`, comando e pasta de publicação) | frontend-web | T-034 | a fazer |
| T-040 | Criar site no Netlify ligado ao GitHub e configurar `VITE_API_URL` | gerente | T-038, T-039 | a fazer |
| T-041 | Teste ponta a ponta: hook instalado, commit com `#<id>`, commit visível no front publicado | gerente | T-036, T-040 | a fazer |
