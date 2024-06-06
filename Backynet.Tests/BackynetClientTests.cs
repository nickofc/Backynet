using Xunit.Abstractions;

namespace Backynet.Tests;

public class BackynetClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BackynetClientTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        WasExecuted.Reset();
        WasCanceled.Reset();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Action_Job_When_Job_Was_Enqueued()
    {
        using var sut = new Sut(_testOutputHelper);
        await sut.Start();

        await sut.BackynetClient.EnqueueAsync(() => FakeSyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Func_Job_When_Job_Was_Enqueued()
    {
        using var sut = new Sut(_testOutputHelper);
        await sut.Start();

        await sut.BackynetClient.EnqueueAsync(() => FakeAsyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Cancel()
    {
        using var sut = new Sut(_testOutputHelper);
        await sut.Start();

        var jobId = await sut.BackynetClient.EnqueueAsync(() => FakeLongRunningAsyncMethod(default));
        await sut.BackynetClient.CancelAsync(jobId);

        WasCanceled.Wait();
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

    private static readonly ManualResetEventSlim WasCanceled = new();

    private static async Task FakeLongRunningAsyncMethod(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(-1, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            WasCanceled.Set();
        }
    }
}