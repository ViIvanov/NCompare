using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NCompare
{
  public sealed class ComparerBuilderInterceptionArgs<T>
  {
    internal ComparerBuilderInterceptionArgs(LambdaExpression expression, Type comparedType,
      object? equalityComparer, object? comparer, string filePath, int lineNumber) {
      Expression = expression ?? throw new ArgumentNullException(nameof(expression));
      ComparedType = comparedType ?? throw new ArgumentNullException(nameof(comparedType));
      EqualityComparer = equalityComparer;
      Comparer = comparer;
      FilePath = filePath ?? String.Empty;
      LineNumber = lineNumber;
    }

    public LambdaExpression Expression { get; }
    public Type ComparedType { get; }

    public object? EqualityComparer { get; }
    public object? Comparer { get; }

    public string FilePath { get; }
    public int LineNumber { get; }

    public bool Equals(T x, T y) => EqualityComparer switch {
      IEqualityComparer<T> comparer => comparer.Equals(x, y),
      Lazy<IEqualityComparer<T>> lazyComparer => lazyComparer.Value.Equals(x, y),
      Lazy<EqualityComparer<T>> lazyComparer => lazyComparer.Value.Equals(x, y),
      _ => EqualityComparer<T>.Default.Equals(x, y),
    };

    public int GetHashCode(T obj) => EqualityComparer switch {
      IEqualityComparer<T> comparer => comparer.GetHashCode(obj),
      Lazy<IEqualityComparer<T>> lazyComparer => lazyComparer.Value.GetHashCode(obj),
      Lazy<EqualityComparer<T>> lazyComparer => lazyComparer.Value.GetHashCode(obj),
      _ => EqualityComparer<T>.Default.GetHashCode(obj),
    };

    public int Compare(T x, T y) => Comparer switch {
      IComparer<T> comparer => comparer.Compare(x, y),
      Lazy<IComparer<T>> lazyComparer => lazyComparer.Value.Compare(x, y),
      Lazy<Comparer<T>> lazyComparer => lazyComparer.Value.Compare(x, y),
      _ => Comparer<T>.Default.Compare(x, y),
    };
  }
}
