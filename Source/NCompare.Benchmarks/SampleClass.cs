using System;
using System.Collections.Generic;

namespace NCompare.Benchmarks
{
  public sealed class SampleClass : IEquatable<SampleClass>, IComparable<SampleClass>
  {
    public int Number { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public string? Text { get; set; }

    public bool Equals(SampleClass other)
      => other == this || other != null && other.Number == Number && other.NullableDateTime == NullableDateTime && other.Text == Text;

    public override int GetHashCode() => HashCode.Combine(Number, NullableDateTime, Text);

    public int CompareTo(SampleClass other) {
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

      return StringComparer.Ordinal.Compare(Text, other.Text);
    }
  }

  internal sealed class SampleClassComparer : IEqualityComparer<SampleClass>, IComparer<SampleClass>
  {
    public bool Equals(SampleClass x, SampleClass y) {
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

    public int GetHashCode(SampleClass obj) => obj != null ? HashCode.Combine(obj.Number, obj.NullableDateTime, obj.Text) : 0;

    public int Compare(SampleClass x, SampleClass y) {
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

        return StringComparer.Ordinal.Compare(x.Text, y.Text);
      }//if
    }
  }

  internal static class SampleClassBenchmarks
  {
    private static readonly DateTime DateTimeValue = DateTime.UtcNow;
    public static SampleClass X { get; } = new SampleClass { Number = 1, NullableDateTime = DateTimeValue, Text = "X", };
    public static SampleClass Y { get; } = new SampleClass { Number = 1, NullableDateTime = DateTimeValue, Text = "Y", };
    public static SampleClass Z { get; } = new SampleClass { Number = 1, NullableDateTime = DateTimeValue, Text = "X", };
    public static SampleClassComparer Comparer { get; } = new SampleClassComparer();
    public static ComparerBuilder<SampleClass> ComparerBuilder { get; } = new ComparerBuilder<SampleClass>()
      .Add(item => item.Number)
      .Add(item => item.NullableDateTime)
      .Add(item => item.Text, StringComparer.Ordinal);
  }

  public class SampleClassEqualityComparerEqualBenchmarks : EqualityComparerEqualBenchmarks<SampleClass>
  {
    public SampleClassEqualityComparerEqualBenchmarks() : base(SampleClassBenchmarks.Comparer, SampleClassBenchmarks.ComparerBuilder, SampleClassBenchmarks.X, SampleClassBenchmarks.Z, SampleClassBenchmarks.Y) { }
  }

  public class SampleClassEqualityComparerNotEqualBenchmarks : EqualityComparerNotEqualBenchmarks<SampleClass>
  {
    public SampleClassEqualityComparerNotEqualBenchmarks() : base(SampleClassBenchmarks.Comparer, SampleClassBenchmarks.ComparerBuilder, SampleClassBenchmarks.X, SampleClassBenchmarks.Z, SampleClassBenchmarks.Y) { }
  }

  public class SampleClassEqualityComparerGetHashCodeBenchmarks : EqualityComparerGetHashCodeBenchmarks<SampleClass>
  {
    public SampleClassEqualityComparerGetHashCodeBenchmarks() : base(SampleClassBenchmarks.Comparer, SampleClassBenchmarks.ComparerBuilder, SampleClassBenchmarks.X, SampleClassBenchmarks.Z, SampleClassBenchmarks.Y) { }
  }

  public class SampleClassComparerEqualBenchmarks : ComparerEqualBenchmarks<SampleClass>
  {
    public SampleClassComparerEqualBenchmarks() : base(SampleClassBenchmarks.Comparer, SampleClassBenchmarks.ComparerBuilder, SampleClassBenchmarks.X, SampleClassBenchmarks.Z, SampleClassBenchmarks.Y) { }
  }

  public class SampleClassComparerNotEqualBenchmarks : ComparerNotEqualBenchmarks<SampleClass>
  {
    public SampleClassComparerNotEqualBenchmarks() : base(SampleClassBenchmarks.Comparer, SampleClassBenchmarks.ComparerBuilder, SampleClassBenchmarks.X, SampleClassBenchmarks.Z, SampleClassBenchmarks.Y) { }
  }

  public class SampleClassComparerSortBenchmarks : ComparerSortBenchmarks<SampleClass>
  {
    public SampleClassComparerSortBenchmarks() : base(SampleClassBenchmarks.Comparer, SampleClassBenchmarks.ComparerBuilder, SampleClassBenchmarks.X, SampleClassBenchmarks.Z, SampleClassBenchmarks.Y) { }
  }
}
