using Backynet.Abstraction;
using Backynet.Tests;

namespace Backynet.PostgreSql.Tests;

public class PostgresRepositoryTests : IDisposable, IAsyncDisposable
{
    private readonly string _serverName;
    private readonly PostgreSqlJobRepository _repository;
    private readonly Task _serverServiceTask;

    public PostgresRepositoryTests()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var serverService = new ServerService(factory, new ServerServiceOptions { MaxTimeWithoutHeartbeat = TimeSpan.FromSeconds(30) });

        _serverName = Guid.NewGuid().ToString();
        _repository = new PostgreSqlJobRepository(factory, DefaultJsonSerializer.Instance, SystemClock.Instance);
        _serverServiceTask = serverService.Heartbeat(_serverName);
    }

    [Fact]
    public async Task Should_Acquire_Only_Not_Locked_From_Database_When_Acquire_Is_Called()
    {
        for (var i = 0; i < 10; i++)
        {
            var emptyJob = Job.Empty();
            emptyJob.JobState = JobState.Scheduled;
            await _repository.Add(emptyJob);
        }

        var sum = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 20; i++)
        {
            var task = Task.Run(async () =>
            {
                var newJobs = await _repository.Acquire(_serverName, int.MaxValue);
                Interlocked.Add(ref sum, newJobs.Count);
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        Assert.Equal(10, sum);
    }

    [Fact]
    public async Task Should_Insert_Job_To_Database_When_Add_Is_Called()
    {
        // arrange

        var expectedJob = Job.Empty();

        //act 

        await _repository.Add(expectedJob);

        // assert

        var actualJob = await _repository.Get(expectedJob.Id);

        Assert.Equal(expectedJob.Id, actualJob.Id);
        Assert.Equal(expectedJob.JobState, actualJob.JobState);
        Assert.Equal(expectedJob.Descriptor.Method, actualJob.Descriptor.Method);
        Assert.Equal(expectedJob.Descriptor.Arguments, actualJob.Descriptor.Arguments);
    }

    [Fact]
    public async Task Should_Acquire_Job_When_GetForServer_Is_Called()
    {
        // arrange

        for (var i = 0; i < 10; i++)
        {
            var emptyJob = Job.Empty();
            emptyJob.JobState = JobState.Scheduled;
            await _repository.Add(emptyJob);
        }

        //act 

        var jobs = await _repository.Acquire(_serverName, 1);

        // assert

        Assert.True(jobs.Count > 0);
        Assert.True(jobs.All(x => x.ServerName == _serverName));
    }

    public async ValueTask DisposeAsync()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        await using var connection = await factory.GetAsync();
        await DatabaseExtensions.DeleteAllJobs(connection);
        _serverServiceTask.Dispose();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }
}