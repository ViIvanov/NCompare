using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NCompare.UnitTests.Samples;

internal sealed class CollectionComparer<T> : IEqualityComparer<IReadOnlyCollection<T>>, IComparer<IReadOnlyCollection<T>>
{
  public CollectionComparer(IEqualityComparer<T> equalityComparer, IComparer<T> comparer) {
    ArgumentNullException.ThrowIfNull(equalityComparer);
    ArgumentNullException.ThrowIfNull(comparer);

    EqualityComparer = equalityComparer;
    Comparer = comparer;
  }

  public static CollectionComparer<T> Default { get; } = new(EqualityComparer<T>.Default, Comparer<T>.Default);

  public static CollectionComparer<T> FromBuilder(ComparerBuilder<T> builder) => new(builder.CreateEqualityComparer(), builder.CreateComparer());

  private IEqualityComparer<T> EqualityComparer { get; }
  private IComparer<T> Comparer { get; }

  public bool Equals(IReadOnlyCollection<T>? x, IReadOnlyCollection<T>? y) => (x, y) switch {
    (null, null) => true,
    (null, var right) => right.Count is 0,
    (var left, null) => left.Count is 0,
    _ => x.Count == y.Count && x.SequenceEqual(y, EqualityComparer),
  };

  public int GetHashCode([DisallowNull] IReadOnlyCollection<T> obj) {
    var hashCode = new HashCode();
    if(obj is null) {
      hashCode.Add(0);
    } else {
      hashCode.Add(obj.Count);
      foreach(var item in obj) {
        hashCode.Add(item, EqualityComparer);
      }//for
    }//if
    return hashCode.GetHashCode();
  }

  public int Compare(IReadOnlyCollection<T>? x, IReadOnlyCollection<T>? y) {
    return (x, y) switch {
      (null, null) => 0,
      (null, var right) => right.Count is 0 ? 0 : -1,
      (var left, null) => left.Count is 0 ? 0 : 1,
      _ => (x.Count - y.Count) switch {
        0 => Compare(x, y),
        var diff => diff,
      },
    };

    int Compare(IReadOnlyCollection<T> x, IReadOnlyCollection<T> y) {
      using var xe = x.GetEnumerator();
      using var ye = y.GetEnumerator();
      while(xe.MoveNext()) {
        ye.MoveNext();
        if(Comparer.Compare(xe.Current, ye.Current) is not 0 and var compare) {
          return compare;
        }//if
      }//while

      return 0;
    }
  }
}
