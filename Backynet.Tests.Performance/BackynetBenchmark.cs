using BenchmarkDotNet.Attributes;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class BackynetBenchmark
{
    private readonly Sut _sut = new();

    [IterationSetup]
    public void Setup()
    {
        Clear.BackynetDatabase();
    }

    [Params(100)]
    public int N { get; set; }

    [Benchmark]
    public async Task EnqueueAsync()
    {
        for (var i = 0; i < N; i++)
        {
            await _sut.BackynetClient.EnqueueAsync(() => TestMethod());
        }
    }

    public static void TestMethod()
    {
    }
}