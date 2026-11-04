# Decisões do projeto

Decisões do dono que complementam ou ajustam a especificação.

| # | Data | Decisão |
| :--- | :--- | :--- |
| D-01 | 2026-11-03 | Kanban com 4 colunas fixas, iguais ao enum `Status` (`ToDo`, `InProgress`, `InReview`, `Done`). Sem colunas personalizáveis. |
| D-02 | 2026-11-03 | MCP sem acesso direto ao banco no transporte `stdio`: o executável local chama a API REST com token, assim a connection string do Neon fica só no Render. O transporte `SSE` roda dentro da `MOps.Api` e usa os mesmos serviços de domínio, protegido pelo mesmo token. |
| D-03 | 2026-11-03 | API e endpoint do hook protegidos por token (variável de ambiente no Render; o hook e o MCP `stdio` enviam o token). |
| D-04 | 2026-11-03 | Git hook em duas versões: Bash e PowerShell. |
| D-05 | 2026-11-03 | Dockerfile na raiz apenas para o deploy da API no Render (que não tem runtime .NET nativo). Continua sem Docker no desenvolvimento local. Plano `free` no `render.yaml` (hiberna; a primeira requisição após inatividade é lenta). |
| D-06 | 2026-11-04 | Login do front: o token da API é digitado uma vez numa tela de login e guardado no `localStorage` do navegador; nunca vai no bundle. |
