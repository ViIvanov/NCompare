// Ignore Spelling: Nito

using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

public abstract class EqualityComparerBenchmarks<T> : Benchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }
}

public abstract class EqualityComparerEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerEqualBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IEquatable)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.IEquatable)]
  public bool Equality_Override_Equal() => Item1_1.Equals(Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.EqualityComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.EqualityComparer)]
  public bool Equality_Comparer_Equal() => Comparators.EqualityComparer.Equals(Item1_1, Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.ComparerBuilder)]
  public bool Equality_Builder_Equal() => Comparators.BuilderEqualityComparer.Equals(Item1_1, Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.NitoFullComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.Equal, BenchmarkDescriptions.NitoFullComparer)]
  public bool Equality_Nito_Equal() => Comparators.NitoFullComparer.Equals(Item1_1, Item1_2);
}

public abstract class EqualityComparerNotEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerNotEqualBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IEquatable)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.IEquatable)]
  public bool Equality_Override_NotEqual() => Item1_1.Equals(Item2);

  [Benchmark(Description = BenchmarkDescriptions.EqualityComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.EqualityComparer)]
  public bool Equality_Comparer_NotEqual() => Comparators.EqualityComparer.Equals(Item1_1, Item2);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.ComparerBuilder)]
  public bool Equality_Builder_NotEqual() => Comparators.BuilderEqualityComparer.Equals(Item1_1, Item2);

  [Benchmark(Description = BenchmarkDescriptions.NitoFullComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.NitoFullComparer)]
  public bool Equality_Nito_NotEqual() => Comparators.NitoFullComparer.Equals(Item1_1, Item2);
}

public abstract class EqualityComparerGetHashCodeBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
{
  protected EqualityComparerGetHashCodeBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IEquatable)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.IEquatable)]
  public int Equality_Override_GetHashCode() => Item1_1.GetHashCode();

  [Benchmark(Description = BenchmarkDescriptions.EqualityComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.EqualityComparer)]
  public int Equality_Comparer_GetHashCode() => Comparators.EqualityComparer.GetHashCode(Item1_1);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.ComparerBuilder)]
  public int Equality_Builder_GetHashCode() => Comparators.BuilderEqualityComparer.GetHashCode(Item1_1);

  [Benchmark(Description = BenchmarkDescriptions.NitoFullComparer)]
  [BenchmarkCategory(BenchmarkCategories.EqualityComparer, BenchmarkCategories.GetHashCode, BenchmarkDescriptions.NitoFullComparer)]
  public int Equality_Nito_GetHashCode() => Comparators.NitoFullComparer.GetHashCode(Item1_1);
}
