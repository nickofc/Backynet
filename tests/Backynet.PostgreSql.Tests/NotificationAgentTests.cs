using System.Collections.Concurrent;

namespace Backynet.PostgreSql.Tests;

public class NotificationAgentTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public NotificationAgentTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact(Timeout = 30000)]
    public async Task Should_Receive_Payload()
    {
        const string expectedChannel = "job_notify";
        var runtime = TimeSpan.FromSeconds(15);

        var agentStartTask = new TaskCompletionSource();
        var concurrentBag = new ConcurrentBag<(NotificationAgent.CallbackEventArgs, CancellationToken)>();
        using var cancellationTokenSource = new CancellationTokenSource(runtime);
        var npgsqlConnectionFactory = new NpgsqlConnectionFactory(_databaseFixture.ConnectionString);
        var agent = new NotificationAgent(npgsqlConnectionFactory)
        {
            OnListenStarted = () =>
            {
                agentStartTask.SetResult();
                return Task.CompletedTask;
            },
            OnCallbackReceived = (arg1, arg2) =>
            {
                concurrentBag.Add((arg1, arg2));
                return Task.CompletedTask;
            }
        };

        var agentTask = agent.Listen([expectedChannel], cancellationTokenSource.Token);
        await await Task.WhenAny(agentTask, agentStartTask.Task);

        await agent.Publish(expectedChannel, cancellationToken: default);
        await Task.WhenAny(Task.Delay(Timeout.Infinite, cancellationTokenSource.Token));
        
        Assert.Single(concurrentBag);

        var ((channel, payload), cancellationToken) = concurrentBag.First();

        Assert.Equal(expectedChannel, channel);
        Assert.Empty(payload);
        Assert.Equal(cancellationTokenSource.Token, cancellationToken);
    }
}