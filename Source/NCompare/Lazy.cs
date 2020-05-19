using System;
using System.Threading;

namespace NCompare
{
  internal static class Lazy
  {
    //public const string ValueName = nameof(Lazy<object>.Value);

    #region Create(…)

    public static Lazy<T> Create<T>(Func<T> valueFactory) => new Lazy<T>(valueFactory);
    public static Lazy<T> Create<T>(Func<T> valueFactory, bool isThreadSafe) => new Lazy<T>(valueFactory, isThreadSafe);
    public static Lazy<T> Create<T>(Func<T> valueFactory, LazyThreadSafetyMode mode) => new Lazy<T>(valueFactory, mode);

    public static Lazy<T> Create<T>(T value) => new Lazy<T>(() => value, isThreadSafe: false);

    #endregion Create(…)
  }
}
