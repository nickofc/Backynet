using Backynet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.Tests;

public class BackynetContextOptionsBuilderTests
{
    [Fact]
    public void Do()
    {
        var optionsBuilder = new BackynetContextOptionsBuilder();

        optionsBuilder.UsePostgreSql("connection-string", builder =>
        {
            builder.UseAutomaticMigration(true)
                .UseCommandTimeout(TimeSpan.FromSeconds(10));
        });

        var options = optionsBuilder.Options;

        Assert.Equal(1, options.Extensions.Count());
    }
}