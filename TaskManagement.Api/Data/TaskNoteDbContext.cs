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
            // Database.EnsureCreated();  // Убери это — используй миграции для Identity!
        }

        public DbSet<TaskNote> Tasks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);  // Обязательно для Identity

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.HasIndex(rt => rt.Token).IsUnique();  // Быстрый поиск
                entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.IsUsed });
            });

            //builder.Entity<TaskNote>(entity =>
            //{
            //    entity.HasKey(e => e.Id);
            //    // ...
            //});
        }
    }
}