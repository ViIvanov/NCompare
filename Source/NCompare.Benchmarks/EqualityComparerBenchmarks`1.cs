using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks
{
  public abstract class EqualityComparerBenchmarks<T> : Benchmarks<T> where T : IEquatable<T>
  {
    protected EqualityComparerBenchmarks(IEqualityComparer<T> equalityComparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(comparerBuilder, values) {
      EqualityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
      BuilderEqualityComparer = ComparerBuilder.CreateEqualityComparer();
    }

    public IEqualityComparer<T> EqualityComparer { get; }

    public EqualityComparer<T> BuilderEqualityComparer { get; }
  }

  public abstract class EqualityComparerEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
  {
    protected EqualityComparerEqualBenchmarks(IEqualityComparer<T> equalityComparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(equalityComparer, comparerBuilder, values) { }

    [Benchmark(Baseline = true)]
    public bool Equality_Override_Equal() => X.Equals(Y);

    [Benchmark]
    public bool Equality_Comparer_Equal() => EqualityComparer.Equals(X, Y);

    [Benchmark]
    public bool Equality_Builder_Equal() => BuilderEqualityComparer.Equals(X, Y);
  }

  public abstract class EqualityComparerNotEqualBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
  {
    protected EqualityComparerNotEqualBenchmarks(IEqualityComparer<T> equalityComparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(equalityComparer, comparerBuilder, values) { }

    [Benchmark(Baseline = true)]
    public bool Equality_Override_NotEqual() => X.Equals(Z);

    [Benchmark]
    public bool Equality_Comparer_NotEqual() => EqualityComparer.Equals(X, Z);

    [Benchmark]
    public bool Equality_Builder_NotEqual() => BuilderEqualityComparer.Equals(X, Z);
  }

  public abstract class EqualityComparerGetHashCodeBenchmarks<T> : EqualityComparerBenchmarks<T> where T : IEquatable<T>
  {
    protected EqualityComparerGetHashCodeBenchmarks(IEqualityComparer<T> equalityComparer, ComparerBuilder<T> comparerBuilder, params T[] values)
      : base(equalityComparer, comparerBuilder, values) { }

    [Benchmark(Baseline = true)]
    public int Equality_Override_GetHashCode() => X.GetHashCode();

    [Benchmark]
    public int Equality_Comparer_GetHashCode() => EqualityComparer.GetHashCode(X);

    [Benchmark]
    public int Equality_Builder_GetHashCode() => BuilderEqualityComparer.GetHashCode(X);
  }
}
