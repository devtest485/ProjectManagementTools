using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProjectManagementTools.Core.Entities;
using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Entities.Projects;
using System.Reflection;

namespace ProjectManagementTools.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Core Entities
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectOwner> ProjectOwners { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }

        // Task Entities
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<SubTaskAssignment> SubTaskAssignments { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }
        public DbSet<SubTaskTag> SubTaskTags { get; set; }

        // Communication & Collaboration
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }

        // Audit & Tracking
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Additional configurations
            ConfigureIdentityTables(builder);
            ConfigureGlobalFilters(builder);
            ConfigureIndexes(builder);
            SeedData(builder);
        }

        private static void ConfigureIdentityTables(ModelBuilder builder)
        {
            // Rename Identity tables to match our naming convention
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        }

        private static void ConfigureGlobalFilters(ModelBuilder builder)
        {
            // Global query filters for soft delete
            builder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<TaskItem>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<SubTask>().HasQueryFilter(st => !st.IsDeleted);
            builder.Entity<Sprint>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(u => u.IsActive);
            builder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Attachment>().HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<TimeLog>().HasQueryFilter(tl => !tl.IsDeleted);
            builder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
        }

        private static void ConfigureIndexes(ModelBuilder builder)
        {
            // Performance indexes
            builder.Entity<Project>()
                .HasIndex(p => new { p.Status, p.IsArchived });

            builder.Entity<TaskItem>()
                .HasIndex(t => new { t.Status, t.Priority })
                .HasDatabaseName("IX_Tasks_Status_Priority");

            builder.Entity<TaskItem>()
                .HasIndex(t => new { t.ProjectId, t.SprintId })
                .HasDatabaseName("IX_Tasks_Project_Sprint");

            builder.Entity<SubTask>()
                .HasIndex(st => new { st.TaskId, st.Status })
                .HasDatabaseName("IX_SubTasks_Task_Status");

            builder.Entity<TimeLog>()
                .HasIndex(tl => new { tl.UserId, tl.LogDate })
                .HasDatabaseName("IX_TimeLogs_User_Date");

            builder.Entity<ActivityLog>()
                .HasIndex(al => new { al.ProjectId, al.CreatedAt })
                .HasDatabaseName("IX_ActivityLogs_Project_Date");

            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("IX_Notifications_User_Read");
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Seed default roles
            var adminRoleId = Guid.NewGuid();
            var projectOwnerRoleId = Guid.NewGuid();
            var memberRoleId = Guid.NewGuid();
            var viewerRoleId = Guid.NewGuid();

            builder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid>
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole<Guid>
                {
                    Id = projectOwnerRoleId,
                    Name = "ProjectOwner",
                    NormalizedName = "PROJECTOWNER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole<Guid>
                {
                    Id = memberRoleId,
                    Name = "Member",
                    NormalizedName = "MEMBER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole<Guid>
                {
                    Id = viewerRoleId,
                    Name = "Viewer",
                    NormalizedName = "VIEWER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );
        }

        // Override SaveChanges to handle audit trails
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Add audit information
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Project project)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        project.UpdatedDate = DateTime.UtcNow;
                    }
                }

                if (entry.Entity is TaskItem task)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        task.UpdatedDate = DateTime.UtcNow;
                    }
                }

                if (entry.Entity is SubTask subTask)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        subTask.UpdatedDate = DateTime.UtcNow;
                    }
                }

                if (entry.Entity is Comment comment)
                {
                    if (entry.State == EntityState.Modified && !comment.IsDeleted)
                    {
                        comment.UpdatedAt = DateTime.UtcNow;
                        comment.IsEdited = true;
                    }
                }

                if (entry.Entity is TimeLog timeLog)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        timeLog.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
