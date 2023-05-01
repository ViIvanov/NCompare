using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

public abstract class EqualityComparerBenchmarks<T> : Benchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }
}

public abstract class EqualityComparerEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerEqualBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IEquatable)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.IEquatable)]
  public bool Equality_Override_Equal() => Item1_1.Equals(Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.EqualityComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.EqualityComparer)]
  public bool Equality_Comparer_Equal() => Comparers.EqualityComparer.Equals(Item1_1, Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.ComparerBuilder)]
  public bool Equality_Builder_Equal() => Comparers.BuilderEqualityComparer.Equals(Item1_1, Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.NitoComparers)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.NitoComparers)]
  public bool Equality_Nito_Equal() => Comparers.NitoFullComparer.Equals(Item1_1, Item1_2);
}

public abstract class EqualityComparerNotEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerNotEqualBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IEquatable)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.IEquatable)]
  public bool Equality_Override_NotEqual() => Item1_1.Equals(Item2);

  [Benchmark(Description = BenchmarkDescriptions.EqualityComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.EqualityComparer)]
  public bool Equality_Comparer_NotEqual() => Comparers.EqualityComparer.Equals(Item1_1, Item2);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.ComparerBuilder)]
  public bool Equality_Builder_NotEqual() => Comparers.BuilderEqualityComparer.Equals(Item1_1, Item2);

  [Benchmark(Description = BenchmarkDescriptions.NitoComparers)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.NitoComparers)]
  public bool Equality_Nito_NotEqual() => Comparers.NitoFullComparer.Equals(Item1_1, Item2);
}

public abstract class EqualityComparerGetHashCodeBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerGetHashCodeBenchmarks(TestComparers<T> comparers, params T[] values) : base(comparers, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IEquatable)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.IEquatable)]
  public int Equality_Override_GetHashCode() => Item1_1.GetHashCode();

  [Benchmark(Description = BenchmarkDescriptions.EqualityComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.EqualityComparer)]
  public int Equality_Comparer_GetHashCode() => Comparers.EqualityComparer.GetHashCode(Item1_1);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.ComparerBuilder)]
  public int Equality_Builder_GetHashCode() => Comparers.BuilderEqualityComparer.GetHashCode(Item1_1);

  [Benchmark(Description = BenchmarkDescriptions.NitoComparers)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.NitoComparers)]
  public int Equality_Nito_GetHashCode() => Comparers.NitoFullComparer.GetHashCode(Item1_1);
}
