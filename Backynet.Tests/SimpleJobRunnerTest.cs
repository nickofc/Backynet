using System.Linq.Expressions;
using Backynet.Core;
using Backynet.Core.Abstraction;

namespace Backynet.Tests;

public class SimpleJobRunnerTest
{
    [Fact]
    public async Task Should_Execute_Invokable()
    {
        const int valueTypeArgument = 997;
        var referenceTypeArgument = new FakeDto { Username = "Antoni" };

        Expression<Func<Task>> expression = () => FakeAsyncMethod(valueTypeArgument, referenceTypeArgument);

        var jobDescriptor = JobDescriptorFactory.Create(expression);
        var job = JobFactory.Create(jobDescriptor);

        var jobRunner = new SimpleJobRunner();
        await jobRunner.Run(job);

        Assert.True(WasExecuted.IsSet);
    }

    private static readonly ManualResetEventSlim WasExecuted = new();

    private static Task FakeAsyncMethod(int fakeArg1, FakeDto fakeArg2)
    {
        WasExecuted.Set();
        return Task.CompletedTask;
    }

    private record FakeDto
    {
        public string Username { get; set; }
    }
}