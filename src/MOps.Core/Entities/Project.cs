namespace MOps.Core.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? GitRepositoryUrl { get; set; }
    /// <summary>Sempre em UTC.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
