using Xunit.Abstractions;

namespace Backynet.Tests.BackynetContextTests;

// ReSharper disable once InconsistentNaming
[Collection(nameof(BackynetContext))]
public class BackynetContext_Cancel
{
    private const int DefaultTimeout = 30 * 1000;

    private readonly BackynetContextFactory _backynetContextFactory;

    public BackynetContext_Cancel(ITestOutputHelper testOutputHelper)
    {
        _backynetContextFactory = new BackynetContextFactory(testOutputHelper);
    }

    [Fact(Timeout = DefaultTimeout)]
    public async Task When_Job_Was_Cancelled_Should_Cancel_Internal_Cancellation_Token()
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

        var jobId = await backynetContext.Client.EnqueueAsync(() => Invocation.FakeInfinityMethodWithCancellationToken(default), CancellationToken.None);

        invokedEvent.Wait(CancellationToken.None);

        // act

        var isCancelled = await backynetContext.Client.CancelAsync(jobId, CancellationToken.None);

        // assert

        Assert.True(isCancelled);
        cancelledEvent.Wait(CancellationToken.None);
    }

    [Fact(Timeout = DefaultTimeout)]
    public async Task When_Job_Was_Not_Found_Should_Return_False()
    {
        // arrange

        using var serverCancellationTokenSource = new CancellationTokenSource();
        using var cancelledEvent = new ManualResetEventSlim();
        using var invokedEvent = new ManualResetEventSlim();

        var backynetContext = _backynetContextFactory.Create();
        await backynetContext.Server.Start(serverCancellationTokenSource.Token);

        var jobId = Guid.NewGuid();

        // act

        var isCancelled = await backynetContext.Client.CancelAsync(jobId, CancellationToken.None);

        // assert

        Assert.False(isCancelled);
    }
}