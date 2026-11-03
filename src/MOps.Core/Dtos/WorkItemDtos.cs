using MOps.Core.Enums;

namespace MOps.Core.Dtos;

/// <summary>Work item exposto pela API e pelo MCP. O Id é global (usado em <c>#id</c> nos commits).</summary>
public sealed record WorkItemDto(
    int Id,
    int ProjectId,
    int? SprintId,
    string Title,
    string? Description,
    WorkItemType Type,
    WorkItemStatus Status,
    Priority Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Dados de entrada para criar ou atualizar um work item. Status e Priority nulos assumem
/// ToDo/Medium na criação e mantêm o valor atual na atualização.
/// </summary>
public sealed record WorkItemInput(
    string? Title,
    string? Description,
    WorkItemType? Type,
    WorkItemStatus? Status,
    Priority? Priority,
    int? SprintId);

/// <summary>Filtros da listagem de work items de um projeto.</summary>
public sealed record WorkItemQuery(
    int? SprintId = null,
    bool OnlyWithoutSprint = false,
    WorkItemStatus? Status = null,
    WorkItemType? Type = null,
    string? Keyword = null);

/// <summary>Entrada da mudança rápida de status (PATCH /api/work-items/{id}/status e MCP).</summary>
public sealed record WorkItemStatusInput(WorkItemStatus? Status);
