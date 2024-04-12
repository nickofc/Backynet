using Backynet.Core;
using Backynet.Postgresql;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class ControllerService : IControllerService
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;

    public ControllerService(NpgsqlConnectionFactory npgsqlConnectionFactory)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
    }

    public async Task Heartbeat(string serverName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = """
                              INSERT INTO clients (server_name, heartbeat_on)
                              VALUES (@server_name, @heartbeat_on)
                              ON CONFLICT(server_name) DO UPDATE
                                  SET heartbeat_on = @heartbeat_on;
                              """;
        command.Parameters.Add(new NpgsqlParameter("id", serverName));
        command.Parameters.Add(new NpgsqlParameter("heartbeat_on", DateTimeOffset.UtcNow));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}