using System.Collections.Immutable;
using System.Text;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks;

using static BenchmarkOperationColumn;

internal sealed class BenchmarkOrderer : IOrderer
{
  public static BenchmarkOrderer Default { get; } = new BenchmarkOrderer();

  public bool SeparateLogicalGroups => true;

  public IEnumerable<BenchmarkCase> GetExecutionOrder(ImmutableArray<BenchmarkCase> benchmarksCase, IEnumerable<BenchmarkLogicalGroupRule>? order = null) => benchmarksCase;

  public IEnumerable<BenchmarkCase> GetSummaryOrder(ImmutableArray<BenchmarkCase> benchmarksCase, Summary summary)
    => benchmarksCase.OrderBy(item => GetBenchmarkOrderKey(item, includeMethod: true));

  public string GetHighlightGroupKey(BenchmarkCase benchmarkCase)
    => GetBenchmarkOrderKey(benchmarkCase, includeMethod: false);

  public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase)
    => GetBenchmarkOrderKey(benchmarkCase, includeMethod: false);

  public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups, IEnumerable<BenchmarkLogicalGroupRule>? order = null)
    => logicalGroups.OrderBy(item => item.Key);

  private static string GetBenchmarkOrderKey(BenchmarkCase benchmarkCase, bool includeMethod) {
    var builder = new StringBuilder();
    builder.Append(benchmarkCase.Job.Id);

    var categories = GetTypeBenchmarkCategories(benchmarkCase);

    var comparerKind = IsEqualityComparerKind(categories) switch {
      true => "0: Equality Comparer",
      false => "1: Comparer",
      null => "2: Unknown",
    };
    builder.Append('|').Append(comparerKind);

    var benchmarkKind = GetBenchmarkKind(categories) switch {
      BenchmarkCategories.Equal => $"0: {BenchmarkCategories.Equal}",
      BenchmarkCategories.NotEqual => $"1: {BenchmarkCategories.NotEqual}",
      BenchmarkCategories.GetHashCode => $"2: {BenchmarkCategories.GetHashCode}",
      BenchmarkCategories.Sort => $"3: {BenchmarkCategories.Sort}",
      var unknown => $"4: Unknown: {unknown}",
    };
    builder.Append('|').Append(benchmarkKind);

    var typeKind = IsReferenceTypeKind(categories) switch {
      true => "0: Reference Type",
      false => "1: Value Type",
      null => "2: Unknown",
    };
    builder.Append('|').Append(typeKind);

    if(includeMethod) {
      var methodKind = benchmarkCase.Descriptor.WorkloadMethodDisplayInfo switch {
        BenchmarkDescriptions.IEquatable => $"0: {BenchmarkDescriptions.IEquatable}",
        BenchmarkDescriptions.IComparable => $"1: {BenchmarkDescriptions.IComparable}",
        BenchmarkDescriptions.EqualityComparer => $"2: {BenchmarkDescriptions.EqualityComparer}",
        BenchmarkDescriptions.Comparer => $"3: {BenchmarkDescriptions.Comparer}",
        BenchmarkDescriptions.ComparerBuilder => $"4: {BenchmarkDescriptions.ComparerBuilder}",
        BenchmarkDescriptions.NitoComparers => $"5: {BenchmarkDescriptions.NitoComparers}",
        var unknown => $"6: Unknown: {unknown}",
      };
      builder.Append('|').Append(methodKind);
    }//if

    return builder.ToString();
  }
}
