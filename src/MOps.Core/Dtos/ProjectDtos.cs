namespace MOps.Core.Dtos;

/// <summary>Projeto exposto pela API e pelo MCP.</summary>
public sealed record ProjectDto(
    int Id,
    string Name,
    string? Description,
    string? GitRepositoryUrl,
    DateTimeOffset CreatedAt);

/// <summary>Dados de entrada para criar ou atualizar um projeto.</summary>
public sealed record ProjectInput(
    string? Name,
    string? Description,
    string? GitRepositoryUrl);
