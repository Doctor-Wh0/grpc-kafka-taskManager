using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;
using TaskManagement.Core.Models;

namespace TaskManagement.Infrastructure.Data
{
    public class TaskNoteDbContext : IdentityDbContext // Без <IdentityUser> для простоты, используем default
    {
        public TaskNoteDbContext(DbContextOptions<TaskNoteDbContext> options) : base(options)
        {
            // Database.EnsureCreated();
        }

        public DbSet<TaskNote> Tasks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TaskNote>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict);  // Не удалять родителя при удалении ребёнка

            builder.Entity<TaskNote>()
                .HasMany(t => t.RelatedTasks)
                .WithMany()  // Симметрично
                .UsingEntity<Dictionary<string, object>>(
                    "TaskNoteRelatedTasks",  // Связующая таблица
                    j => j.HasOne<TaskNote>().WithMany().HasForeignKey("RelatedTaskId"),
                    j => j.HasOne<TaskNote>().WithMany().HasForeignKey("TaskNoteId"),
                    j =>
                    {
                        j.HasKey("TaskNoteId", "RelatedTaskId");  // Composite PK
                        j.ToTable("TaskNoteRelatedTasks");  // Имя таблицы
                    });
        }
    }
}