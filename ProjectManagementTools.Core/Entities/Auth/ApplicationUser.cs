using Microsoft.AspNetCore.Identity;
using ProjectManagementTools.Core.Entities.Projects;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace ProjectManagementTools.Core.Entities.Auth
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? Company { get; set; }
        public string? TimeZone { get; set; } = "UTC";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual List<ProjectOwner> OwnedProjects { get; set; } = new();
        public virtual List<TaskAssignment> TaskAssignments { get; set; } = new();
        public virtual List<SubTaskAssignment> SubTaskAssignments { get; set; } = new();
        public virtual List<Comment> Comments { get; set; } = new();
        public virtual List<Attachment> UploadedAttachments { get; set; } = new();
        public virtual List<TimeLog> TimeLogs { get; set; } = new();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
    }
}
