# Backynet

[![latest version](https://img.shields.io/nuget/v/Backynet)](https://www.nuget.org/packages/Backynet) [![preview version](https://img.shields.io/nuget/vpre/Backynet)](https://www.nuget.org/packages/Backynet/absoluteLatest) [![downloads](https://img.shields.io/nuget/dt/Backynet)](https://www.nuget.org/packages/Backynet) [![100 - commitow](https://img.shields.io/badge/100%20-commitow-lightgreen.svg)](https://100commitow.pl)


// todo - nuget

// todo - description

# Installation

Backynet is available on NuGet. Install the provider package corresponding to your target database or message broker.  
See the list of providers in the docs for additional databases.

```bash
dotnet add package Backynet.PostgreSql
```

# Basic usage

```csharp
var optionsBuilder = new BackynetContextOptionsBuilder().UsePostgreSql("<connection-string>");
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
```

Check out more [examples](https://github.com/nickofc/backynet/tree/master/example/).
