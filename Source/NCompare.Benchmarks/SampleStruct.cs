using System;
using System.Collections.Generic;

namespace NCompare.Benchmarks
{
  public readonly struct SampleStruct : IEquatable<SampleStruct>, IComparable<SampleStruct>
  {
    public SampleStruct(int number, DateTime? dateTime, string? text) {
      Number = number;
      NullableDateTime = dateTime;
      Text = text;
    }

    public int Number { get; }
    public DateTime? NullableDateTime { get; }
    public string? Text { get; }

    public bool Equals(SampleStruct other)
      => other.Number == Number && other.NullableDateTime == NullableDateTime && other.Text == Text;

    public override int GetHashCode() => HashCode.Combine(Number, NullableDateTime, Text);

    public int CompareTo(SampleStruct other) {
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

  internal sealed class SampleStructComparer : IEqualityComparer<SampleStruct>, IComparer<SampleStruct>
  {
    public bool Equals(SampleStruct x, SampleStruct y) => x.Number == y.Number && x.NullableDateTime == y.NullableDateTime && x.Text == y.Text;

    public int GetHashCode(SampleStruct obj) => HashCode.Combine(obj.Number, obj.NullableDateTime, obj.Text);

    public int Compare(SampleStruct x, SampleStruct y) {
      var compare = x.Number.CompareTo(y.Number);
      if(compare != 0) {
        return compare;
      }//if

      compare = Comparer<DateTime?>.Default.Compare(x.NullableDateTime, y.NullableDateTime);
      if(compare != 0) {
        return compare;
      }//if

      return StringComparer.Ordinal.Compare(x.Text, y.Text);
    }
  }

  internal static class SampleStructBenchmarks
  {
    private static readonly DateTime DateTimeValue = DateTime.UtcNow;
    public static SampleStruct X { get; } = new SampleStruct(number: 1, dateTime: DateTimeValue, text: "X");
    public static SampleStruct Y { get; } = new SampleStruct(number: 1, dateTime: DateTimeValue, text: "Y");
    public static SampleStruct Z { get; } = new SampleStruct(number: 1, dateTime: DateTimeValue, text: "X");
    public static SampleStructComparer Comparer { get; } = new SampleStructComparer();
    public static ComparerBuilder<SampleStruct> ComparerBuilder { get; } = new ComparerBuilder<SampleStruct>()
      .Add(item => item.Number)
      .Add(item => item.NullableDateTime)
      .Add(item => item.Text, StringComparer.Ordinal);
  }

  public class SampleStructEqualityComparerEqualBenchmarks : EqualityComparerEqualBenchmarks<SampleStruct>
  {
    public SampleStructEqualityComparerEqualBenchmarks() : base(SampleStructBenchmarks.Comparer, SampleStructBenchmarks.ComparerBuilder, SampleStructBenchmarks.X, SampleStructBenchmarks.Z, SampleStructBenchmarks.Y) { }
  }

  public class SampleStructEqualityComparerNotEqualBenchmarks : EqualityComparerNotEqualBenchmarks<SampleStruct>
  {
    public SampleStructEqualityComparerNotEqualBenchmarks() : base(SampleStructBenchmarks.Comparer, SampleStructBenchmarks.ComparerBuilder, SampleStructBenchmarks.X, SampleStructBenchmarks.Z, SampleStructBenchmarks.Y) { }
  }

  public class SampleStructEqualityComparerGetHashCodeBenchmarks : EqualityComparerGetHashCodeBenchmarks<SampleStruct>
  {
    public SampleStructEqualityComparerGetHashCodeBenchmarks() : base(SampleStructBenchmarks.Comparer, SampleStructBenchmarks.ComparerBuilder, SampleStructBenchmarks.X, SampleStructBenchmarks.Z, SampleStructBenchmarks.Y) { }
  }

  public class SampleStructComparerEqualBenchmarks : ComparerEqualBenchmarks<SampleStruct>
  {
    public SampleStructComparerEqualBenchmarks() : base(SampleStructBenchmarks.Comparer, SampleStructBenchmarks.ComparerBuilder, SampleStructBenchmarks.X, SampleStructBenchmarks.Z, SampleStructBenchmarks.Y) { }
  }

  public class SampleStructComparerNotEqualBenchmarks : ComparerNotEqualBenchmarks<SampleStruct>
  {
    public SampleStructComparerNotEqualBenchmarks() : base(SampleStructBenchmarks.Comparer, SampleStructBenchmarks.ComparerBuilder, SampleStructBenchmarks.X, SampleStructBenchmarks.Z, SampleStructBenchmarks.Y) { }
  }

  public class SampleStructComparerSortBenchmarks : ComparerSortBenchmarks<SampleStruct>
  {
    public SampleStructComparerSortBenchmarks() : base(SampleStructBenchmarks.Comparer, SampleStructBenchmarks.ComparerBuilder, SampleStructBenchmarks.X, SampleStructBenchmarks.Z, SampleStructBenchmarks.Y) { }
  }
}
