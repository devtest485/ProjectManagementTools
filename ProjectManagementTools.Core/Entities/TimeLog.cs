using ProjectManagementTools.Core.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementTools.Core.Entities
{
    public class TimeLog
    {
        public Guid Id { get; set; }

        // Associated entities (one of these will be set)
        public Guid? TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public Guid? SubTaskId { get; set; }
        public virtual SubTask? SubTask { get; set; }

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = default!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal HoursWorked { get; set; }

        [StringLength(1000)]
        public string WorkDescription { get; set; } = string.Empty;

        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Time log metadata
        public bool IsBillable { get; set; } = true;
        public decimal? HourlyRate { get; set; }
        public decimal? TotalCost { get; set; }
        public string? BillingCategory { get; set; }

        // Timer functionality
        public bool IsRunning { get; set; } = false;
        public bool IsManualEntry { get; set; } = false;

        // Flags
        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Computed Properties
        public decimal CalculatedHours => EndTime.HasValue ?
            (decimal)(EndTime.Value - StartTime).TotalHours : 0;

        public decimal CalculatedCost => HourlyRate.HasValue ?
            HoursWorked * HourlyRate.Value : 0;
    }
}
