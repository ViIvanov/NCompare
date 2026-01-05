// Ignore Spelling: Nito

using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

public abstract class ComparerBenchmarks<T>(TestComparators<T> comparators, params IReadOnlyList<T> values) : Benchmarks<T>(comparators, values) where T : IComparable<T>;

public abstract class ComparerEqualBenchmarks<T>(TestComparators<T> comparators, params IReadOnlyList<T> values) : ComparerBenchmarks<T>(comparators, values) where T : IComparable<T>
{
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

public abstract class ComparerNotEqualBenchmarks<T>(TestComparators<T> comparators, params IReadOnlyList<T> values) : ComparerBenchmarks<T>(comparators, values) where T : IComparable<T>
{
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

public abstract class ComparerSortBenchmarks<T>(TestComparators<T> comparators, params IReadOnlyList<T> values) : ComparerBenchmarks<T>(comparators, DuplicateAndShuffleValues(values)) where T : IComparable<T>
{
  private static T[] DuplicateAndShuffleValues(IReadOnlyCollection<T> values) {
    const int Factor = 10;
    var duplicates = Enumerable.Range(0, Factor).SelectMany(_ => values);

#if NET10_0_OR_GREATER
    return [.. duplicates.Shuffle()];
#else
    return Shuffle(duplicates);

    static T[] Shuffle(IEnumerable<T> values) {
  #if NET6_0_OR_GREATER
      var random = Random.Shared;
  #else
      var random = new Random();
  #endif //NET6_0_OR_GREATER
      var array = values.ToArray();
      var before = array.Length;
      while(before > 1) {
        var after = random.Next(before--);
        (array[after], array[before]) = (array[before], array[after]);
      }//while
      return array;
    }
#endif //NET10_0_OR_GREATER
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
