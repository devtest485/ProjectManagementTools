using ProjectManagementTools.Core.Entities.Projects;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementTools.Core.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(7)]
        public string Color { get; set; } = "#007acc"; // Hex color

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual List<TaskItem> Tasks { get; set; } = new();
        public virtual List<SubTask> SubTasks { get; set; } = new();
    }
}
