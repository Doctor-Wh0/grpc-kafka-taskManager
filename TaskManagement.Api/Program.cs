using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskManagement.Core.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Kafka;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Proto;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{ 
    // Настройка JWT для Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var dbContextConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaskNoteDbContext>(options =>
    options.UseNpgsql(dbContextConnection));

builder.Services.AddScoped<ITaskNoteRepository, TaskNoteRepository>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Рекомендации по паролям
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;  // Для простоты, но в прод — true
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.SignIn.RequireConfirmedEmail = false;  // Для теста; в прод — true
})
.AddEntityFrameworkStores<TaskNoteDbContext>()
.AddDefaultTokenProviders();  // Для email confirm, etc.

// Добавляем JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(1)  // Минимальный skew
        };
    });

builder.Services.AddAuthorization();  // Для ролей/политик

var kafkaConnection = Environment.GetEnvironmentVariable("KAFKA_BROKER") ?? builder.Configuration["KafkaConnection"]!;
builder.Services.AddSingleton<ITaskNoteEventPublisher>(sp =>
    new KafkaTaskNoteEventPublisher(
        kafkaConnection,
        "task-events")
    );

builder.Services.AddScoped<TaskNoteEventGrpc.TaskNoteEventGrpcClient>(sp =>
{
    var grpcServerAddress = Environment.GetEnvironmentVariable("GRPC_SERVER_ADDRESS")?? builder.Configuration["GRPCConnection"]!;
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    var channel = GrpcChannel.ForAddress(grpcServerAddress, new GrpcChannelOptions { HttpHandler = handler });
    return new TaskNoteEventGrpc.TaskNoteEventGrpcClient(channel);
});

var app = builder.Build();

// Применение миграций
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskNoteDbContext>();
    try
    {
        dbContext.Database.Migrate(); // Применяет все миграции
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        throw; // Для dev; в prod можно просто логировать
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("http://localhost:5030/swagger/v1/swagger.json", "Task Management API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();