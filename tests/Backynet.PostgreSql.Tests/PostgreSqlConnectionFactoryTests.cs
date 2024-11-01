using Backynet.Tests;
using Npgsql;

namespace Backynet.PostgreSql.Tests;

public class PostgreSqlConnectionFactoryTests
{
    [Fact]
    public async Task Should_Return_Opened_Connection_When_GetAsync_Is_Called()
    {
        var factory = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        await using var connection = factory.Get();
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await connection.OpenAsync();
        await command.ExecuteReaderAsync();
    }
}