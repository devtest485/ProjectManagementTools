using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Entities.Projects;
using System.ComponentModel.DataAnnotations;


namespace ProjectManagementTools.Core.Entities
{
    public class ActivityLog
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public Guid? TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, etc.

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // Project, Task, Sprint, etc.

        public Guid? EntityId { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }
    }
}
