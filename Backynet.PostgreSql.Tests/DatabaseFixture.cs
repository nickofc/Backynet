using Backynet.Tests;
using Npgsql;

namespace Backynet.PostgreSql.Tests;

public sealed class DatabaseFixture : IDisposable
{
    public string SchemaName { get; }
    public string ConnectionString { get; }

    public DatabaseFixture()
    {
        var schemaName = $"{Random.Shared.Next(1000, 9999)}";

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestContext.ConnectionString)
        {
            SearchPath = schemaName
        };

        var connectionString = connectionStringBuilder.ToString();

        ConnectionString = connectionString;
        SchemaName = schemaName;

        Initialize();
    }

    private void Initialize()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{SchemaName}\";";
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP SCHEMA \"{SchemaName}\" CASCADE;";
        connection.Open();
        command.ExecuteNonQuery();
    }
}