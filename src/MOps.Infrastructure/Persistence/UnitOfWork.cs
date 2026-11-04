using Microsoft.EntityFrameworkCore;
using MOps.Core.Repositories;
using Npgsql;

namespace MOps.Infrastructure.Persistence;

public sealed class UnitOfWork(MOpsDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            // Descarta as inserções que falharam para o contexto continuar utilizável.
            foreach (var entry in ex.Entries.Where(e => e.State == EntityState.Added))
                entry.State = EntityState.Detached;
            throw new DuplicateKeyException($"Violação de índice único ({pg.ConstraintName}).", ex);
        }
    }

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
