using System.Reflection;

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
        await Execute(scripts, cancellationToken);
    }

    public IEnumerable<string> GetAllMigrationScripts()
    {
        // 1_Name_Migration.sql

        var scripts = EmbeddedResource
            .Find(x => x.EndsWith("_Migration.sql", StringComparison.InvariantCulture), MigrationAssembly)
            .ToArray();

        var entries = new List<Entry>(scripts.Length);

        foreach (var script in scripts)
        {
            var byDot = script.Split('.');
            var byName = byDot[^2].Split('_');

            entries.Add(new Entry(int.Parse(byName[0]), script));
        }

        var output = entries
            .OrderBy(x => x.Order)
            .Select(x => x.ResourceName)
            .ToArray();

        return output;
    }

    private readonly struct Entry
    {
        public int Order { get; }
        public string ResourceName { get; }

        public Entry(int order, string resourceName)
        {
            Order = order;
            ResourceName = resourceName;
        }
    }

    private async Task Execute(IEnumerable<string> scripts, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.GetAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var script in scripts)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = script;
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}