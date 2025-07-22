using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagement.GrpcService;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using TaskManagement.Proto;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Настройка gRPC
    options.ListenAnyIP(5001, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddGrpc();
var app = builder.Build();
app.UseHttpsRedirection();
app.MapGrpcService<TaskNoteEventService>();

await app.RunAsync();