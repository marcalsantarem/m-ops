using MOps.Core.Dtos;
using MOps.Core.Entities;
using MOps.Core.Enums;
using MOps.Core.Repositories;

namespace MOps.Core.Services;

public sealed class WorkItemService(
    IProjectRepository projects,
    ISprintRepository sprints,
    IWorkItemRepository workItems,
    IUnitOfWork unitOfWork) : IWorkItemService
{
    public const int TitleMaxLength = 300;

    public async Task<IReadOnlyList<WorkItemDto>?> ListAsync(int projectId, WorkItemQuery query, CancellationToken cancellationToken = default)
    {
        if (!await projects.ExistsAsync(projectId, cancellationToken)) return null;

        var filter = new WorkItemFilter
        {
            ProjectId = projectId,
            SprintId = query.SprintId,
            OnlyWithoutSprint = query.OnlyWithoutSprint,
            Status = query.Status,
            Type = query.Type,
            Keyword = string.IsNullOrWhiteSpace(query.Keyword) ? null : query.Keyword.Trim()
        };
        return (await workItems.SearchAsync(filter, cancellationToken)).Select(ToDto).ToList();
    }

    public async Task<WorkItemDto?> GetAsync(int id, CancellationToken cancellationToken = default)
        => await workItems.GetByIdAsync(id, cancellationToken) is { } item ? ToDto(item) : null;

    public async Task<WorkItemDto?> CreateAsync(int projectId, WorkItemInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        if (!await projects.ExistsAsync(projectId, cancellationToken)) return null;
        await ValidateSprintAsync(projectId, data.SprintId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var item = new WorkItem
        {
            ProjectId = projectId,
            Status = data.Status ?? WorkItemStatus.ToDo,
            Priority = data.Priority ?? Priority.Medium,
            CreatedAt = now,
            UpdatedAt = now
        };
        Apply(item, data);

        workItems.Add(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task<WorkItemDto?> UpdateAsync(int id, WorkItemInput input, CancellationToken cancellationToken = default)
    {
        var data = Validate(input);
        var item = await workItems.GetByIdAsync(id, cancellationToken);
        if (item is null) return null;
        await ValidateSprintAsync(item.ProjectId, data.SprintId, cancellationToken);

        Apply(item, data);
        if (data.Status is { } status) item.Status = status;
        if (data.Priority is { } priority) item.Priority = priority;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await workItems.GetByIdAsync(id, cancellationToken);
        if (item is null) return false;

        workItems.Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateSprintAsync(int projectId, int? sprintId, CancellationToken cancellationToken)
    {
        if (sprintId is null) return;
        var sprint = await sprints.GetByIdAsync(sprintId.Value, cancellationToken);
        if (sprint is null || sprint.ProjectId != projectId)
            throw new ValidationFailedException(new Dictionary<string, string[]>
            {
                [nameof(WorkItemInput.SprintId)] = ["SprintId não existe neste projeto."]
            });
    }

    private static WorkItemDto ToDto(WorkItem w)
        => new(w.Id, w.ProjectId, w.SprintId, w.Title, w.Description, w.Type, w.Status, w.Priority, w.CreatedAt, w.UpdatedAt);

    private static void Apply(WorkItem item, WorkItemInput data)
    {
        item.Title = data.Title!;
        item.Description = data.Description;
        item.Type = data.Type!.Value;
        item.SprintId = data.SprintId;
    }

    /// <summary>Normaliza (trim) e valida; lança <see cref="ValidationFailedException"/>.</summary>
    private static WorkItemInput Validate(WorkItemInput? input)
    {
        var title = string.IsNullOrWhiteSpace(input?.Title) ? null : input.Title.Trim();
        var description = string.IsNullOrWhiteSpace(input?.Description) ? null : input.Description;
        var errors = new Dictionary<string, string[]>();

        if (title is null)
            errors[nameof(WorkItemInput.Title)] = ["Title é obrigatório."];
        else if (title.Length > TitleMaxLength)
            errors[nameof(WorkItemInput.Title)] = [$"Title deve ter no máximo {TitleMaxLength} caracteres."];

        if (input?.Type is null)
            errors[nameof(WorkItemInput.Type)] = ["Type é obrigatório."];
        else if (!Enum.IsDefined(input.Type.Value))
            errors[nameof(WorkItemInput.Type)] = ["Type inválido."];

        if (input?.Status is { } status && !Enum.IsDefined(status))
            errors[nameof(WorkItemInput.Status)] = ["Status inválido."];

        if (input?.Priority is { } priority && !Enum.IsDefined(priority))
            errors[nameof(WorkItemInput.Priority)] = ["Priority inválido."];

        if (errors.Count > 0) throw new ValidationFailedException(errors);
        return input! with { Title = title, Description = description };
    }
}
