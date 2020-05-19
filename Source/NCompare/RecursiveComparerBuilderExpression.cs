using System;
using System.Linq.Expressions;

namespace NCompare
{
  internal sealed class RecursiveComparerBuilderExpression<T> : IComparerBuilderExpression
  {
    public RecursiveComparerBuilderExpression(LambdaExpression expression, ComparerBuilder<T> builder) {
      Expression = expression ?? throw new ArgumentNullException(nameof(expression));
      Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public LambdaExpression Expression { get; }
    public ComparerBuilder<T> Builder { get; }

    public override string ToString() => Expression.ToString();

    public Expression AsEquals(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null) {
      throw new NotImplementedException();
    }

    public Expression AsGetHashCode(ParameterExpression obj, Type comparedType, IComparerBuilderInterception? interception = null) {
      throw new NotImplementedException();
    }

    public Expression AsCompare(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null) {
      throw new NotImplementedException();
    }
  }
}
