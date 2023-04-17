using System;
using System.Collections.Generic;

namespace NCompare.Benchmarks;

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
