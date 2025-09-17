namespace TaskManagement.Core.Models
{
    public enum TaskNoteStatus
    {
        New,
        InProgress,
        Completed
    }

    public enum TaskNotePriority
    {
        Low,
        Medium,
        Hight
    }

    public class TaskNote
    {
        public Guid Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Implementer { get; set; } = string.Empty;
        public TaskNoteStatus Status { get; set; }
        public TaskNotePriority Priority { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid? ParentId { get; set; }
        public TaskNote? Parent { get; set; }
        public ICollection<TaskNote> Children { get; set; } = new List<TaskNote>();
        public ICollection<TaskNote> RelatedTasks { get; set; } = new List<TaskNote>();
    }
}


