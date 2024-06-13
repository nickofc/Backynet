using Backynet.Options;
using Xunit.Abstractions;

namespace Backynet.Tests;

public class BackynetContextFactory
{
    private readonly BackynetContextOptions _options;

    public BackynetContextFactory(ITestOutputHelper? testOutputHelper = null)
    {
        var optionsBuilder = new BackynetContextOptionsBuilder()
            .UsePostgreSql(TestContext.ConnectionString);

        if (testOutputHelper != null)
        {
            optionsBuilder.UseLoggerFactory(new DebugLoggerFactory(testOutputHelper));
        }

        _options = optionsBuilder.Options;
    }

    public BackynetContext Create()
    {
        return new BackynetContext(_options);
    }
}