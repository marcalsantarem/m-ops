# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Estado atual

O repositório ainda não tem código. As pastas da estrutura planejada existem, mas estão vazias (só com `.gitkeep`). A especificação está em `documenta_o_de_especifica_o_do_m_ops.md`. Ela é a fonte da verdade para escopo, stack e ordem de construção. Leia-a antes de começar uma fase. Quando a solution existir, substitua esta seção pelos comandos reais de build, execução, testes e migrations.

## Regra máxima: gasto mínimo de tokens

Vale para a sessão principal e para todos os agentes, em toda interação (com o dono e entre agentes):
- Respostas e relatórios curtos e diretos, sem repetir contexto, sem resumos redundantes.
- Ler só o necessário: buscar com Grep/Glob e ler trechos, não arquivos inteiros sem motivo.
- Delegar só quando compensa; tarefa pequena, o gerente faz direto. Ao delegar, passar um briefing enxuto e objetivo.
- Não pedir aos agentes nada além do necessário (sem relatórios longos, sem exploração ampla).

## Modo de trabalho: sessão principal como gerente do projeto

A sessão principal atua como **gerente do projeto**: recebe os pedidos do dono, divide o trabalho, delega aos agentes em `.claude/agents/`, revisa o que eles entregam, commita e reporta.

| Agente | Escopo |
| :--- | :--- |
| `backlog` | Planejamento: quebra em tasks, atribui agentes, controla marcos em `docs/backlog.md` |
| `backend-dotnet` | `MOps.Core`, `MOps.Infrastructure`, `MOps.Api`, `hooks/post-commit` |
| `mcp-server` | `MOps.Mcp` |
| `frontend-web` | `MOps.Web` |
| `migration-reviewer` | Revisão (somente leitura) de toda migration criada ou alterada |

Fluxo para cada pedido:
1. Entender o pedido. Se houver decisão de produto em aberto, perguntar ao dono antes de delegar.
2. Trabalho novo ou maior que uma task: pedir ao `backlog` para quebrar em tasks. Executar as tasks prontas delegando ao agente indicado, passando o contexto necessário (os agentes não veem a conversa). Tarefas que cruzam escopos vão em sequência: primeiro quem fornece (ex.: backend cria o endpoint), depois quem consome (ex.: frontend).
3. Revisar o relatório e as alterações de cada agente; rodar build/testes para conferir.
4. Se houve migration nova ou alterada, acionar o `migration-reviewer`. Veredito **BLOQUEADA** impede o commit até a correção.
5. Commitar na `dev` e pedir ao `backlog` para marcar as tasks como feitas. Se um marco for concluído, avisar o dono (PR só quando ele pedir).
6. Reportar ao dono: o que foi feito, por qual agente, o que foi verificado (e o resultado real), o que ficou pendente e o que precisa de decisão dele.

Os agentes nunca fazem commit, push nem mexem em branches. Ajustes pequenos e triviais podem ser feitos direto pelo gerente.

## Regras de Git (obrigatórias)

- Faça commits **somente na branch `dev`**. A `main` é protegida e só recebe alterações por pull request.
- **Não crie branches** sem pedido explícito.
- **Só abra o PR `dev` → `main` quando for pedido**, ao atingir um marco.

Detalhes em `docs/git-workflow.md`.

## O que é o M-Ops

O M-Ops é uma aplicação web de gestão de projetos e DevOps, no estilo Azure DevOps ou Jira, para um único desenvolvedor. Ela cobre projetos, backlog, sprints, quadro Kanban, bugs e commits de repositórios Git locais vinculados aos work items. Também inclui um servidor MCP para que assistentes de código com IA consultem e alterem o backlog.

Decisões do dono que ajustam a especificação ficam em `docs/decisoes.md` e prevalecem sobre ela.

## Stack e hospedagem (definidos pela especificação)

- **Backend:** .NET 9, ASP.NET Core Web API, EF Core com Npgsql. Hospedado no **Render**, com deploy a partir do GitHub.
- **Banco de dados:** Postgres serverless no **Neon**, com SSL e connection pooling. A string de conexão vem de `ConnectionStrings__DefaultConnection`.
- **MCP:** SDK oficial em C# (ModelContextProtocol). Oferece dois transportes: `stdio` como CLI local e `SSE` como endpoint HTTP na API do Render.
- **Frontend:** React, Vite, TypeScript, Tailwind, ícones Lucide e `@dnd-kit`. Tema escuro por padrão. Hospedado no **Netlify**, com a URL base da API em `VITE_API_URL`.
- **Sem Docker local.** Não adicione docker-compose nem containers para desenvolvimento. O banco é sempre o Neon (use um branch do Neon para desenvolvimento ou testes).

## Estrutura planejada da solution

```
src/MOps.Core            entidades de domínio, enums, interfaces de repositório (sem dependências de infra)
src/MOps.Infrastructure  DbContext, migrations do EF, configuração Npgsql/Neon
src/MOps.Api             controllers REST + webhook de commits do Git
src/MOps.Mcp             servidor MCP que expõe as tools para agentes de IA
src/MOps.Web             SPA em React
hooks/post-commit        Git hook reutilizável que envia commits para a API
```

As dependências seguem `Api`/`Mcp` → `Infrastructure` → `Core`. As tools do MCP e os endpoints REST devem compartilhar a mesma lógica de domínio e de repositório, sem duplicá-la.

## Comportamentos transversais

- **Modelo de dados:** `Project`, `Sprint` (`IsActive`), `WorkItem` e `CommitRecord`.
  - `WorkItem.Type` é `UserStory`, `Task` ou `Bug`.
  - `WorkItem.Status` é `ToDo`, `InProgress`, `InReview` ou `Done`.
  - `WorkItem.Priority` é `Low`, `Medium`, `High` ou `Urgent`.
  - `WorkItem.SprintId` aceita nulo; nulo significa que o item está no backlog.
  - `WorkItem.Description` guarda Markdown.
- **Migrations** são aplicadas na inicialização da API com `Database.MigrateAsync()` no `Program.cs`.
- **Vínculo de commits:**
  - `hooks/post-commit` faz um `POST` para `/api/commits/hook` com mensagem, hash, autor e data.
  - A API extrai o ID do work item de uma referência `#<id>` na mensagem, por exemplo `feat: ajusta layout do menu (#42)`, e grava um `CommitRecord`.
  - O hook precisa funcionar tanto em Bash quanto em PowerShell.
- **Tools do MCP:** `list_projects`, `get_project_context` (projeto e sua sprint ativa), `create_task`, `update_task_status`, `close_task_with_commit` e `search_backlog`.
- **Roteamento SPA no Netlify:** `MOps.Web/public/_redirects` deve conter `/* /index.html 200`.

## Ordem de construção (da especificação)

1. Backend core e banco Neon: solution, EF Core e migrations
2. CRUD REST de projetos, sprints e work items, mais o webhook de commits
3. Servidor MCP e suas tools
4. Frontend Kanban em React
5. Script do Git hook e configuração de ambiente no Render e no Netlify
