using Backynet.Tests.Performance;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .With(ConfigOptions.JoinSummary | ConfigOptions.Default);

BenchmarkRunner.Run(new[] { typeof(HangfireBenchmark), typeof(BackynetBenchmark), typeof(HangfireStaticBenchmark) }, config);