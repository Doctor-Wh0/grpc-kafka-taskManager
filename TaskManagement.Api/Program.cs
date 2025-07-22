using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Kafka;

//using TaskManagement.Infrastructure.Kafka;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Proto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TaskNoteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<ITaskNoteRepository, TaskNoteRepository>();

builder.Services.AddSingleton<ITaskNoteEventPublisher>(sp =>
    new KafkaTaskNoteEventPublisher(
        Environment.GetEnvironmentVariable("KAFKA_BROKER") ?? "localhost:9092",
        "task-events")
    );

builder.Services.AddScoped<TaskNoteEventGrpc.TaskNoteEventGrpcClient>(sp =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    var channel = GrpcChannel.ForAddress("http://grpc:5001", new GrpcChannelOptions { HttpHandler = handler });
    return new TaskNoteEventGrpc.TaskNoteEventGrpcClient(channel);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API V1");
    c.RoutePrefix = string.Empty;
});

app.UseRouting();
app.MapControllers();

app.Run();