using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Models;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using Xunit;

namespace TaskManagement.Tests
{
    public class TaskNoteRepositoryTests
    {
        private TaskNoteDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<TaskNoteDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TaskNoteDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_Should_CreateTask()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new TaskNoteRepository(context);
            var task = new TaskNote { Title = "Test", Description = "Test Desc", Status = TaskNoteStatus.New };

            // Act
            var createdTask = await repository.CreateAsync(task);

            // Assert
            Assert.NotEqual(Guid.Empty, createdTask.Id);
            Assert.Equal("Test", createdTask.Title);
            Assert.NotEqual(default(DateTime), createdTask.CreatedAt);
        }
    }
}