using Backynet;
using Backynet.Options;
using Microsoft.Extensions.Configuration;

var configurationRoot = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var optionsBuilder = new BackynetContextOptionsBuilder()
    .UsePostgreSql(configurationRoot.GetConnectionString("PostgreSql"));
var backynetContext = new DefaultBackynetContext(optionsBuilder.Options);

using var cts = new CancellationTokenSource();

await backynetContext.Client.EnqueueAsync(() => Calculator.Calculate(100, 200));

await backynetContext.Server.Start(cts.Token);


await backynetContext.Client.EnqueueAsync(() => Calculator.Calculate(100, 200));

Console.ReadKey();

public static class Calculator
{
    public static void Calculate(int a, int b)
    {
        Console.WriteLine($"Sum is {a + b}");
    }
}

public class DefaultBackynetContext : BackynetContext
{
    public DefaultBackynetContext(BackynetContextOptions options) : base(options)
    {
    }
}