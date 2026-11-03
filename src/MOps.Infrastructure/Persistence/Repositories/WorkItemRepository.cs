using Microsoft.EntityFrameworkCore;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Infrastructure.Persistence.Repositories;

public sealed class WorkItemRepository(MOpsDbContext db) : IWorkItemRepository
{
    private const string LikeEscape = "\\";

    /// <summary>Rastreado: pode ser editado e salvo via IUnitOfWork.</summary>
    public Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => db.WorkItems.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    /// <summary>Somente leitura; commits ordenados do mais recente para o mais antigo.</summary>
    public Task<WorkItem?> GetByIdWithCommitsAsync(int id, CancellationToken cancellationToken = default)
        => db.WorkItems.AsNoTracking()
            .Include(w => w.Commits.OrderByDescending(c => c.CommittedAt))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkItem>> SearchAsync(WorkItemFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = db.WorkItems.AsNoTracking().AsQueryable();

        if (filter.ProjectId is { } projectId)
            query = query.Where(w => w.ProjectId == projectId);

        if (filter.OnlyWithoutSprint)
            query = query.Where(w => w.SprintId == null);
        else if (filter.SprintId is { } sprintId)
            query = query.Where(w => w.SprintId == sprintId);

        if (filter.Status is { } status)
            query = query.Where(w => w.Status == status);

        if (filter.Type is { } type)
            query = query.Where(w => w.Type == type);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var pattern = "%" + EscapeLike(filter.Keyword.Trim()) + "%";
            query = query.Where(w =>
                EF.Functions.ILike(w.Title, pattern, LikeEscape) ||
                (w.Description != null && EF.Functions.ILike(w.Description, pattern, LikeEscape)));
        }

        return await query
            .OrderByDescending(w => w.UpdatedAt)
            .ThenByDescending(w => w.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => db.WorkItems.AnyAsync(w => w.Id == id, cancellationToken);

    public void Add(WorkItem workItem) => db.WorkItems.Add(workItem);
    public void Update(WorkItem workItem) => db.WorkItems.Update(workItem);
    public void Remove(WorkItem workItem) => db.WorkItems.Remove(workItem);

    /// <summary>Trata \, % e _ digitados pelo usuário como literais.</summary>
    private static string EscapeLike(string value)
        => value.Replace(LikeEscape, LikeEscape + LikeEscape)
                .Replace("%", LikeEscape + "%")
                .Replace("_", LikeEscape + "_");
}
