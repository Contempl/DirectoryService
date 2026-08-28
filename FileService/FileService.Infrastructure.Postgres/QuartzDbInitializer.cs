using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FileService.Infrastructure.Postgres;

public sealed class QuartzDbInitializer(
    IConfiguration configuration,
    ILogger<QuartzDbInitializer> logger)
{
    private const string QuartzTableName = "qrtz_job_details";
    private const string SqlResourceName =
        "FileService.Infrastructure.Postgres.Scripts.quartz_tables.sql";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string is missing");

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        if (await QuartzSchemaExistsAsync(connection, cancellationToken))
        {
            logger.LogInformation("Quartz database schema already exists");
            return;
        }

        var sqlScript = await LoadSqlScriptAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        logger.LogInformation("Quartz database schema initialized successfully");
    }

    private static async Task<bool> QuartzSchemaExistsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = "SELECT to_regclass(@table_name) IS NOT NULL";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("table_name", $"public.{QuartzTableName}");

        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    private static async Task<string> LoadSqlScriptAsync(CancellationToken cancellationToken)
    {
        var assembly = typeof(QuartzDbInitializer).Assembly;
        await using var stream = assembly.GetManifestResourceStream(SqlResourceName)
            ?? throw new FileNotFoundException(
                $"Embedded Quartz SQL script '{SqlResourceName}' was not found");
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}
