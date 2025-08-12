using ProjectManagementTools.Core.Entities.Projects;
using ProjectManagementTools.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementTools.Core.Entities
{
    public class Sprint
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Sprint 1", "Q1 Development"

        [StringLength(500)]
        public string Goal { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        public SprintStatus Status { get; set; } = SprintStatus.Planning;

        // Sprint metrics
        public decimal PlannedVelocity { get; set; }
        public decimal ActualVelocity { get; set; }
        public int PlannedStoryPoints { get; set; }
        public int CompletedStoryPoints { get; set; }
        public int TotalStoryPoints { get; set; }

        // Sprint capacity (hours)
        public decimal PlannedCapacity { get; set; }
        public decimal ActualCapacity { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual List<TaskItem> Tasks { get; set; } = new();
        public virtual List<SubTask> SubTasks { get; set; } = new();

        // Computed Properties
        public bool IsActive => Status == SprintStatus.Active;
        public bool IsOverdue => EndDate < DateTime.UtcNow && Status == SprintStatus.Active;
        public decimal ProgressPercentage => TotalStoryPoints > 0 ? (decimal)CompletedStoryPoints / TotalStoryPoints * 100 : 0;
        public int DaysRemaining => Status == SprintStatus.Active ? Math.Max(0, (EndDate.Date - DateTime.UtcNow.Date).Days) : 0;
        public int TotalDays => (EndDate.Date - StartDate.Date).Days + 1;
    }
}
