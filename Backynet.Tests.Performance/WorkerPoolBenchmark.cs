using Backynet.Core;
using BenchmarkDotNet.Attributes;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class WorkerPoolBenchmark
{
    private readonly WorkerPool<object> _workerPool;
    private readonly object _data;
    
    public WorkerPoolBenchmark()
    {
        _workerPool = new WorkerPool<object>(10, Callback);
        _data = new object();

        _ = _workerPool.Start(CancellationToken.None);
    }

    [Benchmark]
    public Task Post()
    {
        return _workerPool.Post(_data);
    }
    
    private Task Callback(object arg1, CancellationToken arg2)
    {
        return Task.CompletedTask;
    }
}