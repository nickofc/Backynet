using Backynet.Core;
using Backynet.Postgresql;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class BackynetServerService : IBackynetServerService
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;
    private readonly TimeSpan _hostTimeout;

    public BackynetServerService(NpgsqlConnectionFactory npgsqlConnectionFactory, TimeSpan hostTimeout)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
        _hostTimeout = hostTimeout;
    }

    public async Task Heartbeat(string serverName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = """
                              INSERT INTO servers (server_name, heartbeat_on, created_at)
                              VALUES (@server_name, @heartbeat_on, @created_at)
                              ON CONFLICT (server_name) DO UPDATE
                                  SET heartbeat_on = @heartbeat_on;
                              """;
        command.Parameters.Add(new NpgsqlParameter("server_name", serverName));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("heartbeat_on", DateTimeOffset.UtcNow));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("created_at", DateTimeOffset.UtcNow));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task Purge(CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = """
                              DELETE FROM servers WHERE heartbeat_on < @heartbeat_on;
                              """;
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("heartbeat_on", DateTimeOffset.UtcNow - _hostTimeout));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}