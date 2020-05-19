using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NCompare
{
  public static class ComparerBuilderExtensions
  {
    public static ComparerBuilder<T> Add<T, TValue>(this ComparerBuilder<T> builder, Expression<Func<T, TValue>> expression,
      IComparer<TValue>? comparisonComparer, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
      => builder.Add(expression, equalityComparer: null, comparisonComparer, filePath, lineNumber);

    public static ComparerBuilder<T> Add<T, TValue>(this ComparerBuilder<T> builder, Expression<Func<T, TValue>> expression,
      Lazy<IComparer<TValue>>? comparisonComparer, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
      => builder.Add(expression, equalityComparer: null, comparisonComparer, filePath, lineNumber);

    public static ComparerBuilder<T> Add<T, TValue>(this ComparerBuilder<T> builder, Expression<Func<T, TValue>> expression,
      Lazy<Comparer<TValue>>? comparisonComparer, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
      => builder.Add(expression, equalityComparer: null, comparisonComparer, filePath, lineNumber);

    public static ComparerBuilder<T> Add<T, TValue, TComparer>(this ComparerBuilder<T> builder, Expression<Func<T, TValue>> expression,
      TComparer comparer, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) where TComparer : IEqualityComparer<TValue>, IComparer<TValue>
      => builder.Add(expression, comparer, comparer, filePath, lineNumber);
  }
}
