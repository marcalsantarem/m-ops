using MOps.Core.Enums;

namespace MOps.Core.Repositories;

/// <summary>Filtros opcionais para busca de work items. Campos nulos são ignorados.</summary>
public sealed record WorkItemFilter
{
    public int? ProjectId { get; init; }
    public int? SprintId { get; init; }
    /// <summary>Quando true, retorna só itens sem sprint (backlog). Ignora <see cref="SprintId"/>.</summary>
    public bool OnlyWithoutSprint { get; init; }
    public WorkItemStatus? Status { get; init; }
    public WorkItemType? Type { get; init; }
    /// <summary>Busca case-insensitive em título e descrição.</summary>
    public string? Keyword { get; init; }
}
