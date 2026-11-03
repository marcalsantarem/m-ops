using Microsoft.EntityFrameworkCore;
using MOps.Core.Entities;

namespace MOps.Infrastructure.Persistence;

public class MOpsDbContext(DbContextOptions<MOpsDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<CommitRecord> CommitRecords => Set<CommitRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MOpsDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeOffsetsToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeOffsetsToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Npgsql só grava DateTimeOffset com offset 0 em colunas timestamptz.
    /// Converte para UTC todo DateTimeOffset de entidades adicionadas ou modificadas.
    /// </summary>
    private void NormalizeDateTimeOffsetsToUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTimeOffset value && value.Offset != TimeSpan.Zero)
                    property.CurrentValue = value.ToUniversalTime();
            }
        }
    }
}
