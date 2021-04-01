using System;
using System.Collections.Generic;

using Nito.Comparers;

namespace NCompare.Benchmarks
{
  public sealed class SampleClass : IEquatable<SampleClass>, IComparable<SampleClass>
  {
    public int Number { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public string? Text { get; set; }

    public bool Equals(SampleClass? other)
      => other == this || other is not null && other.Number == Number && other.NullableDateTime == NullableDateTime && other.Text == Text;

    public override int GetHashCode() => HashCode.Combine(Number, NullableDateTime, Text);

    public int CompareTo(SampleClass? other) {
      if(other is null) {
        return 1;
      }//if

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

    public int GetHashCode(SampleClass? obj) => obj is not null ? HashCode.Combine(obj.Number, obj.NullableDateTime, obj.Text) : 0;

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

        return String.Compare(x.Text, y.Text);
      }//if
    }
  }

  internal static class SampleClassBenchmarks
  {
    private static readonly DateTime DateTimeValue = DateTime.UtcNow;
    public static SampleClass Item1_1 { get; } = new() { Number = 1, NullableDateTime = DateTimeValue, Text = "Item1", };
    public static SampleClass Item1_2 { get; } = new() { Number = 1, NullableDateTime = DateTimeValue, Text = "Item1", };
    public static SampleClass Item2 { get; } = new() { Number = 1, NullableDateTime = DateTimeValue, Text = "Item2", };

    public static SampleClass[] AllItems = { Item1_1, Item1_2, Item2, };

    public static SampleClassComparer Comparer { get; } = new();

    public static ComparerBuilder<SampleClass> ComparerBuilder { get; } = new ComparerBuilder<SampleClass>()
      .Add(item => item.Number)
      .Add(item => item.NullableDateTime)
      .Add(item => item.Text);

    public static IFullComparer<SampleClass> NitoFullComparer { get; } = Nito.Comparers.ComparerBuilder.For<SampleClass>()
      .OrderBy(p => p.Number)
      .ThenBy(p => p.NullableDateTime)
      .ThenBy(p => p.Text);

    public static TestComparers<SampleClass> AllComparers { get; } = new(Comparer, Comparer, ComparerBuilder, NitoFullComparer);
  }

  public class SampleClassEqualityComparerEqualBenchmarks : EqualityComparerEqualBenchmarks<SampleClass>
  {
    public SampleClassEqualityComparerEqualBenchmarks() : base(SampleClassBenchmarks.AllComparers, SampleClassBenchmarks.AllItems) { }
  }

  public class SampleClassEqualityComparerNotEqualBenchmarks : EqualityComparerNotEqualBenchmarks<SampleClass>
  {
    public SampleClassEqualityComparerNotEqualBenchmarks() : base(SampleClassBenchmarks.AllComparers, SampleClassBenchmarks.AllItems) { }
  }

  public class SampleClassEqualityComparerGetHashCodeBenchmarks : EqualityComparerGetHashCodeBenchmarks<SampleClass>
  {
    public SampleClassEqualityComparerGetHashCodeBenchmarks() : base(SampleClassBenchmarks.AllComparers, SampleClassBenchmarks.AllItems) { }
  }

  public class SampleClassComparerEqualBenchmarks : ComparerEqualBenchmarks<SampleClass>
  {
    public SampleClassComparerEqualBenchmarks() : base(SampleClassBenchmarks.AllComparers, SampleClassBenchmarks.AllItems) { }
  }

  public class SampleClassComparerNotEqualBenchmarks : ComparerNotEqualBenchmarks<SampleClass>
  {
    public SampleClassComparerNotEqualBenchmarks() : base(SampleClassBenchmarks.AllComparers, SampleClassBenchmarks.AllItems) { }
  }

  public class SampleClassComparerSortBenchmarks : ComparerSortBenchmarks<SampleClass>
  {
    public SampleClassComparerSortBenchmarks() : base(SampleClassBenchmarks.AllComparers, SampleClassBenchmarks.AllItems) { }
  }
}
