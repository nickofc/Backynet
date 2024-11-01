using Xunit.Abstractions;

namespace Backynet.Tests.BackynetContextTests;

// ReSharper disable once InconsistentNaming
[Collection(nameof(BackynetContext))]
public class BackynetContext_Enqueue
{
    private const int DefaultTimeout = 30 * 1000;

    private readonly BackynetContextFactory _backynetContextFactory;

    public BackynetContext_Enqueue(ITestOutputHelper testOutputHelper)
    {
        _backynetContextFactory = new BackynetContextFactory(testOutputHelper);
    }

    [Fact(Timeout = DefaultTimeout)]
    public async Task When_Job_Was_Enqueued_Should_Execute()
    {
        // arrange

        using var cancellationTokenSource = new CancellationTokenSource();
        using var invokedEvent = new ManualResetEventSlim();

        var backynetContext = _backynetContextFactory.Create();
        await backynetContext.Server.Start(cancellationTokenSource.Token);

        // ReSharper disable once AccessToDisposedClosure
        Invocation.OnFakeMethodInvocation = () => { invokedEvent.Set(); };

        // act

        await backynetContext.Client.EnqueueAsync(() => Invocation.FakeMethod(), CancellationToken.None);

        // assert

        invokedEvent.Wait(CancellationToken.None);
    }

    [Fact(Timeout = DefaultTimeout)]
    public async Task When_Async_Job_Was_Enqueued_Should_Execute()
    {
        // arrange

        using var cancellationTokenSource = new CancellationTokenSource();
        using var invokedEvent = new ManualResetEventSlim();

        var backynetContext = _backynetContextFactory.Create();
        await backynetContext.Server.Start(cancellationTokenSource.Token);

        // ReSharper disable once AccessToDisposedClosure
        Invocation.OnFakeAsyncMethodInvocation = () => { invokedEvent.Set(); };

        // act

        await backynetContext.Client.EnqueueAsync(() => Invocation.FakeAsyncMethod(), CancellationToken.None);

        // assert

        invokedEvent.Wait(CancellationToken.None);
    }
}