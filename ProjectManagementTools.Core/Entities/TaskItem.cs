using ProjectManagementTools.Core.Entities.Projects;
using ProjectManagementTools.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace ProjectManagementTools.Core.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TaskKey { get; set; } = string.Empty; // e.g., "PROJ-123"

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = default!;

        // Sprint integration
        public Guid? SprintId { get; set; }
        public virtual Sprint? Sprint { get; set; }

        // Hierarchy support
        public Guid? ParentTaskId { get; set; }
        public virtual TaskItem? ParentTask { get; set; }
        public virtual List<TaskItem> SubTasks { get; set; } = new();

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        // Classification
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.NotStarted;
        public TaskType Type { get; set; } = TaskType.Task;

        // Categories and Tags
        public Guid? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // Time tracking
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal OverTimeHours { get; set; }
        public int StoryPoints { get; set; }

        // Dates
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }

        // Progress tracking
        public decimal ProgressPercentage { get; set; }

        // Task metadata
        public string? AcceptanceCriteria { get; set; }
        public string? Definition { get; set; }
        public string? BusinessValue { get; set; }

        // Flags
        public bool IsBlocked { get; set; } = false;
        public string? BlockedReason { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsArchived { get; set; } = false;

        // Navigation properties
        public virtual List<TaskAssignment> AssignedUsers { get; set; } = new();
        public virtual List<TaskTag> Tags { get; set; } = new();
        public virtual List<Comment> Comments { get; set; } = new();
        public virtual List<Attachment> Attachments { get; set; } = new();
        public virtual List<TimeLog> TimeLogs { get; set; } = new();
        public virtual List<SubTask> DirectSubTasks { get; set; } = new();

        // Computed Properties
        public bool IsOverdue => EndDate < DateTime.UtcNow && Status != Enums.TaskStatus.Done;
        public bool IsEpic => Type == TaskType.Epic;
        public bool IsStory => Type == TaskType.Story;
        public bool IsBug => Type == TaskType.Bug;
        public int TotalSubTasks => DirectSubTasks?.Count ?? 0;
        public int CompletedSubTasks => DirectSubTasks?.Count(st => st.Status == Enums.TaskStatus.Done) ?? 0;
        public decimal SubTaskCompletionRate => TotalSubTasks > 0 ? (decimal)CompletedSubTasks / TotalSubTasks * 100 : 0;
    }
}
