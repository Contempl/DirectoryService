using FileService.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddConfiguration(builder.Configuration);

var app = builder.Build();

app.ConfigureApp();

app.Run();