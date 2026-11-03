using MOps.Core.Enums;

namespace MOps.Core.Entities;

public class WorkItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? SprintId { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Conteúdo em Markdown.</summary>
    public string? Description { get; set; }
    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.ToDo;
    public Priority Priority { get; set; } = Priority.Medium;
    /// <summary>Sempre em UTC.</summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Sempre em UTC.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    public Project? Project { get; set; }
    public Sprint? Sprint { get; set; }
    public ICollection<CommitRecord> Commits { get; set; } = new List<CommitRecord>();
}
