using System;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks;

internal static partial class Program
{
  private static void Main(string[] args) {
    if(args is null || args.Length == 0) {
      args = new[] { "--filter", "*", };
    }//if

    var config = ManualConfig.CreateEmpty()
      .AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithWarmupCount(0).WithIterationCount(10).WithInvocationCount(1).WithUnrollFactor(1))
      .AddJob(Job.Default.WithRuntime(ClrRuntime.Net472).WithWarmupCount(0).WithIterationCount(10).WithInvocationCount(1).WithUnrollFactor(1))
      .AddJob(Job.Default.WithRuntime(CoreRuntime.Core70).WithWarmupCount(0).WithIterationCount(10).WithInvocationCount(1).WithUnrollFactor(1))
      .AddColumn(JobCharacteristicColumn.AllColumns)
      .AddColumn(TargetMethodColumn.Type, TargetMethodColumn.Method)
      .AddColumn(StatisticColumn.Mean)
      .AddColumn(BaselineRatioColumn.RatioMean)
      .AddLogger(ConsoleLogger.Unicode)
      .WithOrderer(new BenchmarkOrderer())
      .AddExporter(DefaultExporters.RPlot, DefaultExporters.Csv, DefaultExporters.Html)
      .AddAnalyser(EnvironmentAnalyser.Default)
      .WithOptions(ConfigOptions.JoinSummary)
      .WithSummaryStyle(SummaryStyle.Default)
      .WithUnionRule(ConfigUnionRule.Union);
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

    Console.WriteLine("Benchmark finished. Press <Enter> for exit.");
    Console.ReadLine();
  }
}
