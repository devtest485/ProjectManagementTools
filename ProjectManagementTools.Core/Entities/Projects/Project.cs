using ProjectManagementTools.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace ProjectManagementTools.Core.Entities.Projects
{
    public class Project
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ProjectKey { get; set; } = string.Empty; // e.g., "PROJ-001"

        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal OverTimeHours { get; set; }
        public decimal ProgressPercentage { get; set; }

        // Budget and Cost Tracking
        public decimal? Budget { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Currency { get; set; } = "USD";

        // Project Settings
        public bool IsPublic { get; set; } = false;
        public bool AllowGuestAccess { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;

        // Navigation properties
        public virtual List<ProjectOwner> Owners { get; set; } = new();
        public virtual List<Sprint> Sprints { get; set; } = new();
        public virtual List<TaskItem> Tasks { get; set; } = new();
        public virtual List<Category> Categories { get; set; } = new();
        public virtual List<Tag> Tags { get; set; } = new();
        public virtual List<Attachment> Attachments { get; set; } = new();

        // Computed Properties
        public bool IsOverdue => EndDate < DateTime.UtcNow && Status != ProjectStatus.Completed;
        public int TotalTasks => Tasks?.Count ?? 0;
        public int CompletedTasks => Tasks?.Count(t => t.Status == Enums.TaskStatus.Done) ?? 0;
        public decimal CompletionRate => TotalTasks > 0 ? (decimal)CompletedTasks / TotalTasks * 100 : 0;
    }
}
