using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks;

internal sealed class BenchmarkComparerColumn : BenchmarkColumn
{
  private BenchmarkComparerColumn() : base(TargetMethodColumn.Method) { }

  public static IColumn Default { get; } = new BenchmarkComparerColumn();

  public override string ColumnName => "Comparer";
  public override string Legend => "The comparer used to compare objects";

  public override string GetValue(Summary summary, BenchmarkCase benchmarkCase) => $"`{benchmarkCase.Descriptor.WorkloadMethodDisplayInfo}`";
}
