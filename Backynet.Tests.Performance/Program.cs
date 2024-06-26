using Backynet.Tests.Performance;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .With(ConfigOptions.JoinSummary | ConfigOptions.Default);

// BenchmarkRunner.Run(new[] { typeof(BackynetBenchmark), typeof(HangfireBenchmark) }, config);

BenchmarkRunner.Run(new[] { typeof(BackynetWorkerBenchmark), typeof(HangfireWorkerBenchmark) }, config);