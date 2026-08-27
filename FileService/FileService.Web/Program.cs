using FileService.Configuration;
using FileService.Infrastructure.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddConfiguration(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var quartzDbInitializer = scope.ServiceProvider.GetRequiredService<QuartzDbInitializer>();
    await quartzDbInitializer.InitializeAsync();
}

app.ConfigureApp();

await app.RunAsync();

public partial class Program { }
