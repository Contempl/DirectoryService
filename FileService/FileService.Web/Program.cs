using FileService.Configuration;
using FileService.Infrastructure.Postgres;
using Wolverine.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddHealthChecks()
    .AddWolverine(tags: ["live", "ready"])
    .AddWolverineListeners(
        name: "wolverine-rabbitmq-listeners",
        filter: listener => listener.Uri.Scheme == "rabbitmq",
        tags: ["ready"]);
builder.Host.AddRabbitMqMessaging(builder.Configuration);

builder.Services.AddConfiguration(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var quartzDbInitializer = scope.ServiceProvider.GetRequiredService<QuartzDbInitializer>();
    await quartzDbInitializer.InitializeAsync();
}

app.MapHealthChecks("/health");

app.ConfigureApp();

await app.RunAsync();

public partial class Program { }
