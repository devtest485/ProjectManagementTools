using ProjectManagementTools.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace ProjectManagementTools.Core.Entities
{
    public class SubTask
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }
        public virtual TaskItem Task { get; set; } = default!;

        // Sprint integration
        public Guid? SprintId { get; set; }
        public virtual Sprint? Sprint { get; set; }

        // Category
        public Guid? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.NotStarted;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal OverTimeHours { get; set; }
        public int StoryPoints { get; set; }
        public decimal ProgressPercentage { get; set; }

        // Flags
        public bool IsBlocked { get; set; } = false;
        public string? BlockedReason { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;

        // Navigation properties
        public virtual List<SubTaskAssignment> AssignedUsers { get; set; } = new();
        public virtual List<SubTaskTag> Tags { get; set; } = new();
        public virtual List<Comment> Comments { get; set; } = new();
        public virtual List<Attachment> Attachments { get; set; } = new();
        public virtual List<TimeLog> TimeLogs { get; set; } = new();

        // Computed Properties
        public bool IsOverdue => EndDate < DateTime.UtcNow && Status != Enums.TaskStatus.Done;
        public bool IsCompleted => Status == Enums.TaskStatus.Done;
    }
}
