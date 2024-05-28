using System.Reflection;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class MigrationService
{
    private static readonly Assembly MigrationAssembly = typeof(MigrationService).Assembly;

    private readonly NpgsqlConnectionFactory _connectionFactory;

    public MigrationService(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task Perform(CancellationToken cancellationToken)
    {
        var scripts = GetAllMigrationScripts();
        var migrations = await GetAppliedMigrations();

        var toApply = scripts
            .Where(x => migrations.Contains(x.MigrationName) is false)
            .OrderBy(x => x.Order)
            .Select(x => x.ResourceName)
            .ToArray();

        await Execute(toApply, cancellationToken);
    }

    private async Task<IEnumerable<string>> GetAppliedMigrations()
    {
        try
        {
            var output = new List<string>();

            await using var conn = await _connectionFactory.GetAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM migrations";

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                var name = reader.GetString(0);

                output.Add(name);
            }

            return output;
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return new List<string>();
        }
    }

    private IEnumerable<Entry> GetAllMigrationScripts()
    {
        // 1_Name_Migration.sql

        var resourceNames = EmbeddedResource
            .Find(x => x.EndsWith("_Migration.sql", StringComparison.InvariantCulture), MigrationAssembly)
            .ToArray();

        var entries = new List<Entry>(resourceNames.Length);

        foreach (var resourceName in resourceNames)
        {
            var resourceNameParts = resourceName.Split('.');
            var migrationNameParts = resourceNameParts[^2].Split('_');

            var order = int.Parse(migrationNameParts[0]);
            var migrationName = $"{resourceNameParts[^2]}.{resourceNameParts[^1]}";

            entries.Add(new Entry(order, resourceName, migrationName));
        }

        return entries;
    }

    private readonly struct Entry
    {
        public int Order { get; }
        public string ResourceName { get; }
        public string MigrationName { get; }

        public Entry(int order, string resourceName, string migrationName)
        {
            Order = order;
            ResourceName = resourceName;
            MigrationName = migrationName;
        }
    }

    private async Task Execute(IEnumerable<string> resourceNames, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.GetAsync(cancellationToken);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var resourceName in resourceNames)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = EmbeddedResource.Read(resourceName);
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}