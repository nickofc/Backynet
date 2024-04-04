using System.Linq.Expressions;
using Backynet.Core.Abstraction;

namespace Backynet.Tests;

public class JobDescriptorTests
{
    [Fact]
    public void Should_Return_New_JobDescriptor_When_One_Argument_Is_Const()
    {
        // arrange

        const string fakeArgument = "arg";
        Expression<Func<Task>> expression = () => FakeAsyncMethod(fakeArgument);

        // act

        var job = JobDescriptorFactory.Create(expression);

        // assert

        Assert.NotNull(job.Method);
        Assert.NotNull(job.Arguments);
    }

    [Fact]
    public void Should_Return_New_JobDescriptor_When_One_Argument_Is_Null()
    {
        // arrange

        string fakeArgument = null;
        Expression<Func<Task>> expression = () => FakeAsyncMethod(fakeArgument);

        // act

        var job = JobDescriptorFactory.Create(expression);

        // assert

        Assert.NotNull(job.Method);
        Assert.NotNull(job.Arguments);
    }

    private static Task FakeAsyncMethod(string fakeArg1)
    {
        return Task.CompletedTask;
    }

    private static Task FakeAsyncMethod(int fakeArg1, FakeDto fakeArg2)
    {
        return Task.CompletedTask;
    }

    private record FakeDto
    {
        public string Username { get; set; }
        public int Age { get; set; }
    }
}