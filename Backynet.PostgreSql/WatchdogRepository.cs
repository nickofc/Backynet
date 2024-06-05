using Npgsql;

namespace Backynet.PostgreSql;

public class WatchdogRepository : IWatchdogRepository
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;
    

    public async Task<Guid[]> Get(Guid[] jobIds, string serverName, CancellationToken cancellationToken)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.Connection = connection;
        command.CommandText = """
                              select * from jobs j
                              where j.id in () and 
                              """;
        // command.Parameters.Add(new NpgsqlParameter("group_name", group.GroupName));
        // command.Parameters.Add(new NpgsqlParameter<int>("max_concurrent_threads", group.MaxConcurrentThreads));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return null;
    }
}