namespace MOps.Core.Entities;

public class Sprint
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Sempre em UTC.</summary>
    public DateTimeOffset StartDate { get; set; }
    /// <summary>Sempre em UTC.</summary>
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; }

    public Project? Project { get; set; }
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
