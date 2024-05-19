using BenchmarkDotNet.Attributes;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;

namespace Backynet.Tests.Performance;

[MemoryDiagnoser]
public class HangfireBenchmark
{
    private readonly BackgroundJobClient _backgroundJobClient;

    public HangfireBenchmark()
    {
        var connectionFactory = new NpgsqlConnectionFactory(TestContext.ConnectionString, new PostgreSqlStorageOptions());
        var sqlStorage = new PostgreSqlStorage(connectionFactory);

        _backgroundJobClient = new BackgroundJobClient(sqlStorage);
    }

    [Benchmark]
    public void Enqueue()
    {
        _backgroundJobClient.Enqueue(() => TestMethod());
    }

    public static void TestMethod()
    {
    }
}