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
        WasStartedExecuting.Reset();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Action_Job_When_Job_Was_Enqueued()
    {
        await using var sut = new Sut(_testOutputHelper);
        await sut.Start();

        await sut.BackynetClient.EnqueueAsync(() => FakeSyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Func_Job_When_Job_Was_Enqueued()
    {
        await using var sut = new Sut(_testOutputHelper);
        await sut.Start();

        await sut.BackynetClient.EnqueueAsync(() => FakeAsyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Cancel()
    {
        await using var sut = new Sut(_testOutputHelper);
        await sut.Start();

        var jobId = await sut.BackynetClient.EnqueueAsync(() => FakeLongRunningAsyncMethod(default));

        await Task.Delay(3000);
        await sut.BackynetClient.CancelAsync(jobId);

        WasCanceled.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Process_Job_When_One_Backynet_Istance_Stopped_Working_While_Executing_Job()
    {
        var sut = new Sut(_testOutputHelper);
        await sut.Start();

        var jobId = await sut.BackynetClient.EnqueueAsync(() => FakeAsyncMethod());

        await sut.DisposeAsync();

        sut = new Sut(_testOutputHelper);
        await sut.Start();

        WasExecuted.Wait();
    }

    [Fact]
    public async Task Should_Shutdown()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        var sut = new Sut(_testOutputHelper);
        await sut.BackynetServer.Start(cts.Token);

        await sut.BackynetServer.WaitForShutdown();

        Assert.False(sut.BackynetServer.IsRunning);
    }

    [Fact]
    public async Task Should_Shutdown_When_Job_Is_Executing()
    {
        var cts = new CancellationTokenSource();

        var sut = new Sut(_testOutputHelper);
        await sut.BackynetServer.Start(cts.Token);
        await sut.BackynetClient.EnqueueAsync(() => FakeNotEnding());

        WasStartedExecuting.Wait();

        cts.Cancel();
        await sut.BackynetServer.WaitForShutdown();

        Assert.False(sut.BackynetServer.IsRunning);
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

    private static void FakeAsyncLong()
    {
        Thread.Sleep(20000);
        WasExecuted.Set();
    }

    private static async Task FakeNotEnding()
    {
        WasStartedExecuting.Set();

        await Task.Delay(Timeout.Infinite, CancellationToken.None).ConfigureAwait(false);
    }

    private static ManualResetEventSlim WasStartedExecuting = new();
}

public class BackynetClient_Start
{
    [Fact]
    public void Do()
    {
    }
}

public static class Helper
{
    public static Action? OnFakeMethodExecuting { get; private set; }

    public static async Task FakeMethod()
    {
        OnFakeMethodExecuting?.Invoke();
        await Task.CompletedTask;
    }

    public static Action? OnFakeMethodWithCancellationToken { get; private set; }

    public static async Task FakeMethodWithCancellationToken(CancellationToken cancellationToken)
    {
        OnFakeMethodWithCancellationToken?.Invoke();
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}