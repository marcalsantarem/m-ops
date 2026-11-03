using MOps.Core.Entities;

namespace MOps.Core.Repositories;

public interface ICommitRecordRepository
{
    Task<CommitRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Commits do work item, mais recentes primeiro.</summary>
    Task<IReadOnlyList<CommitRecord>> ListByWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);
    /// <summary>Evita gravar o mesmo commit duas vezes no mesmo work item (hook reenviado).</summary>
    Task<bool> ExistsAsync(int workItemId, string commitHash, CancellationToken cancellationToken = default);

    void Add(CommitRecord commit);
    void Remove(CommitRecord commit);
}
