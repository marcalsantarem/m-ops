using MOps.Core.Entities;

namespace MOps.Core.Repositories;

public interface ISprintRepository
{
    Task<Sprint?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sprint>> ListByProjectAsync(int projectId, CancellationToken cancellationToken = default);
    /// <summary>Sprint com <c>IsActive = true</c> do projeto, ou null se não houver.</summary>
    Task<Sprint?> GetActiveByProjectAsync(int projectId, CancellationToken cancellationToken = default);

    void Add(Sprint sprint);
    void Update(Sprint sprint);
    void Remove(Sprint sprint);
}
