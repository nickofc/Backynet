using Backynet.Abstraction;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NuGet.Frameworks;

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
    [InlineData(JobState.Unknown, true)]
    [InlineData(JobState.Created, false)]
    [InlineData(JobState.Enqueued, false)]
    [InlineData(JobState.Scheduled, false)]
    [InlineData(JobState.Processing, true)]
    [InlineData(JobState.Failed, true)]
    [InlineData(JobState.Succeeded, true)]
    [InlineData(JobState.Canceled, true)]
    [InlineData(JobState.Deleted, true)]
    public async Task Should_Throw_Invalid_Operation_Exception_When_Trying_Execute_Job_In_Invalid_State(JobState jobState, bool shouldThrow)
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethod());
        var job = JobFactory.Create(jobDescriptor);

        job.JobState = jobState;

        if (shouldThrow)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => { await _jobExecutor.Execute(job); });
        }
        else
        {
            await _jobExecutor.Execute(job);
        }
    }

    [Fact]
    public async Task Should_Execute_Job_When_JobState_Is_Created_And_Date_Is_From_Past()
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethod());
        var job = JobFactory.Create(jobDescriptor);
        var now = DateTimeOffset.UtcNow;

        job.JobState = JobState.Created;
        job.NextOccurrenceAt = now;

        await _jobExecutor.Execute(job);

        Assert.Equal(JobState.Succeeded, job.JobState);
    }

    [Fact]
    public async Task Should_Schedule_Job_When_JobState_Is_Created_And_Date_Is_From_Future()
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethod());
        var job = JobFactory.Create(jobDescriptor);
        var now = DateTimeOffset.UtcNow;

        job.JobState = JobState.Created;
        job.NextOccurrenceAt = now.AddDays(1);

        await _jobExecutor.Execute(job);

        Assert.Equal(JobState.Scheduled, job.JobState);
    }

    [Fact]
    public async Task Should_Change_State_To_Succeeded_When_Job_State_Is_Created_And_Next_Occurrence_At_Is_Null()
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethod());
        var job = JobFactory.Create(jobDescriptor);

        job.JobState = JobState.Created;
        job.NextOccurrenceAt = null;

        await _jobExecutor.Execute(job);

        Assert.Equal(JobState.Succeeded, job.JobState);
    }

    [Fact]
    public async Task Should_Change_State_To_Failed_When_Job_Execution_Throws_Exception()
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => FakeMethodThatThrowsException());
        var job = JobFactory.Create(jobDescriptor);

        _jobDescriptorExecutor
            .Execute(Arg.Any<IJobDescriptor>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<JobDescriptorExecutorException>();

        job.JobState = JobState.Created;
        job.NextOccurrenceAt = null;

        await _jobExecutor.Execute(job);

        Assert.Equal(JobState.Failed, job.JobState);
        Assert.Single(job.Errors);
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

    public static void FakeMethodThatThrowsException()
    {
        throw new Exception(nameof(FakeMethodThatThrowsException));
    }
}