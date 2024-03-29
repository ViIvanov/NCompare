﻿using System.Linq.Expressions;
using System.Reflection;

namespace NCompare;

using static Expression;
using static ReplaceVisitor;

internal sealed class ComparerBuilderExpression<T>(LambdaExpression expression, string? expressionText, string? filePath, int lineNumber) : IComparerBuilderExpression
{
  private static readonly MethodInfo EqualityComparerEqualsMethod = GetComparerMethodInfo(typeof(IEqualityComparer<T>), nameof(IEqualityComparer<T>.Equals));
  private static readonly MethodInfo EqualityComparerGetHashCodeMethod = GetComparerMethodInfo(typeof(IEqualityComparer<T>), nameof(IEqualityComparer<T>.GetHashCode));
  private static readonly MethodInfo ComparerCompareMethod = GetComparerMethodInfo(typeof(IComparer<T>), nameof(IComparer<T>.Compare));

  private static readonly Expression DefaultEqualityComparerExpression = Constant(EqualityComparer<T>.Default);
  private static readonly Expression DefaultComparerExpression = Constant(Comparer<T>.Default);

  private static readonly MethodInfo DefaultEqualityComparerEqualsMethod = new Func<T, T, bool>(EqualityComparer<T>.Default.Equals).Method;
  private static readonly MethodInfo DefaultEqualityComparerGetHashCodeMethod = new Func<T, int>(EqualityComparer<T>.Default.GetHashCode).Method;
  private static readonly MethodInfo DefaultComparerCompareMethod = new Func<T, T, int>(Comparer<T>.Default.Compare).Method;

  private static readonly MethodInfo InterceptEqualsMethod = GetInterceptMethodInfo(nameof(IComparerBuilderInterception.InterceptEquals));
  private static readonly MethodInfo InterceptGetHashCodeMethod = GetInterceptMethodInfo(nameof(IComparerBuilderInterception.InterceptGetHashCode));
  private static readonly MethodInfo InterceptCompareMethod = GetInterceptMethodInfo(nameof(IComparerBuilderInterception.InterceptCompare));

  public ComparerBuilderExpression(LambdaExpression expression, IEqualityComparer<T>? equalityComparer, IComparer<T>? comparer,
    string? expressionText, string? filePath, int lineNumber) : this(expression, expressionText, filePath, lineNumber) {
    EqualityComparer = equalityComparer;
    Comparer = comparer;
  }

  public LambdaExpression Expression { get; } = expression ?? throw new ArgumentNullException(nameof(expression));

  public IEqualityComparer<T>? EqualityComparer { get; }
  public IComparer<T>? Comparer { get; }

  public string ExpressionText { get; } = expressionText ?? String.Empty;
  public string FilePath { get; } = filePath ?? String.Empty;
  public int LineNumber { get; } = lineNumber;

  private static MethodInfo GetComparerMethodInfo(Type comparerType, string methodName)
    => comparerType?.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? throw new ArgumentNullException(nameof(comparerType));

  private static MethodInfo GetInterceptMethodInfo(string methodName)
    => typeof(IComparerBuilderInterception).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).MakeGenericMethod(typeof(T));

#pragma warning disable CA1859 // Use concrete types when possible for improved performance (Skip warning for return type of methods below)

  private static Expression MakeEquals(Expression x, Expression y, IEqualityComparer<T>? comparer)
    => comparer is not null
      ? Call(Constant(comparer), EqualityComparerEqualsMethod, x, y)
      : Call(DefaultEqualityComparerExpression, DefaultEqualityComparerEqualsMethod, x, y);

  private static Expression MakeGetHashCode(Expression obj, IEqualityComparer<T>? comparer)
    => comparer is not null
      ? Call(Constant(comparer), EqualityComparerGetHashCodeMethod, obj)
      : Call(DefaultEqualityComparerExpression, DefaultEqualityComparerGetHashCodeMethod, obj);

  private static Expression MakeCompare(Expression x, Expression y, IComparer<T>? comparer)
    => comparer is not null
      ? Call(Constant(comparer), ComparerCompareMethod, x, y)
      : Call(DefaultComparerExpression, DefaultComparerCompareMethod, x, y);

  private Expression ApplyInterception(IComparerBuilderContext context, MethodInfo method, Expression value, params Expression[] args) {
    if(context is null) {
      throw new ArgumentNullException(nameof(context));
    } else if(context.Interception is null) {
      throw new ArgumentException("Interception should be specified.", nameof(context));
    } else if(method is null) {
      throw new ArgumentNullException(nameof(method));
    } else if(value is null) {
      throw new ArgumentNullException(nameof(value));
    } else if(args is null) {
      throw new ArgumentNullException(nameof(args));
    }//if

    // return {interception}.{methodName}<{valueType}>({expression}, {x}[, {y}], args);

    var instance = Constant(context.Interception);

    var variables = Array.ConvertAll(args, static item => Parameter(item.Type));
    var assigns = Enumerable.Zip(variables, args, static (param, arg) => Assign(param, arg));

    var interceptionArgs = new ComparerBuilderInterceptionArgs<T>(Expression, context.ComparedType, EqualityComparer, Comparer, ExpressionText, FilePath, LineNumber);
    var interceptionArgsExpression = Constant(interceptionArgs);

    var arguments = new List<Expression>(args.Length + 2) { value, variables, interceptionArgsExpression, };
    var call = Call(instance, method, arguments);
    var expressions = new List<Expression>(args.Length + 1) { assigns, call, };

    return Block(value.Type, variables, expressions);
  }

#pragma warning restore CA1859 // Use concrete types when possible for improved performance

  public override string ToString() => Expression.ToString();

  #region IComparerBuilderExpression Members

  public Expression AsEquals(IComparerBuilderContext context, ParameterExpression x, ParameterExpression y) {
    if(context is null) {
      throw new ArgumentNullException(nameof(context));
    }//if

    var first = ReplaceParameters(Expression, x);
    var second = ReplaceParameters(Expression, y);
    var value = MakeEquals(first, second, EqualityComparer);
    return context.Interception is null ? value : ApplyInterception(context, InterceptEqualsMethod, value, first, second);
  }

  public Expression AsGetHashCode(IComparerBuilderContext context, ParameterExpression obj) {
    if(context is null) {
      throw new ArgumentNullException(nameof(context));
    }//if

    var arg = ReplaceParameters(Expression, obj);
    var value = MakeGetHashCode(arg, EqualityComparer);
    return context.Interception is null ? value : ApplyInterception(context, InterceptGetHashCodeMethod, value, arg);
  }

  public Expression AsCompare(IComparerBuilderContext context, ParameterExpression x, ParameterExpression y) {
    if(context is null) {
      throw new ArgumentNullException(nameof(context));
    }//if

    var first = ReplaceParameters(Expression, x);
    var second = ReplaceParameters(Expression, y);
    var value = MakeCompare(first, second, Comparer);
    return context.Interception is null ? value : ApplyInterception(context, InterceptCompareMethod, value, first, second);
  }

  #endregion IComparerBuilderExpression Members
}
