using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
    {
        public void Configure(EntityTypeBuilder<TimeLog> builder)
        {
            builder.ToTable("TimeLogs");

            builder.HasKey(tl => tl.Id);

            builder.Property(tl => tl.WorkDescription)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(tl => tl.HourlyRate)
                .HasColumnType("decimal(10,2)");

            builder.Property(tl => tl.TotalCost)
                .HasColumnType("decimal(12,2)");

            builder.Property(tl => tl.BillingCategory)
                .HasMaxLength(100);

            // Relationships
            builder.HasOne(tl => tl.User)
                .WithMany(u => u.TimeLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tl => tl.Task)
                .WithMany(t => t.TimeLogs)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tl => tl.SubTask)
                .WithMany(st => st.TimeLogs)
                .HasForeignKey(tl => tl.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter to exclude deleted time logs
            builder.HasQueryFilter(tl => !tl.IsDeleted);
        }
    }

}
