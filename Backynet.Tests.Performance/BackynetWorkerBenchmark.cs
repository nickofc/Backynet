using BenchmarkDotNet.Attributes;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class BackynetWorkerBenchmark
{
    private static CountdownEvent _countdownEvent = default!;

    [Params(100)]
    public int N { get; set; } = default!;

    [IterationSetup]
    public void Setup()
    {
        _countdownEvent = new CountdownEvent(N);
    }
    
    [Benchmark]
    public async Task Execute()
    {
        using var sut = new Sut();
        await sut.Start();

        for (var i = 0; i < N; i++)
        {
            await sut.BackynetClient.EnqueueAsync(() => TestMethod());
        }

        _countdownEvent.Wait();
    }

    [IterationCleanup]
    public void Cleanup()
    {
        _countdownEvent.Dispose();
    }

    public static void TestMethod()
    {
        _countdownEvent.Signal();
    }
}