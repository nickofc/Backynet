using System.Threading.Channels;
using Npgsql;

namespace Backynet.PostgreSql;

public class Writer
{
    private readonly string _connectionString;

    public Writer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task Send(
        string channelName,
        string payload,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        {
            command.CommandText = $"NOTIFY {channelName}, '{payload}'";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}

public class Reader
{
    private readonly Channel<NpgsqlNotificationEventArgs> _channel;
    private readonly ManualResetEventSlim _onEventLoopTick;
    private readonly string _connectionString;

    public Reader(string connectionString)
    {
        _channel = Channel.CreateBounded<NpgsqlNotificationEventArgs>(10);
        _onEventLoopTick = new ManualResetEventSlim(false);
        _connectionString = connectionString;
    }

    public async Task Start(
        Dictionary<string, List<Func<string, string, CancellationToken, Task>>> callbackMap,
        CancellationToken cancellationToken)
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task? eventLoopTask = null;

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            connection.Notification += OnNotificationEventHandler;

            await connection.OpenAsync(tokenSource.Token);

            await using var command = connection.CreateCommand();
            {
                command.CommandText = string.Join(';', callbackMap.Keys.Select(channelName => $"LISTEN {channelName}"));
                await command.ExecuteNonQueryAsync(tokenSource.Token);
            }

            eventLoopTask = StartEventLoop(callbackMap, tokenSource.Token);

            while (true)
            {
                var eventWaitTask = connection.WaitAsync(tokenSource.Token);
                var completedTask = await Task.WhenAny(eventLoopTask, eventWaitTask);

                await completedTask;
            }
        }
        catch (Exception)
        {
            if (eventLoopTask != null)
            {
                await tokenSource.CancelAsync();

                try
                {
                    await eventLoopTask;
                }
                catch (Exception)
                {
                    // ignored
                }

                eventLoopTask.Dispose();
            }

            throw;
        }
    }

    private void OnNotificationEventHandler(
        object _,
        NpgsqlNotificationEventArgs eventArgs)
    {
        while (!_channel.Writer.TryWrite(eventArgs))
        {
            _onEventLoopTick.Wait();
        }
    }

    private async Task StartEventLoop(
        Dictionary<string, List<Func<string, string, CancellationToken, Task>>> callbackMap,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var @event = await _channel.Reader.ReadAsync(cancellationToken);

            if (callbackMap.TryGetValue(@event.Channel, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    await callback(@event.Channel, @event.Payload, cancellationToken);
                }
            }

            _onEventLoopTick.Set();
        }
    }
}