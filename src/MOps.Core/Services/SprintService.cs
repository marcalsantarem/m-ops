using MOps.Core.Dtos;
using MOps.Core.Entities;
using MOps.Core.Repositories;

namespace MOps.Core.Services;

public sealed class SprintService(IProjectRepository projects, ISprintRepository sprints, IUnitOfWork unitOfWork) : ISprintService
{
    public const int NameMaxLength = 200;

    public async Task<IReadOnlyList<SprintDto>?> ListAsync(int projectId, CancellationToken cancellationToken = default)
    {
        if (!await projects.ExistsAsync(projectId, cancellationToken)) return null;
        return (await sprints.ListByProjectAsync(projectId, cancellationToken)).Select(ToDto).ToList();
    }

    public async Task<SprintDto?> GetAsync(int projectId, int id, CancellationToken cancellationToken = default)
        => await FindAsync(projectId, id, cancellationToken) is { } sprint ? ToDto(sprint) : null;

    public async Task<SprintDto?> CreateAsync(int projectId, SprintInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        if (!await projects.ExistsAsync(projectId, cancellationToken)) return null;

        var sprint = new Sprint { ProjectId = projectId, IsActive = false };
        Apply(sprint, data);

        sprints.Add(sprint);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(sprint);
    }

    public async Task<SprintDto?> UpdateAsync(int projectId, int id, SprintInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        var sprint = await FindAsync(projectId, id, cancellationToken);
        if (sprint is null) return null;

        Apply(sprint, data);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(sprint);
    }

    public async Task<bool> DeleteAsync(int projectId, int id, CancellationToken cancellationToken = default)
    {
        var sprint = await FindAsync(projectId, id, cancellationToken);
        if (sprint is null) return false;

        sprints.Remove(sprint);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SprintDto?> ActivateAsync(int projectId, int id, CancellationToken cancellationToken = default)
    {
        var sprint = await FindAsync(projectId, id, cancellationToken);
        if (sprint is null) return null;
        if (sprint.IsActive) return ToDto(sprint);

        // Dois SaveChanges na mesma transação: o índice único parcial (uma ativa por projeto)
        // é checado linha a linha, então a anterior precisa ser desativada primeiro.
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var current = await sprints.GetActiveByProjectAsync(projectId, ct);
            if (current is not null && current.Id != sprint.Id)
            {
                current.IsActive = false;
                await unitOfWork.SaveChangesAsync(ct);
            }

            sprint.IsActive = true;
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return ToDto(sprint);
    }

    private async Task<Sprint?> FindAsync(int projectId, int id, CancellationToken cancellationToken)
    {
        var sprint = await sprints.GetByIdAsync(id, cancellationToken);
        return sprint is not null && sprint.ProjectId == projectId ? sprint : null;
    }

    private static SprintDto ToDto(Sprint s)
        => new(s.Id, s.ProjectId, s.Name, s.StartDate, s.EndDate, s.IsActive);

    private static void Apply(Sprint sprint, SprintInput data)
    {
        sprint.Name = data.Name!;
        sprint.StartDate = data.StartDate!.Value.ToUniversalTime();
        sprint.EndDate = data.EndDate!.Value.ToUniversalTime();
    }

    /// <summary>Normaliza (trim) e valida; lança <see cref="ValidationFailedException"/>.</summary>
    private static SprintInput Validate(SprintInput? input)
    {
        var name = string.IsNullOrWhiteSpace(input?.Name) ? null : input.Name.Trim();
        var start = input?.StartDate;
        var end = input?.EndDate;
        var errors = new Dictionary<string, string[]>();

        if (name is null)
            errors[nameof(SprintInput.Name)] = ["Name é obrigatório."];
        else if (name.Length > NameMaxLength)
            errors[nameof(SprintInput.Name)] = [$"Name deve ter no máximo {NameMaxLength} caracteres."];

        if (start is null)
            errors[nameof(SprintInput.StartDate)] = ["StartDate é obrigatório."];

        if (end is null)
            errors[nameof(SprintInput.EndDate)] = ["EndDate é obrigatório."];
        else if (start is not null && end < start)
            errors[nameof(SprintInput.EndDate)] = ["EndDate deve ser maior ou igual a StartDate."];

        if (errors.Count > 0) throw new ValidationFailedException(errors);
        return new SprintInput(name, start, end);
    }
}
