using System;
using System.Collections.Generic;

using Nito.Comparers;

namespace NCompare.Benchmarks
{
  public abstract class Benchmarks<T>
  {
    protected Benchmarks(TestComparers<T> comparers, params T[] values) {
      Comparers = comparers ?? throw new ArgumentNullException(nameof(comparers));
      Items = values ?? throw new ArgumentNullException(nameof(values));

      if(Items.Count < 3) {
        throw new ArgumentException($"{nameof(Items)}.{nameof(Items.Count)} < 3", nameof(values));
      }//if

      (Item1_1, Item1_2, Item2) = (Items[0], Items[1], Items[2]);
    }

    public TestComparers<T> Comparers { get; }

    public IReadOnlyList<T> Items { get; }

    public T Item1_1 { get; }
    public T Item1_2 { get; }
    public T Item2 { get; }
  }

  public sealed class TestComparers<T>
  {
    public TestComparers(IEqualityComparer<T> equalityComparer, IComparer<T> comparer, ComparerBuilder<T> comparerBuilder, IFullComparer<T> nitoFullComparer) {
      EqualityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
      Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

      ComparerBuilder = comparerBuilder ?? throw new ArgumentNullException(nameof(comparerBuilder));
      BuilderEqualityComparer = ComparerBuilder.CreateEqualityComparer();
      BuilderComparer = ComparerBuilder.CreateComparer();

      NitoFullComparer = nitoFullComparer ?? throw new ArgumentNullException(nameof(nitoFullComparer));
    }

    public IEqualityComparer<T> EqualityComparer { get; }
    public IComparer<T> Comparer { get; }

    public ComparerBuilder<T> ComparerBuilder { get; }
    public IEqualityComparer<T> BuilderEqualityComparer { get; }
    public IComparer<T> BuilderComparer { get; }

    public IFullComparer<T> NitoFullComparer { get; }
  }
}
