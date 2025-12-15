using DirectoryService.Application.DependencyInjection;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.DI;
using DirectoryService.Presentation.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

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

app.Run();