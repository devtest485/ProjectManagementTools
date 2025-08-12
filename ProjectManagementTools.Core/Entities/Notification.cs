using ProjectManagementTools.Core.Entities.Auth;
using System.ComponentModel.DataAnnotations;


namespace ProjectManagementTools.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success

        public Guid? RelatedEntityId { get; set; }

        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        // Notification settings
        public bool SendEmail { get; set; } = false;
        public bool SendPush { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }
        public DateTime? PushSentAt { get; set; }

        // Computed Properties
        public bool IsUnread => !IsRead;
        public TimeSpan Age => DateTime.UtcNow - CreatedAt;
    }
}
