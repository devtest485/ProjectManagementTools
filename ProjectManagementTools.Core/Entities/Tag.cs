using ProjectManagementTools.Core.Entities.Projects;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementTools.Core.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(7)]
        public string Color { get; set; } = "#007acc"; // Hex color

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual List<TaskTag> TaskTags { get; set; } = new();
        public virtual List<SubTaskTag> SubTaskTags { get; set; } = new();
    }
}
