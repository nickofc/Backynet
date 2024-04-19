using Backynet.Core;
using Backynet.Core.Abstraction;
using Backynet.Postgresql;
using Backynet.PostgreSql;

namespace Backynet.Tests;

public class BackynetClientTests
{
    public class Sut : IDisposable
    {
        public IBackynetWorker BackynetWorker { get; private set; }
        public IBackynetClient BackynetClient { get; private set; }

        public Sut()
        {
            var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
            var serializer = new DefaultJsonSerializer();
            var repository = new PostgreSqlJobRepository(factory, serializer);
            var controllerService = new ControllerService(factory, TimeSpan.FromSeconds(20));
            var jobDescriptorExecutor = new JobDescriptorExecutor();
            var jobExecutor = new JobExecutor(jobDescriptorExecutor, repository);
            var options = new BackynetWorkerOptions();

            BackynetWorker = new BackynetWorker(repository, jobExecutor, options, controllerService);
            BackynetClient = new BackynetClient(repository);
        }

        private CancellationTokenSource _cancellationTokenSource;
        private Task _workerTask;

        public async Task Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _workerTask = BackynetWorker.Start(_cancellationTokenSource.Token);

            await _workerTask;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _workerTask.Wait();
        }
    }

    public BackynetClientTests()
    {
        WasExecuted.Reset();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Action_Job_When_Job_Was_Enqueued()
    {
        using var sut = new Sut();
        await sut.Start();

        await sut.BackynetClient.EnqueueAsync(() => FakeSyncMethod(), CancellationToken.None);

        WasExecuted.Wait();
    }

    [Fact(Timeout = 60 * 1000)]
    public async Task Should_Execute_Func_Job_When_Job_Was_Enqueued()
    {
        using var sut = new Sut();
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