using Backynet.PostgreSql;

namespace Backynet.Tests;

public class EmbeddedResourceTests
{
    [Fact]
    public void Should_Find_Return_Collection_Matched_Entries()
    {
        var assemblies = new[] { typeof(EmbeddedResource).Assembly };

        var entries = EmbeddedResource.Find(x => x.EndsWith("_Migration.sql", StringComparison.InvariantCulture), assemblies);

        Assert.Single(entries);
    }
}