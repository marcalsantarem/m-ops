using MOps.Core.Dtos;

namespace MOps.Core.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <exception cref="ValidationFailedException">Entrada inválida.</exception>
    Task<ProjectDto> CreateAsync(ProjectInput input, CancellationToken cancellationToken = default);

    /// <returns>null se o projeto não existir.</returns>
    /// <exception cref="ValidationFailedException">Entrada inválida.</exception>
    Task<ProjectDto?> UpdateAsync(int id, ProjectInput input, CancellationToken cancellationToken = default);

    /// <returns>false se o projeto não existir.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
