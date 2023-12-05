// Ignore Spelling: Nito

using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

public abstract class ComparerBenchmarks<T> : Benchmarks<T> where T : IComparable<T>
{
  protected ComparerBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }
}

public abstract class ComparerEqualBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
{
  protected ComparerEqualBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IComparable)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Equal, BenchmarkDescriptions.IComparable)]
  public int Compare_Override_Equal() => Item1_1.CompareTo(Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.Comparer)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Equal, BenchmarkDescriptions.Comparer)]
  public int Compare_Comparer_Equal() => Comparators.Comparer.Compare(Item1_1, Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Equal, BenchmarkDescriptions.ComparerBuilder)]
  public int Compare_Builder_Equal() => Comparators.BuilderComparer.Compare(Item1_1, Item1_2);

  [Benchmark(Description = BenchmarkDescriptions.NitoFullComparer)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Equal, BenchmarkDescriptions.NitoFullComparer)]
  public int Compare_Nito_Equal() => Comparators.NitoFullComparer.Compare(Item1_1, Item1_2);
}

public abstract class ComparerNotEqualBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
{
  protected ComparerNotEqualBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, values) { }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IComparable)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.IComparable)]
  public int Compare_Override_NotEqual() => Item1_1.CompareTo(Item2);

  [Benchmark(Description = BenchmarkDescriptions.Comparer)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.Comparer)]
  public int Compare_Comparer_NotEqual() => Comparators.Comparer.Compare(Item1_1, Item2);

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.ComparerBuilder)]
  public int Compare_Builder_NotEqual() => Comparators.BuilderComparer.Compare(Item1_1, Item2);

  [Benchmark(Description = BenchmarkDescriptions.NitoFullComparer)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.NotEqual, BenchmarkDescriptions.NitoFullComparer)]
  public int Compare_Nito_NotEqual() => Comparators.NitoFullComparer.Compare(Item1_1, Item2);
}

public abstract class ComparerSortBenchmarks<T> : ComparerBenchmarks<T> where T : IComparable<T>
{
  protected ComparerSortBenchmarks(TestComparators<T> comparators, params T[] values) : base(comparators, DuplicateAndShuffleValues(values)) { }

  private static T[] DuplicateAndShuffleValues(T[] values) {
    const int Factor = 10;
    var array = Enumerable.Range(0, Factor).SelectMany(_ => values).ToArray();
    Shuffle(array);
    return array;

    static void Shuffle(T[] array) {
#if NET6_0_OR_GREATER
      var random = Random.Shared;
#else
      var random = new Random();
#endif //NET6_0_OR_GREATER
      var before = array.Length;
      while(before > 1) {
        var after = random.Next(before--);
        (array[after], array[before]) = (array[before], array[after]);
      }//while
    }
  }

  [Benchmark(Baseline = true, Description = BenchmarkDescriptions.IComparable)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Sort, BenchmarkDescriptions.IComparable)]
  public int Compare_Override_Sort() => Items.OrderBy(item => item).ToList().Count;

  [Benchmark(Description = BenchmarkDescriptions.Comparer)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Sort, BenchmarkDescriptions.Comparer)]
  public int Compare_Comparer_Sort() => Items.OrderBy(item => item, Comparators.Comparer).ToList().Count;

  [Benchmark(Description = BenchmarkDescriptions.ComparerBuilder)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Sort, BenchmarkDescriptions.ComparerBuilder)]
  public int Compare_Builder_Sort() => Items.OrderBy(item => item, Comparators.BuilderComparer).ToList().Count;

  [Benchmark(Description = BenchmarkDescriptions.NitoFullComparer)]
  [BenchmarkCategory(BenchmarkCategories.Comparer, BenchmarkCategories.Sort, BenchmarkDescriptions.NitoFullComparer)]
  public int Compare_Nito_Sort() => Items.OrderBy(item => item, Comparators.NitoFullComparer).ToList().Count;
}
