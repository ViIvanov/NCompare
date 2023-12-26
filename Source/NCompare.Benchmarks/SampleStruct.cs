using BenchmarkDotNet.Attributes;

using Nito.Comparers;

namespace NCompare.Benchmarks;

public readonly struct SampleStruct(int number, DateTime? dateTime, string? text) : IEquatable<SampleStruct>, IComparable<SampleStruct>
{
  public int Number { get; } = number;
  public DateTime? NullableDateTime { get; } = dateTime;
  public string? Text { get; } = text;

  public bool Equals(SampleStruct other)
    => other.Number == Number && other.NullableDateTime == NullableDateTime && other.Text == Text;

  public override bool Equals(object? obj) => obj is SampleStruct other && Equals(other);

#if NETCOREAPP2_1_OR_GREATER
  public override int GetHashCode() => HashCode.Combine(Number, NullableDateTime, Text);
#else
  public override int GetHashCode() => Number.GetHashCode() ^ NullableDateTime.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
#endif // NETCOREAPP2_1_OR_GREATER

  public int CompareTo(SampleStruct other) {
    var compare = Number.CompareTo(other.Number);
    if(compare != 0) {
      return compare;
    }//if

    compare = Comparer<DateTime?>.Default.Compare(NullableDateTime, other.NullableDateTime);
    if(compare != 0) {
      return compare;
    }//if

    return String.Compare(Text, other.Text);
  }

  public static bool operator ==(SampleStruct left, SampleStruct right) => left.Equals(right);
  public static bool operator !=(SampleStruct left, SampleStruct right) => !(left == right);
}

internal sealed class SampleStructComparer : IEqualityComparer<SampleStruct>, IComparer<SampleStruct>
{
  public bool Equals(SampleStruct x, SampleStruct y) => x.Number == y.Number && x.NullableDateTime == y.NullableDateTime && x.Text == y.Text;

#if NETCOREAPP2_1_OR_GREATER
  public int GetHashCode(SampleStruct obj) => HashCode.Combine(obj.Number, obj.NullableDateTime, obj.Text);
#else
  public int GetHashCode(SampleStruct obj) => obj.Number.GetHashCode() ^ obj.NullableDateTime.GetHashCode() ^ (obj.Text?.GetHashCode() ?? 0);
#endif // NETCOREAPP2_1_OR_GREATER

  public int Compare(SampleStruct x, SampleStruct y) {
    var compare = x.Number.CompareTo(y.Number);
    if(compare != 0) {
      return compare;
    }//if

    compare = Comparer<DateTime?>.Default.Compare(x.NullableDateTime, y.NullableDateTime);
    if(compare != 0) {
      return compare;
    }//if

    return String.Compare(x.Text, y.Text);
  }
}

internal static class SampleStructBenchmarks
{
  private static readonly DateTime DateTimeValue = DateTime.UtcNow;
  public static SampleStruct Item1_1 { get; } = new(number: 1, dateTime: DateTimeValue, text: "Item1");
  public static SampleStruct Item1_2 { get; } = new(number: 1, dateTime: DateTimeValue, text: "Item1");
  public static SampleStruct Item2 { get; } = new(number: 1, dateTime: DateTimeValue, text: "Item2");

  public static SampleStruct[] AllItems = [Item1_1, Item1_2, Item2];

  public static SampleStructComparer Comparer { get; } = new();

  public static ComparerBuilder<SampleStruct> ComparerBuilder { get; } = new ComparerBuilder<SampleStruct>()
    .Add(item => item.Number)
    .Add(item => item.NullableDateTime)
    .Add(item => item.Text);

  public static IFullComparer<SampleStruct> NitoFullComparer { get; } = Nito.Comparers.ComparerBuilder.For<SampleStruct>()
    .OrderBy(p => p.Number)
    .ThenBy(p => p.NullableDateTime)
    .ThenBy(p => p.Text);

  public static TestComparers<SampleStruct> AllComparers { get; } = new(Comparer, Comparer, ComparerBuilder, NitoFullComparer);
}

[BenchmarkCategory(BenchmarkCategories.ValueType, BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal)]
public class SampleStructEqualityComparerEqualBenchmarks : EqualityComparerEqualBenchmarks<SampleStruct>
{
  public SampleStructEqualityComparerEqualBenchmarks() : base(SampleStructBenchmarks.AllComparers, SampleStructBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ValueType, BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual)]
public class SampleStructEqualityComparerNotEqualBenchmarks : EqualityComparerNotEqualBenchmarks<SampleStruct>
{
  public SampleStructEqualityComparerNotEqualBenchmarks() : base(SampleStructBenchmarks.AllComparers, SampleStructBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ValueType, BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode)]
public class SampleStructEqualityComparerGetHashCodeBenchmarks : EqualityComparerGetHashCodeBenchmarks<SampleStruct>
{
  public SampleStructEqualityComparerGetHashCodeBenchmarks() : base(SampleStructBenchmarks.AllComparers, SampleStructBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ValueType, BenchmarkCategories.Comparer, BenchmarkCategories.Equal)]
public class SampleStructComparerEqualBenchmarks : ComparerEqualBenchmarks<SampleStruct>
{
  public SampleStructComparerEqualBenchmarks() : base(SampleStructBenchmarks.AllComparers, SampleStructBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ValueType, BenchmarkCategories.Comparer, BenchmarkCategories.NotEqual)]
public class SampleStructComparerNotEqualBenchmarks : ComparerNotEqualBenchmarks<SampleStruct>
{
  public SampleStructComparerNotEqualBenchmarks() : base(SampleStructBenchmarks.AllComparers, SampleStructBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ValueType, BenchmarkCategories.Comparer, BenchmarkCategories.Sort)]
public class SampleStructComparerSortBenchmarks : ComparerSortBenchmarks<SampleStruct>
{
  public SampleStructComparerSortBenchmarks() : base(SampleStructBenchmarks.AllComparers, SampleStructBenchmarks.AllItems) { }
}
