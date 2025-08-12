using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TaskKey)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(t => t.TaskKey)
                .IsUnique();

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(t => t.Description)
                .HasMaxLength(5000);

            builder.Property(t => t.Priority)
                .HasConversion<int>();

            builder.Property(t => t.Status)
                .HasConversion<int>();

            builder.Property(t => t.Type)
                .HasConversion<int>();

            builder.Property(t => t.EstimatedHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(t => t.ActualHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(t => t.OverTimeHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(t => t.ProgressPercentage)
                .HasColumnType("decimal(5,2)");

            // Self-referencing relationship for parent-child tasks
            builder.HasOne(t => t.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            builder.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Sprint)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(t => t.AssignedUsers)
                .WithOne(ta => ta.Task)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Tags)
                .WithOne(tt => tt.Task)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Comments)
                .WithOne(c => c.Task)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Attachments)
                .WithOne(a => a.Task)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TimeLogs)
                .WithOne(tl => tl.Task)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.DirectSubTasks)
                .WithOne(st => st.Task)
                .HasForeignKey(st => st.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter to exclude deleted tasks
            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
