using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Entities.Projects;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementTools.Core.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        // Threaded comments support
        public Guid? ParentCommentId { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual List<Comment> Replies { get; set; } = new();

        // Associated entities (one of these will be set)
        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public Guid? TaskId { get; set; }
        public virtual TaskItem? Task { get; set; }

        public Guid? SubTaskId { get; set; }
        public virtual SubTask? SubTask { get; set; }

        public Guid AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; } = default!;

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool IsEdited { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsPinned { get; set; } = false;

        // Comment metadata
        public string? MentionedUsers { get; set; } // JSON array of user IDs
        public string? AttachedFiles { get; set; } // JSON array of file references

        // Computed Properties
        public bool IsReply => ParentCommentId.HasValue;
        public int ReplyCount => Replies?.Count ?? 0;
    }
}
