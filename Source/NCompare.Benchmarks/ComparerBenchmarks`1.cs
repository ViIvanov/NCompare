using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

public abstract class ComparerBenchmarks<T> : Benchmarks<T> where T : IComparable<T>
{
  protected ComparerBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }
}

public abstract class ComparerEqualBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
{
  protected ComparerEqualBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true)]
  public int Compare_Override_Equal() => Item1_1.CompareTo(Item1_2);

  [Benchmark]
  public int Compare_Comparer_Equal() => Comparers.Comparer.Compare(Item1_1, Item1_2);

  [Benchmark]
  public int Compare_Builder_Equal() => Comparers.BuilderComparer.Compare(Item1_1, Item1_2);

  [Benchmark]
  public int Compare_Nito_Equal() => Comparers.NitoFullComparer.Compare(Item1_1, Item1_2);
}

public abstract class ComparerNotEqualBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
{
  protected ComparerNotEqualBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true)]
  public int Compare_Override_NotEqual() => Item1_1.CompareTo(Item2);

  [Benchmark]
  public int Compare_Comparer_NotEqual() => Comparers.Comparer.Compare(Item1_1, Item2);

  [Benchmark]
  public int Compare_Builder_NotEqual() => Comparers.BuilderComparer.Compare(Item1_1, Item2);

  [Benchmark]
  public int Compare_Nito_NotEqual() => Comparers.NitoFullComparer.Compare(Item1_1, Item2);
}

public abstract class ComparerSortBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
{
  protected ComparerSortBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true)]
  public int Compare_Override_Sort() => Items.Skip(1).OrderBy(item => item).ToList().Count;

  [Benchmark]
  public int Compare_Comparer_Sort() => Items.Skip(1).OrderBy(item => item, Comparers.Comparer).ToList().Count;

  [Benchmark]
  public int Compare_Builder_Sort() => Items.Skip(1).OrderBy(item => item, Comparers.BuilderComparer).ToList().Count;

  [Benchmark]
  public int Compare_Nito_Sort() => Items.Skip(1).OrderBy(item => item, Comparers.NitoFullComparer).ToList().Count;
}
