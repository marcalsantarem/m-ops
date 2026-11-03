using Microsoft.EntityFrameworkCore;
using MOps.Core.Repositories;

namespace MOps.Infrastructure.Persistence;

public sealed class UnitOfWork(MOpsDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        // Via execution strategy para continuar compatível se EnableRetryOnFailure for ligado.
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            await operation(ct);
            await transaction.CommitAsync(ct);
        }, cancellationToken);
    }
}
