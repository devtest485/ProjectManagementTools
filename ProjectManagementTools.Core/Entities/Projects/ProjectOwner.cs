using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Enums;

namespace ProjectManagementTools.Core.Entities.Projects
{
    public class ProjectOwner
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = default!;

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = default!;

        public OwnerRole Role { get; set; } = OwnerRole.CoOwner;
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? RemovedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Permissions
        public bool CanEditProject { get; set; } = true;
        public bool CanManageTeam { get; set; } = false;
        public bool CanManageSprints { get; set; } = false;
        public bool CanDeleteProject { get; set; } = false;
    }
}
