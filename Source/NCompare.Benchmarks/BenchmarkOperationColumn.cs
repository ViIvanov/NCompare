using System.Reflection;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks;

internal sealed class BenchmarkOperationColumn : BenchmarkColumn
{
  private enum ColumnKind
  {
    TypeKind, // Reference Type or Value Type
    Operation, // Equal, NotEqual, GetHashCode or Sort
  }

  private BenchmarkOperationColumn(ColumnKind kind, IColumn source) : base(source) => Kind = kind;

  public static BenchmarkOperationColumn TypeKind { get; } = new(ColumnKind.TypeKind, TargetMethodColumn.Type);
  public static BenchmarkOperationColumn Operation { get; } = new(ColumnKind.Operation, TargetMethodColumn.Type);

  private ColumnKind Kind { get; }

  public override string Id => $"{nameof(BenchmarkOperationColumn)}.{Kind}";

  public override string ColumnName => Kind switch {
    ColumnKind.TypeKind => "Type Kind",
    var other => other.ToString(),
  };

  public override string Legend => Kind switch {
    ColumnKind.TypeKind => "The type of reference or value is being compared",
    ColumnKind.Operation => "The method being evaluated",
    var unknown => $"Unknown: {unknown}",
  };

  public override string GetValue(Summary summary, BenchmarkCase benchmarkCase) {
    var categories = GetTypeBenchmarkCategories(benchmarkCase);
    return Kind switch {
      ColumnKind.TypeKind => IsReferenceTypeKind(categories) switch {
        true => "`class`",
        false => "`struct`",
        null => "Unknown",
      },
      ColumnKind.Operation => (IsEqualityComparerKind(categories), GetBenchmarkKind(categories)) switch {
        (true, BenchmarkCategories.Equal) => $"{nameof(IEquatable<>.Equals)}(equal)",
        (true, BenchmarkCategories.NotEqual) => $"{nameof(IEquatable<>.Equals)}(not-equal)",
        (false, BenchmarkCategories.Equal) => $"{nameof(IComparable<>.CompareTo)}(equal)",
        (false, BenchmarkCategories.NotEqual) => $"{nameof(IComparable<>.CompareTo)}(not-equal)",
        (_, BenchmarkCategories.GetHashCode) => nameof(BenchmarkCategories.GetHashCode),
        (_, BenchmarkCategories.Sort) => nameof(BenchmarkCategories.Sort),
        (var kind, var unknown) => $"Unknown: {unknown}({kind})",
      },
      var unknown => $"Unknown: {unknown}",
    };
  }

  public static string[] GetTypeBenchmarkCategories(BenchmarkCase benchmarkCase) => benchmarkCase?.Descriptor.Type.GetCustomAttribute<BenchmarkCategoryAttribute>()?.Categories ?? [];
  public static bool? IsReferenceTypeKind(string[] categories) => categories.Contains(BenchmarkCategories.ReferenceType) ? true : categories.Contains(BenchmarkCategories.ValueType) ? false : null;
  public static bool? IsEqualityComparerKind(string[] categories) => categories.Contains(BenchmarkCategories.EqualityComparer) ? true : categories.Contains(BenchmarkCategories.Comparer) ? false : null;
  public static string GetBenchmarkKind(string[] categories) => categories.LastOrDefault() ?? String.Empty;
}
