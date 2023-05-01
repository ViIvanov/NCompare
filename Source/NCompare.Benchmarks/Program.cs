using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using NCompare.Benchmarks;

if(args is null || args.Length == 0) {
  args = new[] { "--filter", "*", };
}//if

var config = ManualConfig.CreateEmpty()
  .AddJob(Configure(Job.Default.WithRuntime(ClrRuntime.Net461)))
  .AddJob(Configure(Job.Default.WithRuntime(ClrRuntime.Net472)))
  .AddJob(Configure(Job.Default.WithRuntime(CoreRuntime.Core70)))
  .AddJob(Configure(Job.Default.WithRuntime(CoreRuntime.Core80)))
  .AddColumn(JobCharacteristicColumn.AllColumns)
  .AddColumn(BenchmarkOperationColumn.TypeKind, BenchmarkOperationColumn.Operation, BenchmarkComparerColumn.Default)
  .AddColumn(StatisticColumn.Mean, StatisticColumn.StdErr, StatisticColumn.StdDev)
  .AddColumn(BaselineRatioColumn.RatioMean)
  .AddLogger(ConsoleLogger.Unicode)
  .WithOrderer(BenchmarkOrderer.Default)
  .AddExporter(DefaultExporters.RPlot, DefaultExporters.Csv, DefaultExporters.Html, DefaultExporters.Markdown)
  .AddAnalyser(EnvironmentAnalyser.Default)
  .WithOptions(ConfigOptions.JoinSummary)
  .WithSummaryStyle(SummaryStyle.Default)
  .WithUnionRule(ConfigUnionRule.Union);
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

Console.WriteLine();
Console.WriteLine("Benchmark finished. Press <Enter> for exit.");
Console.ReadLine();

static Job Configure(Job job)
#if DEBUG
    => job.WithWarmupCount(0).WithIterationCount(10).WithInvocationCount(1).WithUnrollFactor(1);
#else
    => job;
#endif // DEBUG
