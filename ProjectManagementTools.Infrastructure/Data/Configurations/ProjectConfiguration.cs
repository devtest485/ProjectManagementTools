using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities.Projects;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.ProjectKey)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(p => p.ProjectKey)
                .IsUnique();

            builder.Property(p => p.Priority)
                .HasConversion<int>();

            builder.Property(p => p.Status)
                .HasConversion<int>();

            builder.Property(p => p.EstimatedHours)
                .HasColumnType("decimal(10,2)");

            builder.Property(p => p.ActualHours)
                .HasColumnType("decimal(10,2)");

            builder.Property(p => p.OverTimeHours)
                .HasColumnType("decimal(10,2)");

            builder.Property(p => p.ProgressPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(p => p.Budget)
                .HasColumnType("decimal(12,2)");

            builder.Property(p => p.ActualCost)
                .HasColumnType("decimal(12,2)");

            builder.Property(p => p.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            // Relationships
            builder.HasMany(p => p.Owners)
                .WithOne(po => po.Project)
                .HasForeignKey(po => po.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Sprints)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Categories)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Tags)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter to exclude deleted projects
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
