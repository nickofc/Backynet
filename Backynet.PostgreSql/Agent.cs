using Npgsql;

namespace Backynet.PostgreSql;

internal class Agent
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;

    public Agent(NpgsqlConnectionFactory npgsqlConnectionFactory)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
    }

    public async Task Listen(string[] channels, Action<CallbackEventArgs> callback, CancellationToken cancellationToken)
    {
        await using var connection = _npgsqlConnectionFactory.Get(Configure);

        connection.Notification += ConnectionOnNotification;
        await connection.OpenAsync(cancellationToken);

        foreach (var channel in channels)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"LISTEN {channel};";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await connection.WaitAsync(cancellationToken);
        }

        void Configure(NpgsqlConnectionStringBuilder connectionStringBuilder)
        {
            connectionStringBuilder.KeepAlive = 30;
        }

        void ConnectionOnNotification(object _, NpgsqlNotificationEventArgs eventArgs)
        {
            callback(new CallbackEventArgs(eventArgs.Channel, eventArgs.Payload));
        }

        // ReSharper disable once FunctionNeverReturns
    }

    public class CallbackEventArgs(string Channel, string Payload);
}