using System.Linq.Expressions;
using Backynet.Abstraction;
using Backynet.Core;

namespace Backynet.Tests;

public class JobDescriptorExecutorTest
{
    [Fact]
    public async Task Should_Execute_Invokable()
    {
        const int valueTypeArgument = 997;
        var referenceTypeArgument = new FakeDto { Username = "Antoni" };

        Expression<Func<Task>> expression = () => FakeAsyncMethod(valueTypeArgument, referenceTypeArgument);

        var jobDescriptor = JobDescriptorFactory.Create(expression);
        var jobRunner = new JobDescriptorExecutor();

        await jobRunner.Execute(jobDescriptor);

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