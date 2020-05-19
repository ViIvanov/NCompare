using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks
{
  public abstract class ComparerBenchmarks<T> : Benchmarks<T> where T : IComparable<T>
  {
    protected ComparerBenchmarks(IComparer<T> comparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(comparerBuilder, values) {
      Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
      BuilderComparer = ComparerBuilder.CreateComparer();
    }

    public IComparer<T> Comparer { get; }

    public Comparer<T> BuilderComparer { get; }
  }

  public abstract class ComparerEqualBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
  {
    protected ComparerEqualBenchmarks(IComparer<T> comparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(comparer, comparerBuilder, values) { }

    [Benchmark(Baseline = true)]
    public int Compare_Override_Equal() => X.CompareTo(Y);

    [Benchmark]
    public int Compare_Comparer_Equal() => Comparer.Compare(X, Y);

    [Benchmark]
    public int Compare_Builder_Equal() => BuilderComparer.Compare(X, Y);
  }

  public abstract class ComparerNotEqualBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
  {
    protected ComparerNotEqualBenchmarks(IComparer<T> comparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(comparer, comparerBuilder, values) { }

    [Benchmark(Baseline = true)]
    public int Compare_Override_NotEqual() => X.CompareTo(Z);

    [Benchmark]
    public int Compare_Comparer_NotEqual() => Comparer.Compare(X, Z);

    [Benchmark]
    public int Compare_Builder_NotEqual() => BuilderComparer.Compare(X, Z);
  }

  public abstract class ComparerSortBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
  {
    protected ComparerSortBenchmarks(IComparer<T> comparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(comparer, comparerBuilder, values) { }

    [Benchmark(Baseline = true)]
    public int Compare_Override_Sort() => Items.Skip(1).OrderBy(item => item).ToList().Count;

    [Benchmark]
    public int Compare_Comparer_Sort() => Items.Skip(1).OrderBy(item => item, Comparer).ToList().Count;

    [Benchmark]
    public int Compare_Builder_Sort() => Items.Skip(1).OrderBy(item => item, BuilderComparer).ToList().Count;
  }
}
