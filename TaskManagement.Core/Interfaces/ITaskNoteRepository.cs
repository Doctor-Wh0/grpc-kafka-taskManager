using TaskManagement.Core.Models;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskNoteRepository
    {
        Task<TaskNote> CreateAsync(TaskNote task);
        Task<TaskNote?> GetByIdAsync(Guid id);
        Task<IEnumerable<TaskNote>> GetAllAsync();
        Task<TaskNote> UpdateAsync(TaskNote task);
        Task DeleteAsync(Guid id);
    }
}