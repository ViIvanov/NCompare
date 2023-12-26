namespace NCompare;

internal static class Comparers
{
  public static EqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> hashCode) => new MethodEqualityComparer<T>(equals, hashCode);

  public static int RotateRight(int value, int places) {
    if((places &= 0x1F) == 0) {
      return value;
    }//if

    var mask = ~0x7FFFFFFF >> (places - 1);
    return ((value >> places) & ~mask) | ((value << (32 - places)) & mask);
  }

  [Serializable]
  private sealed class MethodEqualityComparer<T>(Func<T, T, bool> equals, Func<T, int> hashCode) : EqualityComparer<T>
  {
    private Func<T, T, bool> EqualsMethod { get; } = equals ?? throw new ArgumentNullException(nameof(equals));
    private Func<T, int> GetHashCodeMethod { get; } = hashCode ?? throw new ArgumentNullException(nameof(hashCode));

    public override bool Equals(T x, T y) => EqualsMethod(x, y);
    public override int GetHashCode(T obj) => GetHashCodeMethod(obj);

    public override bool Equals(object obj) => obj is MethodEqualityComparer<T> other
      && (other.EqualsMethod, other.GetHashCodeMethod) == (EqualsMethod, GetHashCodeMethod);

    public override int GetHashCode() => (EqualsMethod, GetHashCodeMethod).GetHashCode();
  }
}
