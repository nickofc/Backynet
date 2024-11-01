using JetBrains.Annotations;

namespace Backynet.Tests;

[TestSubject(typeof(ThreadPool))]
public class ThreadPoolTest
{
    public ThreadPoolOptions Options { get; }
    public ThreadPool ThreadPool { get; }

    public ThreadPoolTest()
    {
        Options = new ThreadPoolOptions { MaxThreads = 20 };
        ThreadPool = new ThreadPool(Options);
    }

    [Fact]
    public void Should_MaxThread_Be_Equal_To_AvailableThread_Count()
    {
        Assert.Equal(Options.MaxThreads, ThreadPool.AvailableThreadCount);
    }

    [Fact]
    public async Task Should_Decrease_AvailableThreadCount_When_Post_Executed()
    {
        var expectedAvailableThreadCount = Options.MaxThreads - 1;

        await ThreadPool.Post(() => Task.Delay(-1));

        Assert.Equal(expectedAvailableThreadCount, ThreadPool.AvailableThreadCount);
    }

    [Fact]
    public async Task Should_Decrease_AvailableThreadCount_When_MethodCall_Was_Completed()
    {
        var completionEvent = new ManualResetEventSlim();

        await ThreadPool.Post(() =>
        {
            completionEvent.Wait();
            return Task.CompletedTask;
        });

        completionEvent.Set();
        await Task.Delay(1);

        Assert.Equal(Options.MaxThreads, ThreadPool.AvailableThreadCount);
    }

    [Fact]
    public async Task Should_Decrease_AvailableThreadCount_When_MethodCall_Was_Faulted()
    {
        var completionEvent = new ManualResetEventSlim();

        await ThreadPool.Post(() =>
        {
            completionEvent.Wait();
            throw new InvalidOperationException();
        });

        completionEvent.Set();
        await Task.Delay(1);

        Assert.Equal(Options.MaxThreads, ThreadPool.AvailableThreadCount);
    }

    [Fact]
    public async Task Should_Wait_For_Task_To_Complete_When_There_Is_No_Available_Thread()
    {
        for (var i = 0; i < Options.MaxThreads; i++)
        {
            await ThreadPool.Post(InfinityTask);
        }

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        try
        {
            await ThreadPool.Post(InfinityTask, cts.Token);
        }
        catch (OperationCanceledException e) when (e.CancellationToken == cts.Token)
        {
        }

        Assert.True(cts.IsCancellationRequested);
        Assert.Equal(0, ThreadPool.AvailableThreadCount);

        return;

        async Task InfinityTask()
        {
            await Task.Delay(-1, CancellationToken.None);
        }
    }

    [Fact]
    public async Task Should_Return_Immediate_When_Thread_Is_Available()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await ThreadPool.WaitForAvailableThread(cts.Token);

        Assert.Equal(Options.MaxThreads, ThreadPool.AvailableThreadCount);
    }

    [Fact]
    public async Task Should_Release_Task_From_Pool_When_Any_Task_Was_Completed()
    {
        for (var i = 0; i < Options.MaxThreads - 1; i++)
        {
            await ThreadPool.Post(() => Task.Delay(-1));
        }

        await ThreadPool.Post(() => Task.Delay(1000));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await ThreadPool.WaitForAvailableThread(cts.Token);

        Assert.Equal(1, ThreadPool.AvailableThreadCount);
    }
}