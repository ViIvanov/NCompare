using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using NCompare.Benchmarks;

if(args is null || args.Length is 0) {
  args = ["--filter", "*", "--runtimes", "net461", "net472", "net60", "net80"];
}//if

var ee = new CompareToBenchmarks();

var config = ManualConfig.CreateEmpty()
  //.AddJob(Configure(Job.Default.WithRuntime(ClrRuntime.Net461)))
  .AddJob(Configure(Job.Default.WithRuntime(ClrRuntime.Net472)))
  //.AddJob(Configure(Job.Default.WithRuntime(CoreRuntime.Core60)))
  //.AddJob(Configure(Job.Default.WithRuntime(CoreRuntime.Core80)))
  .AddColumn(JobCharacteristicColumn.AllColumns)
  .AddColumn(BenchmarkOperationColumn.TypeKind, BenchmarkOperationColumn.Operation, BenchmarkComparerColumn.Default)
  .AddColumn(StatisticColumn.Mean, StatisticColumn.StdErr, StatisticColumn.StdDev)
  .AddColumn(BaselineRatioColumn.RatioMean)
  .AddLogger(ConsoleLogger.Unicode)
  .WithOrderer(BenchmarkOrderer.Default)
  .AddExporter(DefaultExporters.RPlot, DefaultExporters.Csv, DefaultExporters.Html, DefaultExporters.Markdown)
  .AddAnalyser(EnvironmentAnalyser.Default)
  .WithOptions(ConfigOptions.JoinSummary)
#if DEBUG
  .WithOptions(ConfigOptions.DisableOptimizationsValidator)
#endif // DEBUG
  .WithSummaryStyle(SummaryStyle.Default)
  .WithUnionRule(ConfigUnionRule.Union)
  /*.WithToolchain(InProcessNoEmitToolchain.Instance)*/;
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

Console.WriteLine();
Console.WriteLine("Benchmark finished.");
//Console.WriteLine("Benchmark finished. Press <Enter> for exit.");
//Console.ReadLine();

static Job Configure(Job job)
#if DEBUG
    => job.WithWarmupCount(0).WithIterationCount(10).WithInvocationCount(1).WithUnrollFactor(1);
#else
    => job;
#endif // DEBUG
