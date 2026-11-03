using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOps.Core.Entities;

namespace MOps.Infrastructure.Persistence.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> b)
    {
        b.ToTable("sprints");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.StartDate).IsRequired();
        b.Property(x => x.EndDate).IsRequired();

        b.HasOne(x => x.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.ProjectId).IsUnique().HasFilter("\"IsActive\"");
    }
}
