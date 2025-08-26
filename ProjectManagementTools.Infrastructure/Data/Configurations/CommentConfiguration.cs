using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities;
using ProjectManagementTools.Core.Entities.Projects;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(c => c.MentionedUsers)
                .HasMaxLength(1000);

            builder.Property(c => c.AttachedFiles)
                .HasMaxLength(2000);

            // Self-referencing relationship for replies
            builder.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            builder.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Project)
                .WithMany()
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.SubTask)
                .WithMany(st => st.Comments)
                .HasForeignKey(c => c.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter to exclude deleted comments
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }

    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("ActivityLogs");

            builder.HasKey(al => al.Id);

            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(al => al.IPAddress)
                .HasMaxLength(50);

            builder.Property(al => al.UserAgent)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(al => al.Project)
                .WithMany()
                .HasForeignKey(al => al.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(al => al.Task)
                .WithMany()
                .HasForeignKey(al => al.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.Type)
                .HasMaxLength(50)
                .HasDefaultValue("Info");

            builder.Property(n => n.RelatedEntityType)
                .HasMaxLength(50);

            // Relationships
            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter to exclude deleted notifications
            builder.HasQueryFilter(n => !n.IsDeleted);
        }
    }

    public class ProjectOwnerConfiguration : IEntityTypeConfiguration<ProjectOwner>
    {
        public void Configure(EntityTypeBuilder<ProjectOwner> builder)
        {
            builder.ToTable("ProjectOwners");

            builder.HasKey(po => po.Id);

            builder.Property(po => po.Role)
                .HasConversion<int>();

            // Composite index for project and user
            builder.HasIndex(po => new { po.ProjectId, po.UserId })
                .IsUnique();

            // Relationships
            builder.HasOne(po => po.Project)
                .WithMany(p => p.Owners)
                .HasForeignKey(po => po.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(po => po.User)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(po => po.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#007acc");

            // Relationships
            builder.HasOne(c => c.Project)
                .WithMany(p => p.Categories)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Tasks)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.SubTasks)
                .WithOne(st => st.Category)
                .HasForeignKey(st => st.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
