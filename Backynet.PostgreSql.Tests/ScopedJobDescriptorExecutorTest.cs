using Backynet.Abstraction;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.PostgreSql.Tests;

[TestSubject(typeof(ScopedJobDescriptorExecutor))]
public class ScopedJobDescriptorExecutorTest
{
    [Fact]
    public async Task METHOD()
    {
        var services = new ServiceCollection();
        services.AddScoped<Example>();

        var serviceProvider = services.BuildServiceProvider();
        var jobDescriptorExecutor = new ScopedJobDescriptorExecutor(serviceProvider);

        var requiredService = serviceProvider.GetRequiredService<Example>();
        var jobDescriptor = JobDescriptorFactory.Create(() => requiredService.DoWork());

        await jobDescriptorExecutor.Execute(jobDescriptor);
    }

    public class Example
    {
        public void DoWork()
        {
        }
    }
}