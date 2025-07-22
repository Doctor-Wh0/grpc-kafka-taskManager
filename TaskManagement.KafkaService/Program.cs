using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using TaskManagement.Core.Models;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaConsumerService>();

var host = builder.Build();
await host.RunAsync();


public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<Null, string> _consumer;

    public KafkaConsumerService()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKER") ?? "localhost:9092",
            GroupId = "task-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Null, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("task-events");
        Console.WriteLine("Kafka Consumer started. Listening for messages...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);
                var taskEvent = JsonSerializer.Deserialize<TaskNoteEvent>(result.Message.Value);
                
                if (taskEvent != null)
                {
                    switch (taskEvent.EventType)
                    {
                        case "TaskCreated":
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Kafka: Task Created: ID={taskEvent.TaskNote.Id}, Title={taskEvent.TaskNote.Title}, Status={taskEvent.TaskNote.Status}");
                            Console.ResetColor();
                            break;
                        case "TaskUpdated":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Kafka: Task Updated: ID={taskEvent.TaskNote.Id}, Title={taskEvent.TaskNote.Title}, Status={taskEvent.TaskNote.Status}");
                            Console.ResetColor();
                            break;
                        case "TaskDeleted":
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Kafka: Task Deleted: ID={taskEvent.TaskNote.Id}");
                            Console.ResetColor();
                            break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _consumer.Close();
        }
        catch (ConsumeException e)
        {
            Console.WriteLine($"Consume error: {e.Error.Reason}");
        }

    }
}
