using System.Linq.Expressions;

namespace NCompare;

public sealed class ComparerBuilderInterceptionArgs<T>
{
  internal ComparerBuilderInterceptionArgs(LambdaExpression expression, Type comparedType, IEqualityComparer<T>? equalityComparer, IComparer<T>? comparer, string expressionText, string filePath, int lineNumber) {
    Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    ComparedType = comparedType ?? throw new ArgumentNullException(nameof(comparedType));
    EqualityComparer = equalityComparer;
    Comparer = comparer;
    ExpressionText = expressionText ?? String.Empty;
    FilePath = filePath ?? String.Empty;
    LineNumber = lineNumber;
  }

  public LambdaExpression Expression { get; }
  public Type ComparedType { get; }

  public IEqualityComparer<T>? EqualityComparer { get; }
  public IComparer<T>? Comparer { get; }

  public string ExpressionText { get; }
  public string FilePath { get; }
  public int LineNumber { get; }
}
