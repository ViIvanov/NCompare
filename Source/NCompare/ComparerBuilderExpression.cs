using System.Linq.Expressions;

namespace NCompare;

internal static class ComparerBuilderExpression
{
  public static ComparerBuilderExpression<TValue> Create<T, TValue>(Expression<Func<T, TValue>> expression, string? expressionText, string? filePath, int lineNumber)
    => new(expression, expressionText, filePath, lineNumber);

  public static ComparerBuilderExpression<T> Create<T>(LambdaExpression expression, IEqualityComparer<T>? equalityComparer, IComparer<T>? comparer, string? expressionText, string? filePath, int lineNumber)
    => new(expression, equalityComparer, comparer, expressionText, filePath, lineNumber);
}
