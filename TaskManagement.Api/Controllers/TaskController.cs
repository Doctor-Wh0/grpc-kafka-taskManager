using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;
using Google.Protobuf.WellKnownTypes;
using System.Data;
using TaskManagement.Proto;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskNoteRepository _taskRepository;
        private readonly ITaskNoteEventPublisher _eventPublisher;
        private readonly TaskNoteEventGrpc.TaskNoteEventGrpcClient _grpcClient;

        public TasksController(
            ITaskNoteRepository taskRepository,
            ITaskNoteEventPublisher eventPublisher,
            TaskNoteEventGrpc.TaskNoteEventGrpcClient grpcClient)
        {
            _taskRepository = taskRepository;
            _eventPublisher = eventPublisher;
            _grpcClient = grpcClient;
        }

        [HttpPost]
        public async Task<ActionResult<TaskNote>> Create(TaskNote taskDto)
        {
            var task = new TaskNote
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = taskDto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var createdTask = await _taskRepository.CreateAsync(task);

            await _eventPublisher.PublishTaskNoteCreateAsync(createdTask);
            await _grpcClient.OnTaskCreatedAsync(new TaskNoteEventRequest
            {
                Id = createdTask.Id.ToString(),
                Title = createdTask.Title,
                Description = createdTask.Description,
                Status = (Proto.TaskNoteStatus)createdTask.Status,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            });
            return Ok(createdTask);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskNote>> Get(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskNote>>> GetAll()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskNote>> Update(Guid id, TaskNote taskDto)
        {
            var task = new TaskNote
            {
                Id = id,
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = taskDto.Status,
                UpdatedAt = DateTime.UtcNow
            };
            var updatedTask = await _taskRepository.UpdateAsync(task);

            await _eventPublisher.PublishTaskNoteUpdateAsync(updatedTask);
            await _grpcClient.OnTaskUpdatedAsync(new TaskNoteEventRequest
            {
                Id = updatedTask.Id.ToString(),
                Title = updatedTask.Title,
                Description = updatedTask.Description,
                Status = (Proto.TaskNoteStatus)updatedTask.Status,
                CreatedAt = Timestamp.FromDateTime(updatedTask.CreatedAt),
                UpdatedAt = Timestamp.FromDateTime(updatedTask.UpdatedAt)
            });
            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _taskRepository.DeleteAsync(id);
            await _eventPublisher.PublishTaskNoteDeleteAsync(id);
            await _grpcClient.OnTaskDeletedAsync(new TaskNoteDeletedRequest { Id = id.ToString() });
            return NoContent();
        }
    }
}