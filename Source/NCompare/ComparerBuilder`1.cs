using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NCompare
{
  using static Expression;
  using static ComparerBuilder;

  [DebuggerDisplay("{DebuggerDisplay}")]
  public class ComparerBuilder<T>
  {
    #region Cached Expression and Reflection objects

    internal static readonly ParameterExpression X = Parameter(typeof(T), nameof(X).ToLowerInvariant());
    internal static readonly ParameterExpression Y = Parameter(typeof(T), nameof(Y).ToLowerInvariant());
    internal static readonly ParameterExpression Obj = Parameter(typeof(T), nameof(Obj).ToLowerInvariant());

    private static readonly Type ComparedType = typeof(T);
    private static readonly bool IsValueType = ComparedType.IsValueType;

    #endregion Cached Expression and Reflection objects

    public ComparerBuilder() : this(expressions: new List<IComparerBuilderExpression>()) { }

    public ComparerBuilder(IComparerBuilderInterception interception) : this(expressions: new List<IComparerBuilderExpression>(), interception) { }

    private ComparerBuilder(List<IComparerBuilderExpression> expressions, IComparerBuilderInterception? interception = null) {
      Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
      Interception = interception;
    }

    private List<IComparerBuilderExpression> Expressions { get; }
    public IComparerBuilderInterception? Interception { get; }

    public bool IsEmpty => Expressions.Count == 0;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"Expressions: {Expressions.Count} item(s).";

    private ComparerBuilder<T> Add(IComparerBuilderExpression expression) {
      Expressions.Add(expression ?? throw new ArgumentNullException(nameof(expression)));
      return this;
    }

    #region Add Overloads

    public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) {
      var value = ComparerBuilderExpression<TValue>.Create(expression, filePath!, lineNumber);
      return Add(value);
    }

    public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, IEqualityComparer<TValue>? equalityComparer, IComparer<TValue>? comparisonComparer = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) {
      var equalityComparerOrDefault = equalityComparer ?? EqualityComparer<TValue>.Default;
      var comparisonComparerOrDefault = comparisonComparer ?? Comparer<TValue>.Default;
      var value = ComparerBuilderExpression.Create(expression, equalityComparerOrDefault, comparisonComparerOrDefault, filePath!, lineNumber);
      return Add(value);
    }

    public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, Lazy<IEqualityComparer<TValue>>? equalityComparer, Lazy<IComparer<TValue>>? comparisonComparer = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) {
      var equalityComparerOrDefault = equalityComparer ?? LazyComparers<TValue>.DefaultInterfaceEqualityComparer;
      var comparisonComparerOrDefault = comparisonComparer ?? LazyComparers<TValue>.DefaultInterfaceComparer;
      var value = ComparerBuilderExpression.Create(expression, equalityComparerOrDefault, comparisonComparerOrDefault, filePath!, lineNumber);
      return Add(value);
    }

    public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, Lazy<EqualityComparer<TValue>>? equalityComparer, Lazy<Comparer<TValue>>? comparisonComparer = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) {
      var equalityComparerOrDefault = equalityComparer ?? LazyComparers<TValue>.DefaultClassEqualityComparer;
      var comparisonComparerOrDefault = comparisonComparer ?? LazyComparers<TValue>.DefaultClassComparer;
      var value = ComparerBuilderExpression.Create(expression, equalityComparerOrDefault, comparisonComparerOrDefault, filePath!, lineNumber);
      return Add(value);
    }

    public ComparerBuilder<T> Add<TValue, TComparer>(Expression<Func<T, TValue>> expression, Lazy<TComparer> comparer, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0) where TComparer : IEqualityComparer<TValue>, IComparer<TValue> {
      var value = ComparerBuilderExpression<TValue>.Create(expression, comparer, filePath!, lineNumber);
      return Add(value);
    }

    public ComparerBuilder<T> Add(Expression<Func<T, T>> expression) {
      var value = new RecursiveComparerBuilderExpression<T>(expression, this);
      return Add(value);
    }

    public ComparerBuilder<T> Add<TValue>(Expression<Func<T, TValue>> expression, ComparerBuilder<TValue> builder) {
      if((object)builder == this) {
        throw new ArgumentException($"Recursive comparer added: \"{builder}\".", paramName: nameof(builder));
      }//if

      var value = new NestedComparerBuilderExpression<TValue>(expression, builder);
      return Add(value);
    }

    public ComparerBuilder<T> Add(ComparerBuilder<T> other) {
      if(other is null) {
        throw new ArgumentNullException(nameof(other));
      }//if

      Expressions.AddRange(other.Expressions);
      return this;
    }

    #endregion Add Overloads

    public ComparerBuilder<TDerived> ConvertTo<TDerived>() where TDerived : T => new ComparerBuilder<TDerived>(Expressions.ToList(), Interception);

    #region Build Methods

    internal Expression<Func<T, T, bool>> BuildEquals(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null) {
      var expressions =
        from item in Expressions
        let expr = item.AsEquals(x, y, comparedType, interception)
        where expr != null
        select expr;
      var expression = expressions.Aggregate(AndAlso);
      var body = IsValueType
        ? expression
        // (object)x == (object)y || ((object)x != null && (object)y != null && expression);
        : OrElse(ReferenceEqual(x, y), AndAlso(AndAlso(IsNotNull(x), IsNotNull(y)), expression));
      return Lambda<Func<T, T, bool>>(body, x, y);
    }

    internal Expression<Func<T, int>> BuildGetHashCode(ParameterExpression obj, Type comparedType, IComparerBuilderInterception? interception = null) {
      var expressions =
        from item in Expressions
        let expr = item.AsGetHashCode(obj, comparedType, interception)
        where expr != null
        select expr;
      var list = expressions.ToList();
      var expression = list.Skip(1).Select((item, index) => Tuple.Create(item, index + 1))
        .Aggregate(list.First(), (acc, item) => ExclusiveOr(acc, Call(RotateRightDelegate.Method, item.Item1, Constant(item.Item2))));
      var body = IsValueType
        ? expression
        // ((object)obj == null) ? 0 : expression;
        : Condition(IsNull(obj), Zero, expression);
      return Lambda<Func<T, int>>(body, obj);
    }

    internal Expression<Func<T, T, int>> BuildCompare(ParameterExpression x, ParameterExpression y, Type comparedType, IComparerBuilderInterception? interception = null) {
      var reverse = Expressions.Select(item => item.AsCompare(x, y, comparedType, interception)).Reverse().ToList();

      var labelTargetReturn = Label(typeof(int));
      var labelZero = Label(labelTargetReturn, Zero);
      var returnZero = Return(labelTargetReturn, Zero);
      var returnOne = Return(labelTargetReturn, One);
      var returnMinusOne = Return(labelTargetReturn, MinusOne);
      var returnCompare = Return(labelTargetReturn, Compare);

      Expression seed = Return(labelTargetReturn, reverse.First());
      var expression = reverse.Skip(1).Aggregate(seed, (acc, value) => IfThenElse(NotEqual(Assign(Compare, value), Zero), returnCompare, acc));
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
        : IfThenElse(ReferenceEqual(x, y), returnZero, IfThenElse(IsNull(x), returnMinusOne, IfThenElse(IsNull(y), returnOne, expression)));
      var block = Block(CompareVariables, body, labelZero);
      return Lambda<Func<T, T, int>>(block, x, y);
    }

    #endregion Build Methods

    private void ThrowIfEmpty() {
      if(IsEmpty) {
        const string Message = "There are no expressions specified.";
        throw new InvalidOperationException(Message);
      }//if
    }

    public EqualityComparer<T> CreateEqualityComparer(IComparerBuilderInterception? interception = null) {
      ThrowIfEmpty();
      var equals = BuildEquals(X, Y, ComparedType, interception ?? Interception);
      var equalsDelegate = equals.Compile();
      var hashCode = BuildGetHashCode(Obj, ComparedType, interception ?? Interception);
      var hashCodeDelegate = hashCode.Compile();
      return Comparers.Create(equalsDelegate, hashCodeDelegate);
    }

    public Comparer<T> CreateComparer(IComparerBuilderInterception? interception = null) {
      ThrowIfEmpty();
      var compare = BuildCompare(X, Y, ComparedType, interception ?? Interception);
      var compareDelegate = compare.Compile();
      return Comparers.Create(compareDelegate);
    }

    public Lazy<EqualityComparer<T>> CreateLazyEqualityComparer() => Lazy.Create(() => CreateEqualityComparer());
    public Lazy<Comparer<T>> CreateLazyComparer() => Lazy.Create(() => CreateComparer());
  }
}
