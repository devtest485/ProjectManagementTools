namespace ProjectManagementTools.Core.Entities
{
    public class SubTaskTag
    {
        public Guid SubTaskId { get; set; }
        public virtual SubTask SubTask { get; set; } = default!;

        public Guid TagId { get; set; }
        public virtual Tag Tag { get; set; } = default!;

        public DateTime TaggedDate { get; set; } = DateTime.UtcNow;
        public string? TaggedBy { get; set; }
    }
}
