namespace TaskManagement.Core.Models;

public enum TaskNoteStatus
{
    New,
    InProgress,
    Completed
}

public class TaskNote
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskNoteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
