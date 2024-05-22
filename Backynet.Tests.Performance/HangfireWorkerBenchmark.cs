using BenchmarkDotNet.Attributes;
using Hangfire;
using Hangfire.PostgreSql;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class HangfireWorkerBenchmark
{
    private static CountdownEvent _countdownEvent = default!;

    [Params(100)]
    public int N { get; set; } = default!;

    [IterationSetup]
    public void Setup()
    {
        _countdownEvent = new CountdownEvent(N);

        GlobalConfiguration.Configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseColouredConsoleLogProvider()
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(TestContext.ConnectionString);
    }

    [Benchmark]
    public void Execute()
    {
        using var server = new BackgroundJobServer();

        for (var i = 0; i < N; i++)
        {
            BackgroundJob.Enqueue(() => TestMethod());
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