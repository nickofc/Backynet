# Backynet

[![latest version](https://img.shields.io/nuget/v/Backynet)](https://www.nuget.org/packages/Backynet) [![preview version](https://img.shields.io/nuget/vpre/Backynet)](https://www.nuget.org/packages/Backynet/absoluteLatest) [![downloads](https://img.shields.io/nuget/dt/Backynet)](https://www.nuget.org/packages/Backynet) [![100 - commitow](https://img.shields.io/badge/100%20-commitow-lightgreen.svg)](https://100commitow.pl)

Backynet is a library designed for performing background tasks such as sending emails or processing large files. It focuses on high performance, is built around the .NET ecosystem, and utilizes all its latest features.

Every task added to the queue is guaranteed "at least once delivery", so you don't have to worry about data loss. The library's architecture allows for easy addition of new nodes by specifying the same connection string. Because jobs are serialized, they can be executed on any of the available nodes.

# Installation

Backynet is available on NuGet. Install the provider package corresponding to your target database or message broker (In development, only PostgreSql is supported).

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

# Benchmarks

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19044.4412/21H2/November2021Update)
Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK 9.0.100-preview.3.24204.13
  [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
  Job-HUDYGN : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  WorkerCount=40  
```
| Type              | Method       | N   | Mean     | Error    | StdDev   | Allocated  |
|------------------ |------------- |---- |---------:|---------:|---------:|-----------:|
| BackynetBenchmark | EnqueueAsync | 100 |  2.863 s | 0.0332 s | 0.0311 s |  680.91 KB |
| HangfireBenchmark | Enqueue      | 100 | 23.130 s | 0.1433 s | 0.1270 s | 4244.99 KB |

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19044.4412/21H2/November2021Update)
Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK 9.0.100-preview.3.24204.13
  [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
  Job-PDYXJX : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  WorkerCount=40
```
| Type                    | Method  | N   | Mean     | Error    | StdDev   | Median   | Gen0      | Gen1      | Allocated |
|------------------------ |-------- |---- |---------:|---------:|---------:|---------:|----------:|----------:|----------:|
| BackynetWorkerBenchmark | Execute | 100 |  3.728 s | 0.2856 s | 0.8422 s |  3.032 s |         - |         - |   1.66 MB |
| HangfireWorkerBenchmark | Execute | 100 | 20.693 s | 0.2027 s | 0.1797 s | 20.689 s | 1000.0000 | 1000.0000 |  15.09 MB |
