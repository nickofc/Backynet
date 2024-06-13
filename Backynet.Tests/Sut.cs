using Backynet.Abstraction;
using Backynet.Options;
using Xunit.Abstractions;

namespace Backynet.Tests;

public class Sut : IDisposable, IAsyncDisposable
{
    public IBackynetServer BackynetServer { get; private set; }
    public IBackynetClient BackynetClient { get; private set; }

    public Sut(ITestOutputHelper? testOutputHelper = null)
    {
        var optionsBuilder = new BackynetContextOptionsBuilder()
            .UsePostgreSql(TestContext.ConnectionString);

        if (testOutputHelper != null)
        {
            optionsBuilder.UseLoggerFactory(new DebugLoggerFactory(testOutputHelper));
        }

        var backynetContext = new BackynetContext(optionsBuilder.Options);

        BackynetServer = backynetContext.Server;
        BackynetClient = backynetContext.Client;
    }

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _workerTask;

    public async Task Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _workerTask = BackynetServer.Start(_cancellationTokenSource.Token);

        await _workerTask;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await CastAndDispose(_cancellationTokenSource);
        await CastAndDispose(_workerTask);

        await BackynetServer.WaitForShutdown();

        return;

        static async ValueTask CastAndDispose(IDisposable? resource)
        {
            switch (resource)
            {
                case null:
                    return;
                case IAsyncDisposable resourceAsyncDisposable:
                    await resourceAsyncDisposable.DisposeAsync();
                    break;
                default:
                    resource.Dispose();
                    break;
            }
        }
    }
}