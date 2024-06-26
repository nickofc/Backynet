using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class ServerService : IServerService
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;
    private readonly IServerServiceOptions _serverServiceOptions;

    public ServerService(NpgsqlConnectionFactory npgsqlConnectionFactory, IServerServiceOptions serverServiceOptions)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
        _serverServiceOptions = serverServiceOptions;
    }

    public async Task Heartbeat(CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              INSERT INTO servers (instance_id, server_name, heartbeat_on, created_at)
                              VALUES (@instance_id, @server_name, @heartbeat_on, @created_at)
                              ON CONFLICT (instance_id) DO UPDATE
                                  SET heartbeat_on = @heartbeat_on;
                              """;
        command.Parameters.Add(new NpgsqlParameter<Guid>("instance_id", _serverServiceOptions.InstanceId));
        command.Parameters.Add(new NpgsqlParameter<string>("server_name", _serverServiceOptions.ServerName));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("heartbeat_on", DateTimeOffset.UtcNow));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("created_at", DateTimeOffset.UtcNow));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task Purge(CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              DELETE FROM servers WHERE heartbeat_on < @heartbeat_on;
                              """;
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("heartbeat_on", DateTimeOffset.UtcNow - _serverServiceOptions.MaxTimeWithoutHeartbeat));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}