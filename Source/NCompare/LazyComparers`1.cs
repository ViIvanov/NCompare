using System;
using System.Collections.Generic;

namespace NCompare
{
  internal static class LazyComparers<T>
  {
    public static Lazy<EqualityComparer<T>> DefaultClassEqualityComparer { get; } = AsLazy(EqualityComparer<T>.Default);
    public static Lazy<Comparer<T>> DefaultClassComparer { get; } = AsLazy(Comparer<T>.Default);

    public static Lazy<IEqualityComparer<T>> DefaultInterfaceEqualityComparer { get; } = AsLazy<IEqualityComparer<T>>(EqualityComparer<T>.Default);
    public static Lazy<IComparer<T>> DefaultInterfaceComparer { get; } = AsLazy<IComparer<T>>(Comparer<T>.Default);

    private static Lazy<TValue> AsLazy<TValue>(TValue value) => Lazy.Create(value);
  }
}
