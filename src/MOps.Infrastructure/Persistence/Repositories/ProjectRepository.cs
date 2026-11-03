using Microsoft.EntityFrameworkCore;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(MOpsDbContext db) : IProjectRepository
{
    /// <summary>Rastreado: pode ser editado e salvo via IUnitOfWork.</summary>
    public Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
        => await db.Projects.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => db.Projects.AnyAsync(p => p.Id == id, cancellationToken);

    public void Add(Project project) => db.Projects.Add(project);
    public void Update(Project project) => db.Projects.Update(project);
    public void Remove(Project project) => db.Projects.Remove(project);
}
