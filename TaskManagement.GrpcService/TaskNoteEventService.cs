using Grpc.Core;
using Microsoft.Extensions.Logging;
using TaskManagement.Proto;


namespace TaskManagement.GrpcService
{
    public class TaskNoteEventService : TaskNoteEventGrpc.TaskNoteEventGrpcBase
    {
        private readonly ILogger<TaskNoteEventService> _logger;

        public TaskNoteEventService(ILogger<TaskNoteEventService> logger)
        {
            _logger = logger;
        }

        public override Task<EmptyResponse> OnTaskCreated(TaskNoteEventRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC: Task Created: ID={request.Id}, Title={request.Title}, Status={request.Status}");
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> OnTaskUpdated(TaskNoteEventRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC: Task Updated: ID={request.Id}, Title={request.Title}, Status={request.Status}");
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> OnTaskDeleted(TaskNoteDeletedRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC: Task Deleted: ID={request.Id}");
            return Task.FromResult(new EmptyResponse());
        }
    }
}