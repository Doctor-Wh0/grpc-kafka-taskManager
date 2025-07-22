namespace TaskManagement.Core.Models;

public class TaskNoteEvent
{
    public string EventType { get; set; } = string.Empty;
    public TaskNote TaskNote { get; set; } = new TaskNote();
}