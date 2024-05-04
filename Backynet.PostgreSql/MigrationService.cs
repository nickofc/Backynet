namespace Backynet.PostgreSql;

internal sealed class MigrationService
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    public MigrationService(NpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task Perform(IEnumerable<string> scripts, CancellationToken cancellationToken)
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