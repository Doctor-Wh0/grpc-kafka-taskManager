using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class TaskNoteRepository : ITaskNoteRepository
    {
        private readonly TaskNoteDbContext _context;

        public TaskNoteRepository(TaskNoteDbContext context)
        {
            _context = context;
        }

        public async Task<TaskNote> CreateAsync(TaskNote task)
        {
            task.Id = Guid.NewGuid();
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskNote?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<TaskNote>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<TaskNote> UpdateAsync(TaskNote task)
        {
            var existingTask = await _context.Tasks.FindAsync(task.Id) 
                ?? throw new Exception("Task not found");
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;
            existingTask.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id) 
                ?? throw new Exception("Task not found");
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}