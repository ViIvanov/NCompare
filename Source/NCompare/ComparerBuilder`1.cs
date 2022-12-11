using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NCompare;

using static Expression;
using static ComparerBuilder;

[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
public sealed class ComparerBuilder<T> : IComparerBuilderContext
{
  #region Cached Expression and Reflection objects

  internal static readonly ParameterExpression X = Parameter(typeof(T), nameof(X).ToLowerInvariant());
  internal static readonly ParameterExpression Y = Parameter(typeof(T), nameof(Y).ToLowerInvariant());
  internal static readonly ParameterExpression Obj = Parameter(typeof(T), nameof(Obj).ToLowerInvariant());

  private static readonly Type ComparedType = typeof(T);
  private static readonly bool IsValueType = ComparedType.IsValueType;

  #endregion Cached Expression and Reflection objects

  public ComparerBuilder() : this(expressions: new()) { }

  public ComparerBuilder(IComparerBuilderInterception? interception) : this(expressions: new(), interception) { }

  private ComparerBuilder(List<IComparerBuilderExpression> expressions, IComparerBuilderInterception? interception = null) {
    Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
    Interception = interception;

    EqualityComparer = new(this);
    Comparer = new(this);
  }

  private List<IComparerBuilderExpression> Expressions { get; }
  public IComparerBuilderInterception? Interception { get; }

  Type IComparerBuilderContext.ComparedType => ComparedType;

  public bool IsEmpty => Expressions.Count == 0;

  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  private string DebuggerDisplay => $"Expressions: {Expressions.Count} item(s).";

  private LazyEqualityComparer EqualityComparer { get; }
  private LazyComparer Comparer { get; }

  private ComparerBuilder<T> Add(IComparerBuilderExpression expression) {
    if(EqualityComparer.IsValueCreated || Comparer.IsValueCreated) {
      const string Message = "Comparer(s) already created.";
      throw new InvalidOperationException(Message);
    }//if

    Expressions.Add(expression ?? throw new ArgumentNullException(nameof(expression)));
    return this;
  }

  #region Add Overloads

  public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression,
    [CallerArgumentExpression(nameof(expression))] string? expressionText = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) {
    var value = ComparerBuilderExpression.Create(expression, expressionText, filePath, lineNumber);
    return Add(value);
  }

  public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, IEqualityComparer<TValue>? equalityComparer, IComparer<TValue>? comparer = null,
    [CallerArgumentExpression(nameof(expression))] string? expressionText = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) {
    var equalityComparerOrDefault = equalityComparer ?? EqualityComparer<TValue>.Default;
    var comparerOrDefault = comparer ?? Comparer<TValue>.Default;
    var value = ComparerBuilderExpression.Create(expression, equalityComparerOrDefault, comparerOrDefault, expressionText, filePath, lineNumber);
    return Add(value);
  }

  public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, IComparer<TValue>? comparer,
    [CallerArgumentExpression(nameof(expression))] string? expressionText = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
    => Add(expression, equalityComparer: null, comparer, expressionText, filePath, lineNumber);

  public ComparerBuilder<T> Add<TValue, TComparer>(Expression<Func<T, TValue>> expression, TComparer comparer,
    [CallerArgumentExpression(nameof(expression))] string? expressionText = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) where TComparer : IEqualityComparer<TValue>, IComparer<TValue>
    => Add(expression, comparer, comparer, expressionText, filePath, lineNumber);

  public ComparerBuilder<T> Add(Expression<Func<T, T>> expression,
    [CallerArgumentExpression(nameof(expression))] string? expressionText = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
    => Add(expression, EqualityComparer, Comparer, expressionText, filePath, lineNumber);

  public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, ComparerBuilder<TValue> builder,
    [CallerArgumentExpression(nameof(expression))] string? expressionText = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
    => Add(expression, builder?.EqualityComparer ?? throw new ArgumentNullException(nameof(builder)), builder?.Comparer, expressionText, filePath, lineNumber);

  public ComparerBuilder<T> Add(ComparerBuilder<T> other) {
    if(other is null) {
      throw new ArgumentNullException(nameof(other));
    }//if

    Expressions.AddRange(other.Expressions);
    return this;
  }

  #endregion Add Overloads

  public ComparerBuilder<TDerived> ConvertTo<TDerived>() where TDerived : T => new(new(Expressions), Interception);

  #region Build Methods

  internal Expression<Func<T, T, bool>> BuildEquals(ParameterExpression x, ParameterExpression y) {
    var expressions = Expressions.ConvertAll(item => item.AsEquals(this, x, y));
    var expression = expressions.Aggregate(AndAlso);
    var body = IsValueType
      ? expression
      // (object)x == (object)y || ((object)x != null && (object)y != null && expression);
      : OrElse(ReferenceEqual(x, y), AndAlso(AndAlso(IsNotNull(x), IsNotNull(y)), expression));
    return Lambda<Func<T, T, bool>>(body, x, y);
  }

  internal Expression<Func<T, int>> BuildGetHashCode(ParameterExpression obj) {
    var expressions = Expressions.ConvertAll(item => item.AsGetHashCode(this, obj));
    var expression = expressions.Skip(1).Select((item, index) => (Expression: item, Index: index + 1))
      .Aggregate(expressions[0], (acc, item) => ExclusiveOr(acc, Call(RotateRightDelegate.Method, item.Expression, Constant(item.Index))));
    var body = IsValueType
      ? expression
      // ((object)obj == null) ? 0 : expression;
      : Condition(IsNull(obj), Zero, expression);
    return Lambda<Func<T, int>>(body, obj);
  }

  internal Expression<Comparison<T>> BuildCompare(ParameterExpression x, ParameterExpression y) {
    var expressions = Expressions.ConvertAll(item => item.AsCompare(this, x, y));
    expressions.Reverse();

    Expression seed = Return(LabelTargetReturn, expressions[0]);
    var expression = expressions.Skip(1).Aggregate(seed, (acc, value) => IfThenElse(NotEqual(Assign(Compare, value), Zero), ReturnCompare, acc));
    var body = IsValueType
      ? expression
      // if((object)x == (object)y) {
      //   return 0;
      // } else if((object)x == null) {
      //   return -1;
      // } else if((object)y == null) {
      //   return 1;
      // } else {
      //   return expression;
      // }//if
      : IfThenElse(ReferenceEqual(x, y), ReturnZero, IfThenElse(IsNull(x), ReturnMinusOne, IfThenElse(IsNull(y), ReturnOne, expression)));
    var block = Block(CompareVariables, body, LabelZero);
    return Lambda<Comparison<T>>(block, x, y);
  }

  #endregion Build Methods

  private void ThrowIfEmpty() {
    if(IsEmpty) {
      const string Message = "There are no expressions specified.";
      throw new InvalidOperationException(Message);
    }//if
  }

  private EqualityComparer<T> BuildEqualityComparer() {
    ThrowIfEmpty();

    var equals = BuildEquals(X, Y);
    var equalsDelegate = equals.Compile();

    var hashCode = BuildGetHashCode(Obj);
    var hashCodeDelegate = hashCode.Compile();

    return Comparers.Create(equalsDelegate, hashCodeDelegate);
  }

  private Comparer<T> BuildComparer() {
    ThrowIfEmpty();

    var compare = BuildCompare(X, Y);
    var compareDelegate = compare.Compile();

    return Comparer<T>.Create(compareDelegate);
  }

  public EqualityComparer<T> CreateEqualityComparer() => EqualityComparer.Value;
  public Comparer<T> CreateComparer() => Comparer.Value;

  private sealed class LazyEqualityComparer : EqualityComparer<T>
  {
    public LazyEqualityComparer(ComparerBuilder<T> builder) {
      Builder = builder ?? throw new ArgumentNullException(nameof(builder));
      LazyValue = new(() => Builder.BuildEqualityComparer());
    }

    private ComparerBuilder<T> Builder { get; }
    private Lazy<EqualityComparer<T>> LazyValue { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never), DebuggerHidden]
    public EqualityComparer<T> Value => LazyValue.Value;

    public bool IsValueCreated => LazyValue.IsValueCreated;

    public override bool Equals(T x, T y) => Value.Equals(x, y);
    public override int GetHashCode(T obj) => Value.GetHashCode(obj);

    public override bool Equals(object obj) => obj is LazyEqualityComparer other && other.Builder == Builder;
    public override int GetHashCode() => Builder.GetHashCode();
  }

  private sealed class LazyComparer : Comparer<T>
  {
    public LazyComparer(ComparerBuilder<T> builder) {
      Builder = builder ?? throw new ArgumentNullException(nameof(builder));
      LazyValue = new(() => Builder.BuildComparer());
    }

    private ComparerBuilder<T> Builder { get; }
    private Lazy<Comparer<T>> LazyValue { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never), DebuggerHidden]
    public Comparer<T> Value => LazyValue.Value;

    public bool IsValueCreated => LazyValue.IsValueCreated;

    public override int Compare(T x, T y) => Value.Compare(x, y);

    public override bool Equals(object obj) => obj is LazyComparer other && other.Builder == Builder;
    public override int GetHashCode() => Builder.GetHashCode();
  }
}
