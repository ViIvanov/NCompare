using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace NCompare;

using static Expression;

internal static class ComparerBuilder
{
  public static ConstantExpression Null { get; } = Constant(null);
  public static ConstantExpression Zero { get; } = Constant(0);
  public static ConstantExpression False { get; } = Constant(false);
  public static ConstantExpression True { get; } = Constant(true);

  public static ConstantExpression One { get; } = Constant(1);
  public static ConstantExpression MinusOne { get; } = Constant(-1);

  public static ParameterExpression Compare { get; } = Parameter(typeof(int));
  public static IReadOnlyList<ParameterExpression> CompareVariables { get; } = new ReadOnlyCollection<ParameterExpression>(new[] { Compare, });

  public static LabelTarget LabelTargetReturn { get; } = Label(typeof(int));
  public static Expression LabelZero { get; } = Label(LabelTargetReturn, Zero);
  public static Expression ReturnZero { get; } = Return(LabelTargetReturn, Zero);
  public static Expression ReturnOne { get; } = Return(LabelTargetReturn, One);
  public static Expression ReturnMinusOne { get; } = Return(LabelTargetReturn, MinusOne);
  public static Expression ReturnCompare { get; } = Return(LabelTargetReturn, Compare);

  public static Func<int, int, int> RotateRightDelegate { get; } = Comparers.RotateRight;

  public static Expression IsNull(Expression expression) => expression switch {
    null => throw new ArgumentNullException(nameof(expression)),
    { Type.IsClass: true, } or { Type.IsInterface: true, } => ReferenceEqual(expression, Null),
    _ when Nullable.GetUnderlyingType(expression.Type) is not null => Equal(expression, Null),
    _ => False,
  };

  public static Expression IsNotNull(Expression expression) => expression switch {
    null => throw new ArgumentNullException(nameof(expression)),
    { Type.IsClass: true, } or { Type.IsInterface: true, } => ReferenceNotEqual(expression, Null),
    _ when Nullable.GetUnderlyingType(expression.Type) is not null => NotEqual(expression, Null),
    _ => True,
  };
}
