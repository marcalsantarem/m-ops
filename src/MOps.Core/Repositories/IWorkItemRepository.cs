using MOps.Core.Entities;

namespace MOps.Core.Repositories;

public interface IWorkItemRepository
{
    Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Inclui os commits vinculados.</summary>
    Task<WorkItem?> GetByIdWithCommitsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkItem>> SearchAsync(WorkItemFilter filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    void Add(WorkItem workItem);
    void Update(WorkItem workItem);
    void Remove(WorkItem workItem);
}
