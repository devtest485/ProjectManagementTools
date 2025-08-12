using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
    {
        public void Configure(EntityTypeBuilder<TaskAssignment> builder)
        {
            builder.ToTable("TaskAssignments");

            builder.HasKey(ta => ta.Id);

            builder.Property(ta => ta.Role)
                .HasConversion<int>();

            builder.Property(ta => ta.EstimatedHours)
                .HasColumnType("decimal(8,2)");

            // Composite index for task and user
            builder.HasIndex(ta => new { ta.TaskId, ta.UserId })
                .IsUnique();

            // Relationships
            builder.HasOne(ta => ta.Task)
                .WithMany(t => t.AssignedUsers)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ta => ta.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
