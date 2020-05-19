using System;
using System.Collections.Generic;

namespace NCompare.Benchmarks
{
  public abstract class Benchmarks<T>
  {
    protected Benchmarks(ComparerBuilder<T> comparerBuilder, params T[] values) {
      ComparerBuilder = comparerBuilder ?? throw new ArgumentNullException(nameof(comparerBuilder));
      Items = values ?? throw new ArgumentNullException(nameof(values));

      if(Items.Count < 3) {
        throw new ArgumentException($"{nameof(Items)}.{nameof(Items.Count)} < 3", nameof(values));
      }//if

      (X, Y, Z) = (Items[0], Items[1], Items[2]);
    }

    public ComparerBuilder<T> ComparerBuilder { get; }

    public IReadOnlyList<T> Items { get; }

    public T X { get; }
    public T Y { get; }
    public T Z { get; }
  }
}
