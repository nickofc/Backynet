using Backynet.Abstraction;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Backynet.Tests;

public class JobExecutorTests
{
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly IJobRepository _jobRepository;
    private readonly IJobExecutor _jobExecutor;

    public JobExecutorTests()
    {
        _jobDescriptorExecutor = Substitute.For<IJobDescriptorExecutor>();
        _jobRepository = Substitute.For<IJobRepository>();
        _jobExecutor = new JobExecutor(_jobDescriptorExecutor, _jobRepository,
            SystemClock.Instance, NullLogger<JobExecutor>.Instance);
    }

    [Theory]
    [InlineData(JobState.Unknown)]
    [InlineData(JobState.Created)]
    [InlineData(JobState.Enqueued)]
    [InlineData(JobState.Scheduled)]
    [InlineData(JobState.Processing)]
    [InlineData(JobState.Failed)]
    [InlineData(JobState.Succeeded)]
    [InlineData(JobState.Canceled)]
    [InlineData(JobState.Deleted)]
    public async Task Should_Throw_Invalid_Operation_Exception_When_Trying_Execute_Job_In_Invalid_State(JobState jobState)
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethod());
        var job = JobFactory.Create(jobDescriptor);

        job.JobState = jobState;
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _jobExecutor.Execute(job);
        });
    }
    
    [Fact]
    public async Task Should_Cancel()
    {
        _jobRepository.Update(Arg.Any<Guid>(), Arg.Any<Job>()).Returns(true);
        
        var cancellationTokenSource = new CancellationTokenSource(0);

        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethod());
        var job = JobFactory.Create(jobDescriptor);
        
        await _jobExecutor.Execute(job, cancellationTokenSource.Token);
    }

    public static void FakeMethod()
    {
    }
}