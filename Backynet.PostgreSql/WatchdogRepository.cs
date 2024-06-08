using Backynet.Abstraction;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class WatchdogRepository : IWatchdogRepository
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;

    public WatchdogRepository(NpgsqlConnectionFactory npgsqlConnectionFactory)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
    }

    public async Task<Guid[]> Get(Guid[] jobIds, string serverName, CancellationToken cancellationToken)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT id FROM jobs WHERE id = ANY (@job_ids) AND state = @job_state
                              """;
        command.Parameters.Add(new NpgsqlParameter<Guid[]>("job_ids", jobIds));
        command.Parameters.Add(new NpgsqlParameter<int>("job_state", CastTo<int>.From(JobState.Canceled)));

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<Guid>(jobIds.Length);

        while (await reader.ReadAsync(cancellationToken))
        {
            var jobId = reader.GetFieldValue<Guid>(0);

            rows.Add(jobId);
        }

        return rows.ToArray(); //todo: replace with array
    }
}