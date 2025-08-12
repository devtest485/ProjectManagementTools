using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Entities.Projects;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementTools.Core.Entities
{
    public class Attachment
    {
        public Guid Id { get; set; }

        // Associated entities (one of these will be set)
        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public Guid? TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public Guid? SubTaskId { get; set; }
        public virtual SubTask? SubTask { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Guid UploadedBy { get; set; }
        public virtual ApplicationUser UploadedByUser { get; set; } = default!;

        // File metadata
        public string? Description { get; set; }
        public string? FileHash { get; set; } // For duplicate detection
        public bool IsImage { get; set; } = false;
        public bool IsDocument { get; set; } = false;
        public bool IsArchive { get; set; } = false;

        // Flags
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Computed Properties
        public string FileExtension => Path.GetExtension(FileName).ToLowerInvariant();
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
