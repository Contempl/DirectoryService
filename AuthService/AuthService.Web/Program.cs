using AuthService.Application;
using AuthService.Configuration;
using AuthService.Core.EmailSender;
using AuthService.Core.Identity;
using Framework.Middleware;
using Framework.Response;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
else
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddDatabaseWithLogging(builder.Configuration);

builder.Services.AddConfiguration(builder.Configuration);

builder.Services.AddHostedService<SeedDataService>();

builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandlingMiddleware();

app.UseCors(bld =>
{
    bld.WithOrigins("http://localhost:3000")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseMigrations();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var apiGroup = app.MapGroup("/api").WithOpenApi();
app.MapEndpoints(apiGroup);

app.Run();
