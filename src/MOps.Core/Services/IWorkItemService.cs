using MOps.Core.Dtos;

namespace MOps.Core.Services;

/// <summary>
/// Work items. Listagem e criação são por projeto (null se o projeto não existir);
/// leitura, atualização e remoção usam o ID global (null/false se não existir).
/// </summary>
public interface IWorkItemService
{
    Task<IReadOnlyList<WorkItemDto>?> ListAsync(int projectId, WorkItemQuery query, CancellationToken cancellationToken = default);
    Task<WorkItemDto?> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <exception cref="ValidationFailedException">Entrada inválida ou sprint inexistente/de outro projeto.</exception>
    Task<WorkItemDto?> CreateAsync(int projectId, WorkItemInput input, CancellationToken cancellationToken = default);

    /// <exception cref="ValidationFailedException">Entrada inválida ou sprint inexistente/de outro projeto.</exception>
    Task<WorkItemDto?> UpdateAsync(int id, WorkItemInput input, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
