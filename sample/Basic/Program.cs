using Backynet;
using Backynet.Options;

var optionsBuilder = new BackynetContextOptionsBuilder()
    .UsePostgreSql(Environment.GetEnvironmentVariable("BACKYNET_CONNECTION_STRING"));
var defaultBackynetContext = new DefaultBackynetContext(optionsBuilder.Options);

using var cts = new CancellationTokenSource();
await defaultBackynetContext.Server.Start(cts.Token);

await defaultBackynetContext.Client.EnqueueAsync(() => Calculator.Calculate(100, 200));

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