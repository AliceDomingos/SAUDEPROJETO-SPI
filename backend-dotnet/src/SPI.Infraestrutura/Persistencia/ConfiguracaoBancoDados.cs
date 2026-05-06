using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SPI.Infrastructure.Data.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = DatabaseProviderNames.Sqlite;
}

public static class DatabaseProviderNames
{
    public const string Sqlite = "Sqlite";
    public const string SqlServer = "SqlServer";
}

public static class DatabaseConfigurationExtensions
{
    public static DbContextOptionsBuilder UseConfiguredDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        IConfiguration configuration)
    {
        var provider = GetConfiguredProvider(configuration);

        return provider switch
        {
            DatabaseProviderNames.SqlServer => optionsBuilder.UseSqlServer(
                GetRequiredConnectionString(configuration, DatabaseProviderNames.SqlServer)),
            DatabaseProviderNames.Sqlite => optionsBuilder.UseSqlite(
                EnsureSqliteDirectoryExists(GetRequiredConnectionString(configuration, DatabaseProviderNames.Sqlite))),
            _ => throw new InvalidOperationException(
                $"Provider de banco '{provider}' nao suportado. Use '{DatabaseProviderNames.Sqlite}' ou '{DatabaseProviderNames.SqlServer}'.")
        };
    }

    public static string GetConfiguredProvider(IConfiguration configuration)
    {
        var configuredProvider = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>()?
            .Provider;

        if (string.IsNullOrWhiteSpace(configuredProvider))
        {
            return DatabaseProviderNames.Sqlite;
        }

        if (string.Equals(configuredProvider, DatabaseProviderNames.SqlServer, StringComparison.OrdinalIgnoreCase))
        {
            return DatabaseProviderNames.SqlServer;
        }

        if (string.Equals(configuredProvider, DatabaseProviderNames.Sqlite, StringComparison.OrdinalIgnoreCase))
        {
            return DatabaseProviderNames.Sqlite;
        }

        return configuredProvider.Trim();
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException($"Connection string '{name}' nao configurada.");
    }

    private static string EnsureSqliteDirectoryExists(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) || builder.DataSource == ":memory:")
        {
            return connectionString;
        }

        var fullPath = Path.GetFullPath(builder.DataSource);
        var directoryPath = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return connectionString;
    }
}



