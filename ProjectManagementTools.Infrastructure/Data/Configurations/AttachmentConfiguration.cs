using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagementTools.Core.Entities;

namespace ProjectManagementTools.Infrastructure.Data.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("Attachments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.ContentType)
                .HasMaxLength(100);

            builder.Property(a => a.Description)
                .HasMaxLength(1000);

            builder.Property(a => a.FileHash)
                .HasMaxLength(128);

            // Relationships
            builder.HasOne(a => a.UploadedByUser)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(a => a.UploadedBy)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Project)
                .WithMany(p => p.Attachments)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.SubTask)
                .WithMany(st => st.Attachments)
                .HasForeignKey(a => a.SubTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter to exclude deleted attachments
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
