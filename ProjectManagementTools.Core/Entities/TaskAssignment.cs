using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Enums;

namespace ProjectManagementTools.Core.Entities
{
    public class TaskAssignment
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }
        public virtual TaskItem Task { get; set; } = default!;

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = default!;

        public AssignmentRole Role { get; set; } = AssignmentRole.Developer;
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? RemovedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Assignment metadata
        public string? AssignedBy { get; set; }
        public string? Notes { get; set; }
        public decimal? EstimatedHours { get; set; }
    }
}
