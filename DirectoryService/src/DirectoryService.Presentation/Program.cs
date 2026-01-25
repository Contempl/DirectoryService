using DirectoryService.Application.DependencyInjection;
using DirectoryService.Infrastructure.DI;
using DirectoryService.Presentation.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseSerilogRequestLogging(); 

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}