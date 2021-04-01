using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NCompare
{
  public sealed class ComparerExpressionContext<T>
  {
    internal ComparerExpressionContext(LambdaExpression expression, Type comparedType,
      IEqualityComparer<T> equalityComparer, IComparer<T>? comparer,
      string? filePath, int lineNumber) {
      ComparedType = comparedType ?? throw new ArgumentNullException(nameof(comparedType));
      Expression = expression ?? throw new ArgumentNullException(nameof(expression));
      EqualityComparer = equalityComparer;
      Comparer = comparer;
      FilePath = filePath ?? String.Empty;
      LineNumber = lineNumber;
    }

    public LambdaExpression Expression { get; }
    public Type ComparedType { get; }

    public IEqualityComparer<T>? EqualityComparer { get; }
    public IComparer<T>? Comparer { get; }

    public string FilePath { get; }
    public int LineNumber { get; }
  }
}
