using Backynet.Options;

namespace Backynet.Tests;

public class BackynetContextTests
{
    [Fact]
    public void Do()
    {
        var backynetContextOptionsBuilder = new BackynetContextOptionsBuilder(); 
        backynetContextOptionsBuilder.UsePostgreSql(TestContext.ConnectionString);
        
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