namespace ProjectManagementTools.Core.Entities
{
    public class TaskTag
    {
        public Guid TaskId { get; set; }
        public virtual TaskItem Task { get; set; } = default!;

        public Guid TagId { get; set; }
        public virtual Tag Tag { get; set; } = default!;

        public DateTime TaggedDate { get; set; } = DateTime.UtcNow;
        public string? TaggedBy { get; set; }
    }
}
