using Backynet.Abstraction;
using Backynet.Tests;

namespace Backynet.PostgreSql.Tests;

public class PostgresRepositoryTests : IDisposable, IAsyncDisposable
{
    [Fact]
    public async Task Should_Acquire_Only_Not_Locked_From_Database_When_Acquire_Is_Called()
    {
        var serverName = Guid.NewGuid().ToString();
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var serializer = new DefaultJsonSerializer();
        var repository = new PostgreSqlJobRepository(factory, serializer,
            new PostgreSqlJobRepositoryOptions { MaxTimeWithoutHeartbeat = TimeSpan.FromSeconds(30) });
        var serverService = new ServerService(factory, new ServerServiceOptions { MaxTimeWithoutHeartbeat = TimeSpan.FromSeconds(30) });
        await serverService.Heartbeat(serverName);

        for (var i = 0; i < 10; i++)
        {
            var emptyJob = Job.Empty();
            emptyJob.JobState = JobState.Scheduled;
            await repository.Add(emptyJob);
        }

        var sum = 0;
        var tasks = new List<Task>();

        for (var i = 0; i < 50; i++)
        {
            var task = Task.Run(async () =>
            {
                var newJobs = await repository.Acquire(serverName, int.MaxValue);
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
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var serializer = new DefaultJsonSerializer();
        var repository = new PostgreSqlJobRepository(factory, serializer,
            new PostgreSqlJobRepositoryOptions() { MaxTimeWithoutHeartbeat = TimeSpan.FromSeconds(30) });

        //act 

        await repository.Add(expectedJob);

        // assert

        var actualJob = await repository.Get(expectedJob.Id);

        Assert.Equal(expectedJob.Id, actualJob.Id);
        Assert.Equal(expectedJob.JobState, actualJob.JobState);
        Assert.Equal(expectedJob.Descriptor.Method, actualJob.Descriptor.Method);
        Assert.Equal(expectedJob.Descriptor.Arguments, actualJob.Descriptor.Arguments);
    }

    [Fact]
    public async Task Should_Acquire_Job_When_GetForServer_Is_Called()
    {
        // arrange

        var serverName = Guid.NewGuid().ToString();
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var serializer = new DefaultJsonSerializer();
        var repository = new PostgreSqlJobRepository(factory, serializer,
            new PostgreSqlJobRepositoryOptions { MaxTimeWithoutHeartbeat = TimeSpan.FromSeconds(30) });

        for (var i = 0; i < 10; i++)
        {
            var emptyJob = Job.Empty();
            emptyJob.JobState = JobState.Scheduled;
            await repository.Add(emptyJob);
        }

        //act 

        var jobs = await repository.Acquire(serverName, 1);

        // assert

        Assert.True(jobs.Count > 0);
        Assert.True(jobs.All(x => x.ServerName == serverName));
    }

    public async ValueTask DisposeAsync()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        await using var connection = await factory.GetAsync();
        await DatabaseExtensions.DeleteAllJobs(connection);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }
}