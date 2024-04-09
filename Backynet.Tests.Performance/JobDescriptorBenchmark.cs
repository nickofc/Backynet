using Backynet.Core.Abstraction;
using BenchmarkDotNet.Attributes;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class JobDescriptorBenchmark
{
    [Benchmark]
    public IJobDescriptor Create()
    {
        return JobDescriptorFactory.Create(() => FakeAsyncMethod());
    }
    
    private static Task FakeAsyncMethod()
    {
        return Task.CompletedTask;
    }
}