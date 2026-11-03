using Microsoft.EntityFrameworkCore;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Infrastructure.Persistence.Repositories;

public sealed class CommitRecordRepository(MOpsDbContext db) : ICommitRecordRepository
{
    /// <summary>Rastreado: pode ser removido e salvo via IUnitOfWork.</summary>
    public Task<CommitRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.CommitRecords.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CommitRecord>> ListByWorkItemAsync(int workItemId, CancellationToken cancellationToken = default)
        => await db.CommitRecords.AsNoTracking()
            .Where(c => c.WorkItemId == workItemId)
            .OrderByDescending(c => c.CommittedAt)
            .ThenByDescending(c => c.Id)
            .ToListAsync(cancellationToken);

    /// <summary>Checagem prévia; o índice único (WorkItemId, CommitHash) continua sendo a garantia final.</summary>
    public Task<bool> ExistsAsync(int workItemId, string commitHash, CancellationToken cancellationToken = default)
        => db.CommitRecords.AnyAsync(c => c.WorkItemId == workItemId && c.CommitHash == commitHash, cancellationToken);

    public void Add(CommitRecord commit) => db.CommitRecords.Add(commit);
    public void Remove(CommitRecord commit) => db.CommitRecords.Remove(commit);
}
