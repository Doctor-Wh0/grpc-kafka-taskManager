using TaskManagement.Core.Models;

namespace TaskManagement.Core.Interfaces;

public interface ITaskNoteEventPublisher
{
    public Task PublishTaskNoteCreateAsync(TaskNote taskNote);
    public Task PublishTaskNoteUpdateAsync(TaskNote taskNote);
    public Task PublishTaskNoteDeleteAsync(Guid taskNoteI);
}