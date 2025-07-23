using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Kafka;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Proto;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbContextConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaskNoteDbContext>(options =>
    options.UseNpgsql(dbContextConnection));

builder.Services.AddScoped<ITaskNoteRepository, TaskNoteRepository>();

var kafkaConnection = Environment.GetEnvironmentVariable("KAFKA_BROKER") ?? builder.Configuration["KafkaConnection"];
builder.Services.AddSingleton<ITaskNoteEventPublisher>(sp =>
    new KafkaTaskNoteEventPublisher(
        kafkaConnection,
        "task-events")
    );

builder.Services.AddScoped<TaskNoteEventGrpc.TaskNoteEventGrpcClient>(sp =>
{
    var grpcServerAddress = Environment.GetEnvironmentVariable("GRPC_SERVER_ADDRESS")?? builder.Configuration["GRPCConnection"];
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    var channel = GrpcChannel.ForAddress(grpcServerAddress, new GrpcChannelOptions { HttpHandler = handler });
    return new TaskNoteEventGrpc.TaskNoteEventGrpcClient(channel);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API V1");
    c.RoutePrefix = string.Empty;
});

app.UseRouting();
app.MapControllers();

app.Run();