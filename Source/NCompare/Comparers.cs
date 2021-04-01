using System;
using System.Collections.Generic;

namespace NCompare
{
  public static class Comparers
  {
    public static EqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> hashCode) => new MethodEqualityComparer<T>(equals, hashCode);

    public static Comparer<T> Create<T>(Func<T, T, int> compare)
      => compare is not null ? Comparer<T>.Create((x, y) => compare(x, y)) : throw new ArgumentNullException(nameof(compare));

    public static int RotateRight(int value, int places) {
      if((places &= 0x1F) == 0) {
        return value;
      }//if

      var mask = ~0x7FFFFFFF >> (places - 1);
      return ((value >> places) & ~mask) | ((value << (32 - places)) & mask);
    }

    [Serializable]
    private sealed class MethodEqualityComparer<T> : EqualityComparer<T>
    {
      public MethodEqualityComparer(Func<T, T, bool> equals, Func<T, int> hashCode) {
        EqualsMethod = equals ?? throw new ArgumentNullException(nameof(equals));
        GetHashCodeMethod = hashCode ?? throw new ArgumentNullException(nameof(hashCode));
      }

      private Func<T, T, bool> EqualsMethod { get; }
      private Func<T, int> GetHashCodeMethod { get; }

      public override bool Equals(T x, T y) => EqualsMethod(x, y);
      public override int GetHashCode(T obj) => GetHashCodeMethod(obj);

      public override bool Equals(object obj) => obj is MethodEqualityComparer<T> other
        && other.EqualsMethod == EqualsMethod && other.GetHashCodeMethod == GetHashCodeMethod;

      public override int GetHashCode() => (EqualsMethod, GetHashCodeMethod).GetHashCode();
    }
  }
}
