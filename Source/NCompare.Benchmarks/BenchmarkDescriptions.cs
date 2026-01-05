// Ignore Spelling: Nito

namespace NCompare.Benchmarks;

internal static class BenchmarkDescriptions
{
  public const string IEquatable = $"{nameof(IEquatable<>)}<T>";
  public const string IComparable = $"{nameof(IComparable<>)}<T>";

  public const string EqualityComparer = $"{nameof(EqualityComparer<>)}<T>";
  public const string Comparer = $"{nameof(Comparer<>)}<T>";

  public const string ComparerBuilder = $"{nameof(ComparerBuilder<>)}<T>";
  public const string NitoFullComparer = $"{nameof(Nito.Comparers.IFullComparer<>)}<T>";
}
