using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Models;

namespace TaskManagement.Infrastructure.Data
{
    public class TaskNoteDbContext : DbContext
    {
        public TaskNoteDbContext(DbContextOptions<TaskNoteDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }



        public DbSet<TaskNote> Tasks { get; set; }
    }
}