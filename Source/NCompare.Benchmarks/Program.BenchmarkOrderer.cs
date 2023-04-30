using System.Collections.Immutable;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks;

internal static partial class Program
{
  private sealed class BenchmarkOrderer : IOrderer
  {
    public bool SeparateLogicalGroups => true;

    public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarksCase, IEnumerable<BenchmarkLogicalGroupRule>? order = null) => benchmarksCase;

    public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarksCase, Summary summary)
      => benchmarksCase.OrderBy(item => (item.Job.ToString(), item.Descriptor.Type.Namespace, item.Descriptor.Type.Name));

    public string GetHighlightGroupKey(BenchmarkCase benchmarkCase)
      => $"{benchmarkCase.Job.DisplayInfo}|{benchmarkCase.Descriptor.Type.Namespace}|{benchmarkCase.Descriptor.Type.Name}";

    public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase)
      => $"{benchmarkCase.Job.DisplayInfo}|{benchmarkCase.Descriptor.Type.Namespace}|{benchmarkCase.Descriptor.Type.Name}";

    public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups, IEnumerable<BenchmarkLogicalGroupRule>? order = null)
      => logicalGroups.OrderBy(item => item.Key);
  }
}
