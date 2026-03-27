using AuthService.Configuration;
using Framework.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddDatabaseWithLogging(builder.Configuration);

builder.Services.AddIdentityProvider(builder.Configuration);

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

app.Run();
