using MOps.Core.Repositories;

namespace MOps.Infrastructure.Persistence;

public sealed class UnitOfWork(MOpsDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
