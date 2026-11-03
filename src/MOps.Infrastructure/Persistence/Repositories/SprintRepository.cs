using Microsoft.EntityFrameworkCore;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Infrastructure.Persistence.Repositories;

public sealed class SprintRepository(MOpsDbContext db) : ISprintRepository
{
    /// <summary>Rastreado: pode ser editado e salvo via IUnitOfWork.</summary>
    public Task<Sprint?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.Sprints.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Sprint>> ListByProjectAsync(int projectId, CancellationToken cancellationToken = default)
        => await db.Sprints.AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.StartDate)
            .ToListAsync(cancellationToken);

    /// <summary>Rastreado: usado para encerrar ou trocar a sprint ativa.</summary>
    public Task<Sprint?> GetActiveByProjectAsync(int projectId, CancellationToken cancellationToken = default)
        => db.Sprints
            .Where(s => s.ProjectId == projectId && s.IsActive)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(Sprint sprint) => db.Sprints.Add(sprint);
    public void Update(Sprint sprint) => db.Sprints.Update(sprint);
    public void Remove(Sprint sprint) => db.Sprints.Remove(sprint);
}
