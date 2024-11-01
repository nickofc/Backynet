using BenchmarkDotNet.Attributes;
using Hangfire;
using Hangfire.PostgreSql;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class HangfireBenchmark
{
    public HangfireBenchmark()
    {
        GlobalConfiguration.Configuration.UsePostgreSqlStorage(TestContext.ConnectionString);
    }

    [IterationSetup]
    public void Setup()
    {
        Clear.HangfireDatabase();
    }

    [Params(100)]
    public int N { get; set; }

    [Benchmark]
    public void Enqueue()
    {
        for (var i = 0; i < N; i++)
        {
            BackgroundJob.Enqueue(() => TestMethod());
        }
    }

    public static void TestMethod()
    {
    }
}