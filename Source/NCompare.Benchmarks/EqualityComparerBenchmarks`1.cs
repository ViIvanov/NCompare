using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

public abstract class EqualityComparerBenchmarks<T> : Benchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }
}

public abstract class EqualityComparerEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerEqualBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true)]
  public bool Equality_Override_Equal() => Item1_1.Equals(Item1_2);

  [Benchmark]
  public bool Equality_Comparer_Equal() => Comparers.EqualityComparer.Equals(Item1_1, Item1_2);

  [Benchmark]
  public bool Equality_Builder_Equal() => Comparers.BuilderEqualityComparer.Equals(Item1_1, Item1_2);

  [Benchmark]
  public bool Equality_Nito_Equal() => Comparers.NitoFullComparer.Equals(Item1_1, Item1_2);
}

public abstract class EqualityComparerNotEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerNotEqualBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true)]
  public bool Equality_Override_NotEqual() => Item1_1.Equals(Item2);

  [Benchmark]
  public bool Equality_Comparer_NotEqual() => Comparers.EqualityComparer.Equals(Item1_1, Item2);

  [Benchmark]
  public bool Equality_Builder_NotEqual() => Comparers.BuilderEqualityComparer.Equals(Item1_1, Item2);

  [Benchmark]
  public bool Equality_Nito_NotEqual() => Comparers.NitoFullComparer.Equals(Item1_1, Item2);
}

public abstract class EqualityComparerGetHashCodeBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerGetHashCodeBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true)]
  public int Equality_Override_GetHashCode() => Item1_1.GetHashCode();

  [Benchmark]
  public int Equality_Comparer_GetHashCode() => Comparers.EqualityComparer.GetHashCode(Item1_1);

  [Benchmark]
  public int Equality_Builder_GetHashCode() => Comparers.BuilderEqualityComparer.GetHashCode(Item1_1);

  [Benchmark]
  public int Equality_Nito_GetHashCode() => Comparers.NitoFullComparer.GetHashCode(Item1_1);
}
