using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOps.Core.Entities;

namespace MOps.Infrastructure.Persistence.Configurations;

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> b)
    {
        b.ToTable("work_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(300);
        b.Property(x => x.Description); // Markdown, sem limite (text)
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();

        b.HasOne(x => x.Project)
            .WithMany(p => p.WorkItems)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Sprint)
            .WithMany(s => s.WorkItems)
            .HasForeignKey(x => x.SprintId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.ProjectId, x.Status });
        b.HasIndex(x => x.SprintId);
    }
}
