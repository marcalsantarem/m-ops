namespace MOps.Core.Dtos;

/// <summary>Sprint exposta pela API e pelo MCP.</summary>
public sealed record SprintDto(
    int Id,
    int ProjectId,
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsActive);

/// <summary>Dados de entrada para criar ou atualizar uma sprint. A ativação é feita por operação própria.</summary>
public sealed record SprintInput(
    string? Name,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate);
