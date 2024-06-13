using Backynet.Options;

namespace Backynet.Tests.BackynetContextTests;

// ReSharper disable once InconsistentNaming
public class BackynetContext_Create
{
    [Fact]
    public void When_Called_With_Valid_Options_Should_Create_BackynetContext()
    {
        var backynetContextOptionsBuilder = new BackynetContextOptionsBuilder()
            .UsePostgreSql(TestContext.ConnectionString);

        var backynetContext = new AppBackynetContext(backynetContextOptionsBuilder.Options);

        Assert.NotNull(backynetContext.Client);
        Assert.NotNull(backynetContext.Server);
    }

    public class AppBackynetContext : BackynetContext
    {
        public AppBackynetContext(BackynetContextOptions options) : base(options)
        {
        }
    }
}