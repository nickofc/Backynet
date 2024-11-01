using Xunit.Abstractions;

namespace Backynet.Tests.BackynetContextTests;

// ReSharper disable once InconsistentNaming
[Collection(nameof(BackynetContext))]
public class BackynetContext_Stop
{
    private const int DefaultTimeout = 30 * 1000;

    private readonly BackynetContextFactory _backynetContextFactory;

    public BackynetContext_Stop(ITestOutputHelper testOutputHelper)
    {
        _backynetContextFactory = new BackynetContextFactory(testOutputHelper);
    }

    [Fact(Timeout = DefaultTimeout)]
    public async Task When_Server_Cancellation_Token_Was_Cancelled_Should_Stop_Server()
    {
        // arrange

        using var serverCancellationTokenSource = new CancellationTokenSource();
        var backynetContext = _backynetContextFactory.Create();

        Assert.False(backynetContext.Server.IsRunning);

        await backynetContext.Server.Start(serverCancellationTokenSource.Token);

        Assert.True(backynetContext.Server.IsRunning);

        // act

        await serverCancellationTokenSource.CancelAsync();
        await backynetContext.Server.WaitForShutdown(CancellationToken.None);

        // assert

        Assert.False(backynetContext.Server.IsRunning);
    }

    [Fact(Timeout = DefaultTimeout)]
    public async Task When_Server_Cancellation_Token_Was_Cancelled_Should_Cancel_Internal_Cancellation_Token()
    {
        // arrange

        using var serverCancellationTokenSource = new CancellationTokenSource();
        using var cancelledEvent = new ManualResetEventSlim();
        using var invokedEvent = new ManualResetEventSlim();

        var backynetContext = _backynetContextFactory.Create();
        await backynetContext.Server.Start(serverCancellationTokenSource.Token);

        // ReSharper disable once AccessToDisposedClosure
        Invocation.OnFakeInfinityMethodWithCancellationTokenInvocation = () => { invokedEvent.Set(); };
        // ReSharper disable once AccessToDisposedClosure
        Invocation.OnFakeInfinityMethodWithCancellationTokenCancelledInvocation = _ => { cancelledEvent.Set(); };

        await backynetContext.Client.EnqueueAsync(() => Invocation.FakeInfinityMethodWithCancellationToken(default), CancellationToken.None);

        invokedEvent.Wait(CancellationToken.None);

        // act

        await serverCancellationTokenSource.CancelAsync();
        await backynetContext.Server.WaitForShutdown(CancellationToken.None);

        // assert

        cancelledEvent.Wait(CancellationToken.None);
    }
}