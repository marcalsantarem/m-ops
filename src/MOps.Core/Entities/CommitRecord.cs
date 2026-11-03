namespace MOps.Core.Entities;

public class CommitRecord
{
    public int Id { get; set; }
    public int WorkItemId { get; set; }
    public string CommitHash { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    /// <summary>Sempre em UTC.</summary>
    public DateTimeOffset CommittedAt { get; set; }

    public WorkItem? WorkItem { get; set; }
}
