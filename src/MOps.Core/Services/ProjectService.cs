using MOps.Core.Dtos;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Core.Services;

public sealed class ProjectService(IProjectRepository projects, IUnitOfWork unitOfWork) : IProjectService
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int GitRepositoryUrlMaxLength = 500;

    public async Task<IReadOnlyList<ProjectDto>> ListAsync(CancellationToken cancellationToken = default)
        => (await projects.ListAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<ProjectDto?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(id, cancellationToken);
        return project is null ? null : ToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(ProjectInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        var project = new Project { CreatedAt = DateTimeOffset.UtcNow };
        Apply(project, data);

        projects.Add(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    public async Task<ProjectDto?> UpdateAsync(int id, ProjectInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        var project = await projects.GetByIdAsync(id, cancellationToken);
        if (project is null) return null;

        Apply(project, data);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(id, cancellationToken);
        if (project is null) return false;

        projects.Remove(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProjectDto ToDto(Project p)
        => new(p.Id, p.Name, p.Description, p.GitRepositoryUrl, p.CreatedAt);

    private static void Apply(Project project, ProjectInput data)
    {
        project.Name = data.Name!;
        project.Description = data.Description;
        project.GitRepositoryUrl = data.GitRepositoryUrl;
    }

    /// <summary>Normaliza (trim, vazio vira null) e valida; lança <see cref="ValidationFailedException"/>.</summary>
    private static ProjectInput Validate(ProjectInput? input)
    {
        var name = Normalize(input?.Name);
        var description = Normalize(input?.Description);
        var url = Normalize(input?.GitRepositoryUrl);
        var errors = new Dictionary<string, string[]>();

        if (name is null)
            errors[nameof(ProjectInput.Name)] = ["Name é obrigatório."];
        else if (name.Length > NameMaxLength)
            errors[nameof(ProjectInput.Name)] = [$"Name deve ter no máximo {NameMaxLength} caracteres."];

        if (description is { Length: > DescriptionMaxLength })
            errors[nameof(ProjectInput.Description)] = [$"Description deve ter no máximo {DescriptionMaxLength} caracteres."];

        if (url is not null)
        {
            if (url.Length > GitRepositoryUrlMaxLength)
                errors[nameof(ProjectInput.GitRepositoryUrl)] = [$"GitRepositoryUrl deve ter no máximo {GitRepositoryUrlMaxLength} caracteres."];
            else if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                     || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                errors[nameof(ProjectInput.GitRepositoryUrl)] = ["GitRepositoryUrl deve ser uma URL http(s) válida."];
        }

        if (errors.Count > 0) throw new ValidationFailedException(errors);
        return new ProjectInput(name, description, url);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
