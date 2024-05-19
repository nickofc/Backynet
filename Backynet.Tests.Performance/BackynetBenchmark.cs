using BenchmarkDotNet.Attributes;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class BackynetBenchmark
{
    private readonly Sut _sut = new();

    [Benchmark]
    public async Task EnqueueAsync()
    {
        await _sut.BackynetClient.EnqueueAsync(() => TestMethod());
    }

    public static void TestMethod()
    {
    }
}