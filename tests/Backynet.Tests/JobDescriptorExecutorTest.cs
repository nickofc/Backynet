using System.Linq.Expressions;
using Backynet.Abstraction;

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

    [Fact]
    public async Task Should_Pass_Cancellation_Token()
    {
        //

        var cts = new CancellationTokenSource();

        var expectedCancellationToken = cts.Token;
        CancellationToken actualCancellationToken = default;

        //

        Expression<Func<Task>> expression = () => FakeAsyncMethodWithCancellationToken(default, null, default);
        var jobDescriptor = JobDescriptorFactory.Create(expression);
        FakeAsyncMethodWithCancellationToken_Invoked = (_, _, arg3) => { actualCancellationToken = arg3; };

        var jobRunner = new JobDescriptorExecutor();
        await jobRunner.Execute(jobDescriptor, expectedCancellationToken);

        //

        Assert.Equal(expectedCancellationToken, actualCancellationToken);
    }

    private static Action<int, FakeDto, CancellationToken> FakeAsyncMethodWithCancellationToken_Invoked;

    private static Task FakeAsyncMethodWithCancellationToken(int fakeArg1, FakeDto fakeArg2, CancellationToken cancellationToken)
    {
        FakeAsyncMethodWithCancellationToken_Invoked.Invoke(fakeArg1, fakeArg2, cancellationToken);
        return Task.CompletedTask;
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