using TaskManagement.Core.Models;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskNoteEventListener
    {
        void OnTaskCreated(TaskNote task);
        void OnTaskUpdated(TaskNote task);
        void OnTaskDeleted(Guid taskId);
    }
}