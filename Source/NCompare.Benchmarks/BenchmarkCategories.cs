namespace NCompare.Benchmarks;

internal static class BenchmarkCategories
{
  public const string ValueType = nameof(ValueType);
  public const string ReferenceType = nameof(ReferenceType);

  public const string EqualityComparer = nameof(EqualityComparer);
  public const string Comparer = nameof(Comparer);

  public const string Equal = nameof(Equal);
  public const string NotEqual = nameof(NotEqual);
  public new const string GetHashCode = nameof(GetHashCode);
  public const string Sort = nameof(Sort);
}
