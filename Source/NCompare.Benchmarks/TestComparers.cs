using Nito.Comparers;

namespace NCompare.Benchmarks;

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
