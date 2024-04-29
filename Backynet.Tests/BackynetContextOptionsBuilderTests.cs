using Backynet.Core;
using Backynet.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.Tests;

public class BackynetContextOptionsBuilderTests
{
    [Fact]
    public void Should_Build_Options_When_Executed_With_Valid_Arguments()
    {
        var expectedCommandTimeout = TimeSpan.FromSeconds(30);
        const string expectedConnectionString = "test-connection-string";
        const bool expectedAutomaticMigration = true;

        var backynetContextOptionsBuilder = new BackynetContextOptionsBuilder();

        backynetContextOptionsBuilder.UsePostgreSql(expectedConnectionString, postgreSqlBackynetContextOptionsBuilder =>
        {
            postgreSqlBackynetContextOptionsBuilder
                .UseAutomaticMigration(expectedAutomaticMigration)
                .UseCommandTimeout(expectedCommandTimeout);
        });

        var postgreSqlOptionsExtension = backynetContextOptionsBuilder.Options.FindExtension<PostgreSqlOptionsExtension>();

        Assert.NotNull(postgreSqlOptionsExtension);
        Assert.Equal(expectedCommandTimeout, postgreSqlOptionsExtension.CommandTimeout);
        Assert.Equal(expectedConnectionString, postgreSqlOptionsExtension.ConnectionString);
        Assert.Equal(expectedAutomaticMigration, postgreSqlOptionsExtension.IsAutomaticMigrationEnabled);
    }
}