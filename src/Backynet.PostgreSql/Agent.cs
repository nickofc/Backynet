using System.Collections.Concurrent;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class Agent
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;

    public Agent(NpgsqlConnectionFactory npgsqlConnectionFactory)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
    }

    public Func<CallbackEventArgs, CancellationToken, Task> OnCallbackReceived { get; set; } = (_, _) => Task.CompletedTask;
    public Func<Task> OnListenStarted { get; set; } = () => Task.CompletedTask;

    public async Task Listen(string[] channels, CancellationToken cancellationToken)
    {
        var events = new ConcurrentQueue<CallbackEventArgs>();
        await using var connection = _npgsqlConnectionFactory.Get(Configure);

        connection.Notification += ConnectionOnNotification;
        await connection.OpenAsync(cancellationToken);

        foreach (var channel in channels)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"LISTEN {channel}";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await OnListenStarted();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await connection.WaitAsync(cancellationToken);

            while (events.TryDequeue(out var eventArgs))
            {
                await OnCallbackReceived(eventArgs, cancellationToken);
            }
        }

        void Configure(NpgsqlConnectionStringBuilder connectionStringBuilder)
        {
            connectionStringBuilder.KeepAlive = 30; // todo: load from options
        }

        void ConnectionOnNotification(object _, NpgsqlNotificationEventArgs eventArgs)
        {
            events.Enqueue(new CallbackEventArgs(eventArgs.Channel, eventArgs.Payload));
        }

        // ReSharper disable once FunctionNeverReturns
    }

    public async Task Publish(string channel, CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();

        await using var command = connection.CreateCommand();
        command.CommandText = $"NOTIFY {channel}";

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public record CallbackEventArgs(string Channel, string Payload);
}