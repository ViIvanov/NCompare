using System;
using System.Linq.Expressions;

namespace NCompare
{
  using static ReplaceVisitor;

  internal sealed class NestedComparerBuilderExpression<T> : IComparerBuilderExpression
  {
    public NestedComparerBuilderExpression(LambdaExpression expression, ComparerBuilder<T> builder) {
      Expression = expression ?? throw new ArgumentNullException(nameof(expression));
      Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public LambdaExpression Expression { get; }
    public ComparerBuilder<T> Builder { get; }

    public override string ToString() => Expression.ToString();

    #region IComparerExpression Members

    public Expression AsEquals(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null) {
      var expression = Builder.BuildEquals(ComparerBuilder<T>.X, ComparerBuilder<T>.Y, comparedType, interception);
      var first = ReplaceParameters(Expression, x);
      var second = ReplaceParameters(Expression, y);
      return ReplaceParameters(expression, first, second);
    }

    public Expression AsGetHashCode(ParameterExpression obj, Type comparedType, IComparerBuilderInterception? interception = null) {
      var expression = Builder.BuildGetHashCode(ComparerBuilder<T>.Obj, comparedType, interception);
      var value = ReplaceParameters(Expression, obj);
      return ReplaceParameters(expression, value);
    }

    public Expression AsCompare(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null) {
      var expression = Builder.BuildCompare(ComparerBuilder<T>.X, ComparerBuilder<T>.Y, comparedType, interception);
      var first = ReplaceParameters(Expression, x);
      var second = ReplaceParameters(Expression, y);
      return ReplaceParameters(expression, first, second);
    }

    #endregion IComparerExpression Members
  }
}
