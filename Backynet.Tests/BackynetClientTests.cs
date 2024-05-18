using Backynet.Abstraction;
using Backynet.Options;
using Xunit.Abstractions;

namespace Backynet.Tests;

public class BackynetClientTests
{
    public class Sut : IDisposable
    {
        public IBackynetServer BackynetServer { get; private set; }
        public IBackynetClient BackynetClient { get; private set; }

        public Sut(ITestOutputHelper testOutputHelper)
        {
            var optionsBuilder = new BackynetContextOptionsBuilder()
                .UseMaxTimeWithoutHeartbeat(TimeSpan.FromSeconds(30))
                .UsePostgreSql(TestContext.ConnectionString)
                .UseLoggerFactory(new DebugLoggerFactory(testOutputHelper));

            var backynetContext = new BackynetContext(optionsBuilder.Options);

            BackynetServer = backynetContext.Server;
            BackynetClient = backynetContext.Client;
        }

        private CancellationTokenSource _cancellationTokenSource;
        private Task _workerTask;

        public async Task Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _workerTask = BackynetServer.Start(_cancellationTokenSource.Token);

            await _workerTask;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _workerTask.Wait();
        }
    }

    private readonly ITestOutputHelper _testOutputHelper;

    public BackynetClientTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        WasExecuted.Reset();
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