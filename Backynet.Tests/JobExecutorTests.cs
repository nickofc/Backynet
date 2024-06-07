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