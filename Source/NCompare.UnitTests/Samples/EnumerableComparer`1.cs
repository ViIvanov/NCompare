using System.Diagnostics.CodeAnalysis;

namespace NCompare.UnitTests.Samples;

internal sealed class EnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>, IComparer<IEnumerable<T>>
{
  public EnumerableComparer(IEqualityComparer<T> equalityComparer, IComparer<T> comparer) {
    ArgumentNullException.ThrowIfNull(equalityComparer);
    ArgumentNullException.ThrowIfNull(comparer);

    EqualityComparer = equalityComparer;
    Comparer = comparer;
  }

  public static EnumerableComparer<T> Default { get; } = new(EqualityComparer<T>.Default, Comparer<T>.Default);

  public static EnumerableComparer<T> FromBuilder(ComparerBuilder<T> builder) => new(builder.CreateEqualityComparer(), builder.CreateComparer());

  private IEqualityComparer<T> EqualityComparer { get; }
  private IComparer<T> Comparer { get; }

  public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y) => (x, y) switch {
    (null, null) => true,
    (null, var right) => !right.Any(), // Treat empty sequence equal null
    (var left, null) => !left.Any(),
    _ => x.SequenceEqual(y, EqualityComparer),
  };

  public int GetHashCode([DisallowNull] IEnumerable<T> obj) => obj?.Aggregate(new HashCode(), (hashCode, item) => {
    hashCode.Add(item, EqualityComparer);
    return hashCode;
  }, hashCode => hashCode.ToHashCode()) ?? new HashCode().ToHashCode();

  public int Compare(IEnumerable<T>? x, IEnumerable<T>? y) {
    return (x, y) switch {
      (null, null) => 0,
      (null, var right) => right.Any() ? -1 : 0,
      (var left, null) => left.Any() ? 1 : 0,
      _ => Compare(x, y),
    };

    int Compare(IEnumerable<T> x, IEnumerable<T> y) {
      using var xe = x.GetEnumerator();
      using var ye = y.GetEnumerator();
      while(xe.MoveNext()) {
        if(ye.MoveNext()) {
          if(Comparer.Compare(xe.Current, ye.Current) is not 0 and var compare) {
            return compare;
          }//if
        } else {
          return 1;
        }//if
      }//while

      return ye.MoveNext() ? -1 : 0;
    }
  }
}
