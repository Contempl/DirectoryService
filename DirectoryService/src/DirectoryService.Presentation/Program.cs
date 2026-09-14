using DirectoryService.Application.DependencyInjection;
using DirectoryService.Infrastructure.DI;
using DirectoryService.Presentation.Configuration;
using Framework.Middleware;
using Serilog;
using Wolverine.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Host.AddRabbitMqMessaging(builder.Configuration);

// FS-14: Проверяем runtime Wolverine и принимающий RabbitMQ listener.
builder.Services.AddHealthChecks()
    .AddWolverine(tags: ["live", "ready"])
    .AddWolverineListeners(
        name: "wolverine-rabbitmq-listeners",
        filter: listener => listener.Uri.Scheme == "rabbitmq",
        tags: ["ready"]);

builder.Services.AddControllers();

var app = builder.Build();

app.UseDatabaseMigrations();

// FS-14: Общая проверка готовности messaging-инфраструктуры.
app.MapHealthChecks("/health");

app.UseExceptionHandlingMiddleware();

app.UseSerilogRequestLogging();

app.UseCors(bld =>
{
    bld.WithOrigins("http://localhost:3000")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseDevAuth();
app.UseMiddleware<UserScopedDataMiddleware>();
app.UseAuthorization();
        

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}
