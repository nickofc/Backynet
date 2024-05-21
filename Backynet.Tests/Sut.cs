using Backynet.Abstraction;
using Backynet.Options;
using Xunit.Abstractions;

namespace Backynet.Tests;

public class Sut : IDisposable
{
    public IBackynetServer BackynetServer { get; private set; }
    public IBackynetClient BackynetClient { get; private set; }

    public Sut(ITestOutputHelper? testOutputHelper = null)
    {
        var optionsBuilder = new BackynetContextOptionsBuilder()
            .UseMaxTimeWithoutHeartbeat(TimeSpan.FromSeconds(30))
            .UsePostgreSql(TestContext.ConnectionString);

        if (testOutputHelper != null)
        {
            optionsBuilder.UseLoggerFactory(new DebugLoggerFactory(testOutputHelper));
        }

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
        GC.SuppressFinalize(this);
    }
}