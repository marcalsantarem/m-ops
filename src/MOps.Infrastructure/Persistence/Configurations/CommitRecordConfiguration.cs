using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOps.Core.Entities;

namespace MOps.Infrastructure.Persistence.Configurations;

public class CommitRecordConfiguration : IEntityTypeConfiguration<CommitRecord>
{
    public void Configure(EntityTypeBuilder<CommitRecord> b)
    {
        b.ToTable("commit_records");
        b.HasKey(x => x.Id);
        b.Property(x => x.CommitHash).IsRequired().HasMaxLength(64); // SHA-1 (40) ou SHA-256 (64)
        b.Property(x => x.Message).IsRequired().HasMaxLength(4000);
        b.Property(x => x.Author).IsRequired().HasMaxLength(200);
        b.Property(x => x.CommittedAt).IsRequired();

        b.HasOne(x => x.WorkItem)
            .WithMany(w => w.Commits)
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.WorkItemId);
        b.HasIndex(x => x.CommitHash);
        // Evita duplicar o mesmo commit no mesmo work item (hook reenviado).
        b.HasIndex(x => new { x.WorkItemId, x.CommitHash }).IsUnique();
    }
}
