using BenchmarkDotNet.Attributes;
using Hangfire;
using Hangfire.PostgreSql;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class HangfireStaticBenchmark
{
    public HangfireStaticBenchmark()
    {
        GlobalConfiguration.Configuration.UsePostgreSqlStorage(TestContext.ConnectionString);
    }

    [Benchmark]
    public void Enqueue()
    {
        BackgroundJob.Enqueue(() => TestMethod());
    }

    public static void TestMethod()
    {
    }
}