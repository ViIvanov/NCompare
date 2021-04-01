using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NCompare
{
  using static Expression;

  internal static class ComparerBuilder
  {
    public static ConstantExpression Null { get; } = Constant(null);
    public static ConstantExpression Zero { get; } = Constant(0);
    public static ConstantExpression False { get; } = Constant(false);

    public static ConstantExpression One { get; } = Constant(1);
    public static ConstantExpression MinusOne { get; } = Constant(-1);

    public static ParameterExpression Compare { get; } = Parameter(typeof(int));
    public static IReadOnlyList<ParameterExpression> CompareVariables { get; } = new ReadOnlyCollection<ParameterExpression>(new[] { Compare, });

    public static Func<int, int, int> RotateRightDelegate { get; } = Comparers.RotateRight;

    public static MethodInfo ObjectEqualsMethodInfo { get; } = new Func<object, object, bool>(Equals).Method;
    public static MethodInfo ObjectGetHashCodeMethodInfo { get; } = new Func<int>(new object().GetHashCode).Method;

    public static BinaryExpression IsNull(Expression expression) {
      if(expression is null) {
        throw new ArgumentNullException(nameof(expression));
      }//if

      if(expression.IsTypeByReference()) {
        return ReferenceEqual(expression, Null);
      } else { // Nullable value type
        return Equal(expression, Null);
      }//if
    }

    public static BinaryExpression IsNotNull(Expression expression) {
      if(expression is null) {
        throw new ArgumentNullException(nameof(expression));
      }//if

      if(expression.IsTypeByReference()) {
        return ReferenceNotEqual(expression, Null);
      } else { // Nullable value type
        return NotEqual(expression, Null);
      }//if
    }

    public static Expression ToObject(Expression expression)
      => expression is not null
        ? expression.IsTypeByReference() ? expression : Convert(expression, typeof(object))
        : throw new ArgumentNullException(nameof(expression));

    public static bool IsTypeByReference(this Expression expression)
      => expression is not null
        ? IsTypeByReference(expression.Type)
        : throw new ArgumentNullException(nameof(expression));

    public static bool IsTypeByReference(this Type type)
      => type is not null
        ? type.IsClass || type.IsInterface
        : throw new ArgumentNullException(nameof(type));

    public static bool IsTypeNullable(this Expression expression) {
      if(expression is null) {
        throw new ArgumentNullException(nameof(expression));
      }//if

      var type = expression.Type;
      return type.IsTypeByReference() || type.IsValueType && Nullable.GetUnderlyingType(type) is not null;
    }

    public static bool IsTypeNullable(this Type type) {
      if(type is null) {
        throw new ArgumentNullException(nameof(type));
      }//if

      return type.IsTypeByReference() || type.IsValueType && Nullable.GetUnderlyingType(type) is not null;
    }
  }
}
