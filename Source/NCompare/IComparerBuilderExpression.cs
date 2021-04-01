using System;
using System.Linq.Expressions;

namespace NCompare
{
  internal interface IComparerBuilderExpression
  {
    Expression AsEquals(IComparerBuilderContext context, ParameterExpression x, ParameterExpression y);
    Expression AsGetHashCode(IComparerBuilderContext context, ParameterExpression obj);
    Expression AsCompare(IComparerBuilderContext context, ParameterExpression x, ParameterExpression y);
  }
}
