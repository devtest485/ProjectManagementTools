using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities;
using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Entities.Projects;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class SubTaskTagConfiguration : IEntityTypeConfiguration<SubTaskTag>
    {
        public void Configure(EntityTypeBuilder<SubTaskTag> builder)
        {
            builder.ToTable("SubTaskTags");

            // Composite primary key
            builder.HasKey(st => new { st.SubTaskId, st.TagId });

            // Relationships
            builder.HasOne(st => st.SubTask)
                .WithMany(s => s.Tags)
                .HasForeignKey(st => st.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(st => st.Tag)
                .WithMany(t => t.SubTaskTags)
                .HasForeignKey(st => st.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Additional properties
            builder.Property(st => st.TaggedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(st => st.TaggedBy)
                .HasMaxLength(100);
        }
    }

    public class SubTaskAssignmentConfiguration : IEntityTypeConfiguration<SubTaskAssignment>
    {
        public void Configure(EntityTypeBuilder<SubTaskAssignment> builder)
        {
            builder.ToTable("SubTaskAssignments");

            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.Role)
                .HasConversion<int>();

            builder.Property(sa => sa.EstimatedHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(sa => sa.AssignedBy)
                .HasMaxLength(100);

            builder.Property(sa => sa.Notes)
                .HasMaxLength(1000);

            // Composite index for subtask and user
            builder.HasIndex(sa => new { sa.SubTaskId, sa.UserId })
                .IsUnique();

            // Relationships
            builder.HasOne(sa => sa.SubTask)
                .WithMany(s => s.AssignedUsers)
                .HasForeignKey(sa => sa.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sa => sa.User)
                .WithMany(u => u.SubTaskAssignments)
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class SubTaskConfiguration : IEntityTypeConfiguration<SubTask>
    {
        public void Configure(EntityTypeBuilder<SubTask> builder)
        {
            builder.ToTable("SubTasks");

            builder.HasKey(st => st.Id);

            builder.Property(st => st.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(st => st.Description)
                .HasMaxLength(2000);

            builder.Property(st => st.Priority)
                .HasConversion<int>();

            builder.Property(st => st.Status)
                .HasConversion<int>();

            builder.Property(st => st.EstimatedHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(st => st.ActualHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(st => st.OverTimeHours)
                .HasColumnType("decimal(8,2)");

            builder.Property(st => st.ProgressPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(st => st.BlockedReason)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(st => st.Task)
                .WithMany(t => t.DirectSubTasks)
                .HasForeignKey(st => st.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(st => st.Sprint)
                .WithMany(s => s.SubTasks)
                .HasForeignKey(st => st.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(st => st.Category)
                .WithMany(c => c.SubTasks)
                .HasForeignKey(st => st.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(st => st.AssignedUsers)
                .WithOne(sa => sa.SubTask)
                .HasForeignKey(sa => sa.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(st => st.Tags)
                .WithOne(st => st.SubTask)
                .HasForeignKey(st => st.SubTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(st => st.Comments)
                .WithOne(c => c.SubTask)
                .HasForeignKey(c => c.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(st => st.Attachments)
                .WithOne(a => a.SubTask)
                .HasForeignKey(a => a.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(st => st.TimeLogs)
                .WithOne(tl => tl.SubTask)
                .HasForeignKey(tl => tl.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter to exclude deleted subtasks
            builder.HasQueryFilter(st => !st.IsDeleted);
        }
    }

    public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
    {
        public void Configure(EntityTypeBuilder<Sprint> builder)
        {
            builder.ToTable("Sprints");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Goal)
                .HasMaxLength(500);

            builder.Property(s => s.Description)
                .HasMaxLength(2000);

            builder.Property(s => s.Status)
                .HasConversion<int>();

            builder.Property(s => s.PlannedVelocity)
                .HasColumnType("decimal(8,2)");

            builder.Property(s => s.ActualVelocity)
                .HasColumnType("decimal(8,2)");

            builder.Property(s => s.PlannedCapacity)
                .HasColumnType("decimal(10,2)");

            builder.Property(s => s.ActualCapacity)
                .HasColumnType("decimal(10,2)");

            // Relationships
            builder.HasOne(s => s.Project)
                .WithMany(p => p.Sprints)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Tasks)
                .WithOne(t => t.Sprint)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(s => s.SubTasks)
                .WithOne(st => st.Sprint)
                .HasForeignKey(st => st.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            // Global query filter to exclude deleted sprints
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }

    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#007acc");

            builder.Property(t => t.Description)
                .HasMaxLength(200);

            // Relationships
            builder.HasOne(t => t.Project)
                .WithMany(p => p.Tags)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TaskTags)
                .WithOne(tt => tt.Tag)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.SubTaskTags)
                .WithOne(st => st.Tag)
                .HasForeignKey(st => st.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}