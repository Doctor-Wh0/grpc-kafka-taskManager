using Confluent.Kafka;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;
using System.Text.Json;

namespace TaskManagement.Infrastructure.Kafka
{
    public class KafkaTaskNoteEventPublisher : ITaskNoteEventPublisher
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaTaskNoteEventPublisher(string bootstrapServers, string topic)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _topic = topic;
        }

        public async Task PublishTaskNoteCreateAsync(TaskNote taskNote)
        {
            var taskEvent = new TaskNoteEvent { EventType = "TaskCreated", TaskNote = taskNote };
            var message = JsonSerializer.Serialize(taskEvent);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
        }

        public async Task PublishTaskNoteUpdateAsync(TaskNote taskNote)
        {
            var taskEvent = new TaskNoteEvent { EventType = "TaskUpdated", TaskNote = taskNote };
            var message = JsonSerializer.Serialize(taskEvent);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
        }

        public async Task PublishTaskNoteDeleteAsync(Guid taskId)
        {
            var taskEvent = new TaskNoteEvent { EventType = "TaskDeleted", TaskNote = new TaskNote { Id = taskId } };
            var message = JsonSerializer.Serialize(taskEvent);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}