using Backynet.Postgresql;
using Backynet.Tests;

namespace Backynet.PostgreSql.Tests;

public class ControllerServiceTests
{
    [Fact]
    public async Task Do()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var controllerService = new ControllerService(factory, TimeSpan.FromSeconds(10));
        await controllerService.Heartbeat("server_name");
    }
}