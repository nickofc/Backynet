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
        Assert.Equal(nameof(FakeAsyncMethod), job.Method.Name);
        Assert.Equal(typeof(JobDescriptorTests).AssemblyQualifiedName, job.Method.TypeName);

        Assert.NotNull(job.Arguments);
        Assert.Equal(fakeArgument, job.Arguments[0].Value);
        Assert.Equal(typeof(string).AssemblyQualifiedName, job.Arguments[0].TypeName);
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

    [Fact]
    public void Should_Return_New_JobDescriptor_When_One_Argument_Is_Int_And_Record()
    {
        // arrange

        var fakeArgument = new FakeDto();
        Expression<Func<Task>> expression = () => FakeAsyncMethod(1, fakeArgument);

        // act

        var job = JobDescriptorFactory.Create(expression);

        // assert

        Assert.NotNull(job.Method);
        Assert.Equal(nameof(FakeAsyncMethod), job.Method.Name);
        Assert.Equal(typeof(JobDescriptorTests).AssemblyQualifiedName, job.Method.TypeName);

        Assert.NotNull(job.Arguments);
        Assert.Equal(1, job.Arguments[0].Value);
        Assert.Equal(typeof(int).AssemblyQualifiedName, job.Arguments[0].TypeName);
        Assert.Equal(fakeArgument, job.Arguments[1].Value);
        Assert.Equal(typeof(FakeDto).AssemblyQualifiedName, job.Arguments[1].TypeName);
    }

    [Fact]
    public void Should_Return_New_JobDescriptor_When_Method_Is_Private()
    {
        // arrange

        Expression<Func<Task>> expression = () => FakePrivateAsyncMethod();

        // act

        var job = JobDescriptorFactory.Create(expression);

        // assert

        Assert.NotNull(job.Method);
        Assert.Equal(nameof(FakePrivateAsyncMethod), job.Method.Name);
        Assert.Equal(typeof(JobDescriptorTests).AssemblyQualifiedName, job.Method.TypeName);

        Assert.NotNull(job.Arguments);
        Assert.Empty(job.Arguments);
    }

    private Task FakePrivateAsyncMethod()
    {
        return Task.CompletedTask;
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