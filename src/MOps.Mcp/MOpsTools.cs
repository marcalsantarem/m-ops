using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using MOps.Core.Dtos;
using MOps.Core.Enums;

namespace MOps.Mcp;

/// <summary>Tools do M-Ops. Toda regra de negócio fica na API; aqui só orquestração e validação de entrada.</summary>
[McpServerToolType]
public sealed class MOpsTools(MOpsApiClient api)
{
    private const string StatusValues = "ToDo, InProgress, InReview, Done";
    private const string TypeValues = "UserStory, Task, Bug";
    private const string PriorityValues = "Low, Medium, High, Urgent";

    [McpServerTool(Name = "list_projects", ReadOnly = true, Idempotent = true)]
    [Description("Lista todos os projetos do M-Ops. Retorna JSON com id, name, description, gitRepositoryUrl e createdAt de cada projeto. Use o id nas demais tools.")]
    public async Task<string> ListProjects(CancellationToken ct) =>
        ToJson(await api.GetProjectsAsync(ct));

    [McpServerTool(Name = "get_project_context", ReadOnly = true, Idempotent = true)]
    [Description("Retorna o contexto de um projeto: dados do projeto, a sprint ativa (ou null) e os work items dessa sprint (id, title, description em Markdown, type, status, priority). Use antes de começar a trabalhar em um projeto.")]
    public async Task<string> GetProjectContext(
        [Description("Id do projeto (veja list_projects).")] int projectId,
        CancellationToken ct)
    {
        var project = await api.GetProjectAsync(projectId, ct);
        var sprints = await api.GetSprintsAsync(projectId, ct);
        var active = sprints.FirstOrDefault(s => s.IsActive);
        var items = active is null ? [] : await api.GetWorkItemsAsync(projectId, active.Id, null, null, ct);
        return ToJson(new { project, activeSprint = active, activeSprintWorkItems = items });
    }

    [McpServerTool(Name = "create_task", Destructive = false)]
    [Description("Cria um work item em um projeto. Sem sprintId o item vai para o backlog. Retorna o work item criado em JSON (o id serve para referenciar commits com #id).")]
    public async Task<string> CreateTask(
        [Description("Id do projeto.")] int projectId,
        [Description("Título do work item.")] string title,
        [Description("Tipo: " + TypeValues + ". Padrão: Task.")] string type = "Task",
        [Description("Descrição em Markdown (opcional).")] string? description = null,
        [Description("Prioridade: " + PriorityValues + ". Padrão: Medium.")] string priority = "Medium",
        [Description("Id da sprint (opcional). Omitido = backlog.")] int? sprintId = null,
        CancellationToken ct = default)
    {
        var input = new WorkItemInput(
            title,
            description,
            ParseEnum<WorkItemType>(type, "type", TypeValues),
            WorkItemStatus.ToDo,
            ParseEnum<Priority>(priority, "priority", PriorityValues),
            sprintId);
        return ToJson(await api.CreateWorkItemAsync(projectId, input, ct));
    }

    [McpServerTool(Name = "update_task_status", Idempotent = true, Destructive = false)]
    [Description("Altera o status (coluna do Kanban) de um work item. Retorna o work item atualizado em JSON.")]
    public async Task<string> UpdateTaskStatus(
        [Description("Id do work item.")] int workItemId,
        [Description("Novo status: " + StatusValues + ".")] string status,
        CancellationToken ct)
    {
        var parsed = ParseEnum<WorkItemStatus>(status, "status", StatusValues);
        return ToJson(await api.UpdateStatusAsync(workItemId, parsed, ct));
    }

    [McpServerTool(Name = "close_task_with_commit", Idempotent = true, Destructive = false)]
    [Description("Conclui um work item: muda o status para Done e registra o commit vinculado a ele. A referência #<workItemId> é acrescentada à mensagem se faltar. Retorna o work item atualizado e o resultado do vínculo do commit em JSON.")]
    public async Task<string> CloseTaskWithCommit(
        [Description("Id do work item a concluir.")] int workItemId,
        [Description("Hash do commit (SHA completo ou abreviado).")] string commitHash,
        [Description("Mensagem ou resumo do commit.")] string message,
        [Description("Autor do commit (opcional). Padrão: 'MCP'.")] string? author = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(commitHash))
            throw new McpException("commitHash é obrigatório.");

        var msg = (message ?? "").Trim();
        if (!Regex.IsMatch(msg, $@"#{workItemId}(?!\d)"))
            msg = msg.Length == 0 ? $"#{workItemId}" : $"{msg} (#{workItemId})";

        var workItem = await api.UpdateStatusAsync(workItemId, WorkItemStatus.Done, ct);

        CommitHookResult commit;
        try
        {
            commit = await api.PostCommitAsync(new CommitHookInput(
                commitHash.Trim(), msg, string.IsNullOrWhiteSpace(author) ? "MCP" : author.Trim(), DateTimeOffset.UtcNow), ct);
        }
        catch (McpException ex)
        {
            throw new McpException($"O work item #{workItemId} foi marcado como Done, mas o commit não foi registrado: {ex.Message}");
        }

        return ToJson(new { workItem, commit });
    }

    [McpServerTool(Name = "search_backlog", ReadOnly = true, Idempotent = true)]
    [Description("Busca work items de um projeto (backlog e sprints) por palavra-chave no título/descrição e, opcionalmente, por status. Retorna a lista de work items em JSON.")]
    public async Task<string> SearchBacklog(
        [Description("Id do projeto.")] int projectId,
        [Description("Palavra-chave (opcional).")] string? keyword = null,
        [Description("Filtro de status (opcional): " + StatusValues + ".")] string? status = null,
        CancellationToken ct = default)
    {
        WorkItemStatus? parsed = string.IsNullOrWhiteSpace(status) ? null : ParseEnum<WorkItemStatus>(status, "status", StatusValues);
        return ToJson(await api.GetWorkItemsAsync(projectId, null, parsed, keyword, ct));
    }

    private static TEnum ParseEnum<TEnum>(string value, string name, string accepted) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value?.Trim(), ignoreCase: true, out var parsed) && Enum.IsDefined(parsed) && !int.TryParse(value, out _)
            ? parsed
            : throw new McpException($"Valor inválido para {name}: '{value}'. Aceitos: {accepted}.");

    private static string ToJson<T>(T value) => JsonSerializer.Serialize(value, MOpsApiClient.Json);
}
