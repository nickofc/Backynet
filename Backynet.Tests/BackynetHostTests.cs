using Backynet.Core;
using Backynet.Postgresql;
using Backynet.PostgreSql;

namespace Backynet.Tests;

public class BackynetHostTests
{
    [Fact(Skip = "wip")]
    public async Task Should()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var controllerService = new ControllerService(factory, TimeSpan.FromSeconds(10));

        var s = new BackynetHost(controllerService, new BackynetHostOptions() { ServerName = "test-host", HeartbeatInterval = TimeSpan.FromSeconds(1)});
        await s.Start(default);
    }
}