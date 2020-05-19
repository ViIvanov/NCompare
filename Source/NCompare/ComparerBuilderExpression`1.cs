using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCompare
{
  using static ComparerBuilder;
  using static Expression;
  using static ReplaceVisitor;

  internal sealed class ComparerBuilderExpression<T> : IComparerBuilderExpression
  {
    private static readonly MethodInfo GetHashCodeMethodInfo = typeof(T).GetMethod(nameof(GetHashCode), BindingFlags.Public | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);
    private static readonly Type[] InterceptTypeArguments = { typeof(T), };

    //private static readonly ConstantExpression NullEqualityComparer = Constant(null, typeof(IEqualityComparer<T>));
    //private static readonly ConstantExpression NullComparer = Constant(null, typeof(IComparer<T>));

    private ComparerBuilderExpression(LambdaExpression expression, string filePath, int lineNumber, object? equalityComparer = null, object? comparer = null,
      Expression? equalityComparerExpression = null, Expression? comparerExpression = null) {
      Expression = expression ?? throw new ArgumentNullException(nameof(expression));

      EqualityComparer = equalityComparer;
      Comparer = comparer;

      EqualityComparerExpression = equalityComparerExpression;
      ComparerExpression = comparerExpression;

      FilePath = filePath ?? String.Empty;
      LineNumber = lineNumber;
    }

    public LambdaExpression Expression { get; }

    public object? EqualityComparer { get; }
    public object? Comparer { get; }

    public string FilePath { get; }
    public int LineNumber { get; }

    private Expression? EqualityComparerExpression { get; }
    private Expression? ComparerExpression { get; }

    #region Create Methods

    public static ComparerBuilderExpression<T> Create(LambdaExpression expression, string filePath, int lineNumber) => new ComparerBuilderExpression<T>(expression, filePath, lineNumber);

    public static ComparerBuilderExpression<T> Create(LambdaExpression expression, IEqualityComparer<T> equalityComparer, IComparer<T> comparisonComparer, string filePath, int lineNumber) {
      var equalityComparerExpression = equalityComparer != null ? Constant(equalityComparer) : null;
      var comparisonComparerExpression = comparisonComparer != null ? Constant(comparisonComparer) : null;
      return new ComparerBuilderExpression<T>(expression, filePath, lineNumber, equalityComparer, comparisonComparer, equalityComparerExpression, comparisonComparerExpression);
    }

    public static ComparerBuilderExpression<T> Create(LambdaExpression expression, Lazy<IEqualityComparer<T>> equalityComparer, Lazy<IComparer<T>> comparisonComparer, string filePath, int lineNumber) {
      var equalityComparerExpression = ExpandLazyValueExpression(equalityComparer);
      var comparisonComparerExpression = ExpandLazyValueExpression(comparisonComparer);
      return new ComparerBuilderExpression<T>(expression, filePath, lineNumber, equalityComparer, comparisonComparer, equalityComparerExpression, comparisonComparerExpression);
    }

    public static ComparerBuilderExpression<T> Create<TComparer>(LambdaExpression expression, Lazy<TComparer> comparer, string filePath, int lineNumber) where TComparer : IEqualityComparer<T>, IComparer<T> {
      var comparerExpression = ExpandLazyValueExpression(comparer);
      return new ComparerBuilderExpression<T>(expression, filePath, lineNumber, comparer, comparer, comparerExpression, comparerExpression);
    }

    public static ComparerBuilderExpression<T> Create(LambdaExpression expression, Lazy<EqualityComparer<T>> equalityComparer, Lazy<Comparer<T>> comparisonComparer, string filePath, int lineNumber) {
      var equalityComparerExpression = ExpandLazyValueExpression(equalityComparer);
      var comparisonComparerExpression = ExpandLazyValueExpression(comparisonComparer);
      return new ComparerBuilderExpression<T>(expression, filePath, lineNumber, equalityComparer, comparisonComparer, equalityComparerExpression, comparisonComparerExpression);
    }

    private static Expression? ExpandLazyValueExpression<TValue>(Lazy<TValue>? value) => value != null ? Property(Constant(value), nameof(value.Value)) : null;

    #endregion Create Methods

    #region Comparison Helpers

    private static Expression MakeEquals(Expression x, Expression y, Expression? comparer) {
      if(x is null) {
        throw new ArgumentNullException(nameof(x));
      } else if(y is null) {
        throw new ArgumentNullException(nameof(y));
      }//if

      if(comparer != null) {
        // comparer.Equals(x, y);
        return CallComparerMethod(comparer, nameof(IEqualityComparer<T>.Equals), x, y);
      } else {
        if(x.Type.IsValueType && y.Type.IsValueType) {
          // x == y;
          // TODO: Compare via IEquatable<> when operator== is not defined.
          return Equal(x, y);
        } else {
          // Object.Equals(x, y);
          return Call(ObjectEqualsMethodInfo, ToObject(x), ToObject(y));
        }//if
      }//if
    }

    private static Expression MakeGetHashCode(Expression obj, Expression? comparer) {
      if(obj is null) {
        throw new ArgumentNullException(nameof(obj));
      }//if

      if(comparer != null) {
        // comparer.GetHashCode(obj);
        return CallComparerMethod(comparer, nameof(IEqualityComparer<T>.GetHashCode), obj);
      } else {
        var method = obj.Type.IsValueType ? GetHashCodeMethodInfo : ObjectGetHashCodeMethodInfo;
        var call = Call(obj, method);
        if(obj.Type.IsValueType) {
          // obj.GetHashCode();
          return call;
        } else {
          // obj != null ? obj.GetHashCode() : 0
          return Condition(IsNotNull(obj), call, Zero);
        }//if
      }//if
    }

    private static Expression MakeCompare(Expression x, Expression y, Expression? comparer) {
      if(x is null) {
        throw new ArgumentNullException(nameof(x));
      } else if(y is null) {
        throw new ArgumentNullException(nameof(y));
      }//if

      if(comparer != null) {
        // comparer.Compare(x, y);
        return CallComparerMethod(comparer, nameof(IComparer<T>.Compare), x, y);
      } else {
        // (x < y) ? -1 : (y < x ? 1 : 0);
        var compare = Condition(LessThan(x, y), MinusOne, Condition(LessThan(y, x), One, Zero));
        return (x.IsTypeNullable(), y.IsTypeNullable()) switch
        {
          (false, false) => compare,
          (false, true) => Condition(IsNull(y), One, compare), // (object)y == null ? 1 : {compare};
          (true, false) => Condition(IsNull(x), MinusOne, compare), // (object)x == null ? -1 : {compare};
          // (object)x == null ? (y == null ? 0 : -1) : ((object)y == null ? 1 : {compare});
          (true, true) => Condition(IsNull(x), Condition(IsNull(y), Zero, MinusOne), Condition(IsNull(y), One, compare)),
        };
      }//if
    }

    private static Expression CallComparerMethod(Expression comparer, string methodName, params Expression[] arguments) {
      if(comparer is null) {
        throw new ArgumentNullException(nameof(comparer));
      }//if

      const BindingFlags MethodLookup = BindingFlags.Public | BindingFlags.Instance;
      var types = arguments != null && arguments.Any() ? Array.ConvertAll(arguments, item => item.Type) : Type.EmptyTypes;
      // TODO: Cache methods
      var method = comparer.Type.GetMethod(methodName, MethodLookup, binder: null, types, modifiers: null);
      if(method is null) {
        var message = $"Method \"{methodName}\" is not found in type \"{comparer.Type}\".";
        throw new ArgumentException(message, nameof(methodName));
      }//if

      return Call(comparer, method, arguments);
    }

    #endregion Comparison Helpers

    private Expression ApplyInterception(IComparerBuilderInterception interception, Type comparedType, string methodName,
      Expression first, Expression? second, Expression? comparer, Func<Expression, Expression, Expression?, Expression> make) {
      if(interception is null) {
        throw new ArgumentNullException(nameof(interception));
      } else if(first is null) {
        throw new ArgumentNullException(nameof(first));
      } else if(make is null) {
        throw new ArgumentNullException(nameof(make));
      }//if

      // return {interception}.{methodName}<{valueType}>({expression}, {x}[, {y}], args);

      var instance = Constant(interception);

      var firstArg = Parameter(first.Type);
      var assignFirst = Assign(firstArg, first);
      var useSecond = second != null;
      var secondArg = useSecond ? Parameter(second!.Type) : null;
      var assignSecond = useSecond ? Assign(secondArg, second) : null;
      var variables = useSecond ? new[] { firstArg, secondArg, } : new[] { firstArg, };

      var valueArg = make(firstArg, secondArg!, comparer);
      var interceptionArgs = new ComparerBuilderInterceptionArgs<T>(Expression, comparedType, EqualityComparer, Comparer, FilePath, LineNumber);
      var args = Constant(interceptionArgs);
      var arguments = useSecond
        ? new[] { valueArg, firstArg, secondArg, args, }
        : new[] { valueArg, firstArg, args, };
      var call = Call(instance, methodName, InterceptTypeArguments, arguments);
      var expressions = useSecond
        ? new Expression[] { assignFirst, assignSecond!, call, }
        : new Expression[] { assignFirst, call, };

      return Block(valueArg.Type, variables, expressions);
    }

    public override string ToString() => Expression.ToString();

    #region IComparerBuilderExpression Members

    public Expression AsEquals(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception) {
      var first = ReplaceParameters(Expression, x);
      var second = ReplaceParameters(Expression, y);
      if(interception is null) {
        return MakeEquals(first, second, EqualityComparerExpression);
      } else {
        const string MethodName = nameof(IComparerBuilderInterception.InterceptEquals);
        return ApplyInterception(interception, comparedType, MethodName, first, second, EqualityComparerExpression, make: MakeEquals);
      }//if
    }

    public Expression AsGetHashCode(ParameterExpression obj, Type comparedType, IComparerBuilderInterception? interception) {
      var value = ReplaceParameters(Expression, obj);
      if(interception is null) {
        return MakeGetHashCode(value, EqualityComparerExpression);
      } else {
        const string MethodName = nameof(IComparerBuilderInterception.InterceptGetHashCode);
        return ApplyInterception(interception, comparedType, MethodName, value, second: null, EqualityComparerExpression, make: (arg, _, comparer) => MakeGetHashCode(arg, comparer));
      }//if
    }

    public Expression AsCompare(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception) {
      var first = ReplaceParameters(Expression, x);
      var second = ReplaceParameters(Expression, y);
      if(interception is null) {
        return MakeCompare(first, second, ComparerExpression);
      } else {
        const string MethodName = nameof(IComparerBuilderInterception.InterceptCompare);
        return ApplyInterception(interception, comparedType, MethodName, first, second, ComparerExpression, make: MakeCompare);
      }//if
    }

    #endregion IComparerBuilderExpression Members
  }
}
