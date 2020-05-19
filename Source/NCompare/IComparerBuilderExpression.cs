using System;
using System.Linq.Expressions;

namespace NCompare
{
  internal interface IComparerBuilderExpression
  {
    Expression AsEquals(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null);
    Expression AsGetHashCode(ParameterExpression obj, Type comparedType, IComparerBuilderInterception? interception = null);
    Expression AsCompare(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null);
  }
}
