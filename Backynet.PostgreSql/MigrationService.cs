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
        var scripts = FindAllMigrationScripts();
        await Execute(scripts, cancellationToken);
    }

    public IEnumerable<string> FindAllMigrationScripts()
    {
        // 00000000_Name_Migration.sql

        var scripts = EmbeddedResource
            .Find(x => x.EndsWith("_Migration.sql", StringComparison.InvariantCulture), MigrationAssembly)
            .ToArray();
        
        // todo: sort by date/id
        
        return scripts;
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