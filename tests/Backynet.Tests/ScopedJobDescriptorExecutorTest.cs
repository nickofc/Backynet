using Backynet.Abstraction;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.Tests;

[TestSubject(typeof(ScopedJobDescriptorExecutor))]
public class ScopedJobDescriptorExecutorTest
{
    private static readonly ManualResetEventSlim ExecutedEvent = new();

    [Fact(Timeout = 1000)]
    public async Task Should_Execute_Job_From_ServiceProvider()
    {
        var serviceProvider = new ServiceCollection()
            .AddScoped<Worker>()
            .BuildServiceProvider();

        var worker = serviceProvider.GetRequiredService<Worker>();
        var jobDescriptor = JobDescriptorFactory.Create(() => worker.Execute());

        var jobDescriptorExecutor = new ScopedJobDescriptorExecutor(serviceProvider);
        await jobDescriptorExecutor.Execute(jobDescriptor);

        ExecutedEvent.Wait();
    }

    private class Worker
    {
        public void Execute()
        {
            ExecutedEvent.Set();
        }
    }
}