// Ignore Spelling: Nito Nullable

using BenchmarkDotNet.Attributes;

using Nito.Comparers;

namespace NCompare.Benchmarks;

#pragma warning disable CA1036 // Override methods on comparable types
public sealed class SampleClass : IEquatable<SampleClass>, IComparable<SampleClass>
#pragma warning restore CA1036 // Override methods on comparable types
{
  public int Number { get; set; }
  public DateTime? NullableDateTime { get; set; }
  public string? Text { get; set; }

  public bool Equals(SampleClass? other)
    => other == this || other is not null && other.Number == Number /*&& other.NullableDateTime == NullableDateTime && other.Text == Text*/;

  public override bool Equals(object? obj) => Equals(obj as SampleClass);

#if NETCOREAPP2_1_OR_GREATER
  public override int GetHashCode() => /*HashCode.Combine(*/Number/*, NullableDateTime, Text)*/;
#else
  public override int GetHashCode() => Number.GetHashCode() /*^ NullableDateTime.GetHashCode() ^ (Text?.GetHashCode() ?? 0)*/;
#endif // NETCOREAPP2_1_OR_GREATER

  public int CompareTo(SampleClass? other) {
    if(other is null) {
      return 1;
    }//if

    var compare = Number.CompareTo(other.Number);
    //if(compare != 0) {
      return compare;
    //}//if

    //compare = Comparer<DateTime?>.Default.Compare(NullableDateTime, other.NullableDateTime);
    //if(compare != 0) {
    //  return compare;
    //}//if

    //return String.Compare(Text, other.Text, StringComparison.Ordinal);
  }
}

internal sealed class SampleClassComparer : IEqualityComparer<SampleClass>, IComparer<SampleClass>
{
  public bool Equals(SampleClass? x, SampleClass? y) {
    if(x == y) {
      return true;
    } else if(x is null) {
      return y is null;
    } else if(y is null) {
      return false;
    } else {
      return x.Number == y.Number && x.NullableDateTime == y.NullableDateTime && x.Text == y.Text;
    }//if
  }

#if NETCOREAPP2_1_OR_GREATER
  public int GetHashCode(SampleClass? obj) => obj is not null ? HashCode.Combine(obj.Number, obj.NullableDateTime, obj.Text) : 0;
#else
  public int GetHashCode(SampleClass? obj) => obj is not null ? obj.Number.GetHashCode() ^ obj.NullableDateTime.GetHashCode() ^ (obj.Text?.GetHashCode() ?? 0) : 0;
#endif // NETCOREAPP2_1_OR_GREATER

  public int Compare(SampleClass? x, SampleClass? y) {
    if(x == y) {
      return 0;
    } else if(x is null) {
      return y is null ? 0 : -1;
    } else if(y is null) {
      return 1;
    } else {
      var compare = x.Number.CompareTo(y.Number);
      if(compare != 0) {
        return compare;
      }//if

      compare = Comparer<DateTime?>.Default.Compare(x.NullableDateTime, y.NullableDateTime);
      if(compare != 0) {
        return compare;
      }//if

      return String.Compare(x.Text, y.Text, StringComparison.Ordinal);
    }//if
  }
}

internal static class SampleClassBenchmarks
{
  private static readonly DateTime DateTimeValue = DateTime.UtcNow;
  public static SampleClass Item1_1 { get; } = new() { Number = 1, NullableDateTime = DateTimeValue, Text = "Item1", };
  public static SampleClass Item1_2 { get; } = new() { Number = 1, NullableDateTime = DateTimeValue, Text = "Item1", };
  public static SampleClass Item2 { get; } = new() { Number = 1, NullableDateTime = DateTimeValue, Text = "Item2", };

  public static SampleClass[] AllItems = [Item1_1, Item1_2, Item2];

  public static SampleClassComparer Comparer { get; } = new();

  public static ComparerBuilder<SampleClass> ComparerBuilder { get; } = new ComparerBuilder<SampleClass>()
    //.Add(item => item.Number, Comparer<int>.Default)
    .Add(item => item.Number, Comparer<int>.Default)
    //.Add(item => item.NullableDateTime)
    /*.Add(item => item.Text)*/;

  public static IFullComparer<SampleClass> NitoFullComparer { get; } = Nito.Comparers.ComparerBuilder.For<SampleClass>()
    .OrderBy(p => p.Number)
    .ThenBy(p => p.NullableDateTime)
    .ThenBy(p => p.Text);

  public static TestComparators<SampleClass> AllComparators { get; } = new(Comparer, Comparer, ComparerBuilder, NitoFullComparer);
}

[BenchmarkCategory(BenchmarkCategories.ReferenceType, BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal)]
public class SampleClassEqualityComparerEqualBenchmarks : EqualityComparerEqualBenchmarks<SampleClass>
{
  public SampleClassEqualityComparerEqualBenchmarks() : base(SampleClassBenchmarks.AllComparators, SampleClassBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ReferenceType, BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual)]
public class SampleClassEqualityComparerNotEqualBenchmarks : EqualityComparerNotEqualBenchmarks<SampleClass>
{
  public SampleClassEqualityComparerNotEqualBenchmarks() : base(SampleClassBenchmarks.AllComparators, SampleClassBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ReferenceType, BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode)]
public class SampleClassEqualityComparerGetHashCodeBenchmarks : EqualityComparerGetHashCodeBenchmarks<SampleClass>
{
  public SampleClassEqualityComparerGetHashCodeBenchmarks() : base(SampleClassBenchmarks.AllComparators, SampleClassBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ReferenceType, BenchmarkCategories.Comparer, BenchmarkCategories.Equal)]
public class SampleClassComparerEqualBenchmarks : ComparerEqualBenchmarks<SampleClass>
{
  public SampleClassComparerEqualBenchmarks() : base(SampleClassBenchmarks.AllComparators, SampleClassBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ReferenceType, BenchmarkCategories.Comparer, BenchmarkCategories.NotEqual)]
public class SampleClassComparerNotEqualBenchmarks : ComparerNotEqualBenchmarks<SampleClass>
{
  public SampleClassComparerNotEqualBenchmarks() : base(SampleClassBenchmarks.AllComparators, SampleClassBenchmarks.AllItems) { }
}

[BenchmarkCategory(BenchmarkCategories.ReferenceType, BenchmarkCategories.Comparer, BenchmarkCategories.Sort)]
public class SampleClassComparerSortBenchmarks : ComparerSortBenchmarks<SampleClass>
{
  public SampleClassComparerSortBenchmarks() : base(SampleClassBenchmarks.AllComparators, SampleClassBenchmarks.AllItems) { }
}
