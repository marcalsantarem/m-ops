using MOps.Core.Dtos;

namespace MOps.Core.Services;

/// <summary>
/// Sprints de um projeto. Os métodos retornam null/false se o projeto ou a sprint não existir
/// ou se a sprint pertencer a outro projeto.
/// </summary>
public interface ISprintService
{
    Task<IReadOnlyList<SprintDto>?> ListAsync(int projectId, CancellationToken cancellationToken = default);
    Task<SprintDto?> GetAsync(int projectId, int id, CancellationToken cancellationToken = default);

    /// <exception cref="ValidationFailedException">Entrada inválida.</exception>
    Task<SprintDto?> CreateAsync(int projectId, SprintInput input, CancellationToken cancellationToken = default);

    /// <exception cref="ValidationFailedException">Entrada inválida.</exception>
    Task<SprintDto?> UpdateAsync(int projectId, int id, SprintInput input, CancellationToken cancellationToken = default);

    /// <summary>Remove a sprint; os work items dela voltam ao backlog (FK SetNull).</summary>
    Task<bool> DeleteAsync(int projectId, int id, CancellationToken cancellationToken = default);

    /// <summary>Ativa a sprint e desativa a sprint ativa anterior do projeto, na mesma transação.</summary>
    Task<SprintDto?> ActivateAsync(int projectId, int id, CancellationToken cancellationToken = default);
}
