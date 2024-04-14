using Backynet.Core;
using Backynet.Postgresql;

namespace Backynet.Tests;

public class BackynetClientTests
{
    public BackynetClientTests()
    {
        WasExecuted.Reset();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Action_Job_When_Job_Was_Enqueued()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var serializer = new DefaultJsonSerializer();
        var repository = new PostgreSqlJobRepository(factory, serializer);

        var backynetServer = new BackynetWorker(repository, new JobDescriptorExecutor(), new BackynetWorkerOptions());
        await backynetServer.Start(CancellationToken.None);

        var backynetClient = new BackynetClient(repository);
        await backynetClient.EnqueueAsync(() => FakeSyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Func_Job_When_Job_Was_Enqueued()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var serializer = new DefaultJsonSerializer();
        var repository = new PostgreSqlJobRepository(factory, serializer);

        var backynetServer = new BackynetWorker(repository, new JobDescriptorExecutor(), new BackynetWorkerOptions());
        await backynetServer.Start(CancellationToken.None);

        var backynetClient = new BackynetClient(repository);
        await backynetClient.EnqueueAsync(() => FakeAsyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    private static readonly ManualResetEventSlim WasExecuted = new();

    private static Task FakeAsyncMethod()
    {
        WasExecuted.Set();
        return Task.CompletedTask;
    }

    private static void FakeSyncMethod()
    {
        WasExecuted.Set();
    }
}