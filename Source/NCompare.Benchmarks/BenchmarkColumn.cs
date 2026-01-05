using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks;

internal abstract class BenchmarkColumn(IColumn source) : IColumn
{
  protected IColumn SourceColumn { get; } = source ?? throw new ArgumentNullException(nameof(source));

  public virtual string Id => GetType().Name;

  public abstract string ColumnName { get; }

  public virtual bool AlwaysShow => SourceColumn.AlwaysShow;
  public virtual ColumnCategory Category => SourceColumn.Category;
  public virtual int PriorityInCategory => SourceColumn.PriorityInCategory;
  public virtual bool IsNumeric => SourceColumn.IsNumeric;
  public virtual UnitType UnitType => SourceColumn.UnitType;
  public virtual string Legend => SourceColumn.Legend;

  public virtual string GetValue(Summary summary, BenchmarkCase benchmarkCase) => SourceColumn.GetValue(summary, benchmarkCase);
  public virtual string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
  public virtual bool IsAvailable(Summary summary) => SourceColumn.IsAvailable(summary);
  public virtual bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => SourceColumn.IsDefault(summary, benchmarkCase);

  public override string ToString() => ColumnName;
}
