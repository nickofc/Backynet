using Microsoft.Extensions.Configuration;

namespace Backynet.Tests;

// TODO: replace with testcontainers
public sealed class TestContext
{
    private static readonly IConfigurationRoot Instance =
        new Lazy<IConfigurationRoot>(BuildConfiguration).Value;

    private static IConfigurationRoot BuildConfiguration()
    {
        var configurationRoot = new ConfigurationBuilder()
            .AddUserSecrets<TestContext>()
            .Build();

        return configurationRoot;
    }

    public static string ConnectionString { get; } = Instance.GetConnectionString("PostgreSql")
                                                     ?? throw new InvalidOperationException("Unable to load connection string.");
}