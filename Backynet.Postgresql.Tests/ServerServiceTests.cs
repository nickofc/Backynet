using Backynet.Tests;

namespace Backynet.PostgreSql.Tests;

public class ServerServiceTests
{
    [Fact]
    public async Task Do()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var controllerService = new ServerService(factory, TimeSpan.FromSeconds(10));
        await controllerService.Heartbeat("server_name");
    }
}