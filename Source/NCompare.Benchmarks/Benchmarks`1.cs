namespace NCompare.Benchmarks;

#pragma warning disable CA1724 // Type names should not match namespaces
public abstract class Benchmarks<T>
#pragma warning restore CA1724 // Type names should not match namespaces
{
  protected Benchmarks(TestComparators<T> comparators, params T[] values) {
    Comparators = comparators ?? throw new ArgumentNullException(nameof(comparators));
    Items = values ?? throw new ArgumentNullException(nameof(values));

    if(Items.Count < 3) {
      throw new ArgumentException($"{nameof(Items)}.{nameof(Items.Count)} < 3", nameof(values));
    }//if

    (Item1_1, Item1_2, Item2) = (Items[0], Items[1], Items[2]);
  }

  public TestComparators<T> Comparators { get; }

  public IReadOnlyList<T> Items { get; }

  public T Item1_1 { get; }
  public T Item1_2 { get; }
  public T Item2 { get; }
}
