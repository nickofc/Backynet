using Backynet.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.PostgreSql.Tests;

public class ServerServiceTests
{
    [Fact]
    public async Task Should_Update_Heartbeat()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        var controllerService = new ServerService(factory, new ServerServiceOptions { MaxTimeWithoutHeartbeat = TimeSpan.FromSeconds(30) });

        await controllerService.Heartbeat();
    }
}