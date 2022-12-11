using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NCompare;

internal static class ComparerBuilderExpression
{
  public static ComparerBuilderExpression<TValue> Create<T, TValue>(Expression<Func<T, TValue>> expression, string? filePath, int lineNumber)
    => new(expression, filePath, lineNumber);

  public static ComparerBuilderExpression<T> Create<T>(LambdaExpression expression, IEqualityComparer<T>? equalityComparer, IComparer<T>? comparer, string? filePath, int lineNumber)
    => new(expression, equalityComparer, comparer, filePath, lineNumber);
}
