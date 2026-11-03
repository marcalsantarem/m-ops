using MOps.Core.Entities;

namespace MOps.Core.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    void Add(Project project);
    void Update(Project project);
    void Remove(Project project);
}
