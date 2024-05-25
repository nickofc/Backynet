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
        var serializer = new MessagePackSerializerProvider();

        _serverName = Guid.NewGuid().ToString();
        _repository = new PostgreSqlJobRepository(factory, serializer, SystemClock.Instance);
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

    [Fact]
    public async Task Should_Update_Validate_Row_Version()
    {
        // arrange

        var job1 = Job.Empty();
        job1.JobState = JobState.Enqueued;
        await _repository.Add(job1);

        // act

        var job2 = await _repository.Get(job1.Id);
        job2!.ServerName = "test-server-name";

        var job3 = await _repository.Get(job1.Id);
        job3!.ServerName = "funky-server-name";

        var update2 = await _repository.Update(job1.Id, job2);
        var update3 = await _repository.Update(job1.Id, job3);
        
        // assert
        
        Assert.True(update2);
        Assert.False(update3);
        
        var actualJob = await _repository.Get(job1.Id);
        
        actualJob!.RowVersion = 0; // ignore row_version comparison because it will be outdated
        job2.RowVersion = 0;
        
        Assert.Equivalent(job2, actualJob);
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