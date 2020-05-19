using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NCompare
{
  internal static class ComparerBuilderExpression
  {
    public static ComparerBuilderExpression<T> Create<T>(LambdaExpression expression, IEqualityComparer<T> equalityComparer, IComparer<T> comparisonComparer, string filePath, int lineNumber)
      => ComparerBuilderExpression<T>.Create(expression, equalityComparer, comparisonComparer, filePath, lineNumber);

    public static ComparerBuilderExpression<T> Create<T>(LambdaExpression expression, Lazy<IEqualityComparer<T>> equalityComparer, Lazy<IComparer<T>> comparisonComparer, string filePath, int lineNumber)
      => ComparerBuilderExpression<T>.Create(expression, equalityComparer, comparisonComparer, filePath, lineNumber);

    public static ComparerBuilderExpression<T> Create<T>(LambdaExpression expression, Lazy<EqualityComparer<T>> equalityComparer, Lazy<Comparer<T>> comparisonComparer, string filePath, int lineNumber)
      => ComparerBuilderExpression<T>.Create(expression, equalityComparer, comparisonComparer, filePath, lineNumber);
  }
}
