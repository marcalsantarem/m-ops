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
}
