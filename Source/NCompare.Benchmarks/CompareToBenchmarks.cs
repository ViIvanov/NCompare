using System.Linq.Expressions;
using System.Reflection;

using BenchmarkDotNet.Attributes;

namespace NCompare.Benchmarks;

using static Expression;

public class CompareToBenchmarks
{
  private readonly int Value1 = 377;
  private readonly int Value2 = 377;

  private static readonly MethodInfo CompareToMethodByMethodHandle = new Func<int, int>(default(int).CompareTo).Method;
  private static readonly MethodInfo CompareToMethodByMethodName = typeof(IComparable<int>).GetMethod(nameof(IComparable<int>.CompareTo), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, binder: null, types: [typeof(int)], modifiers: null)!;

  private static readonly Expression DefaultComparerExpression = Constant(Comparer<int>.Default);
  private static readonly MethodInfo DefaultComparerAbstractCompareMethod = new Comparison<int>(Comparer<int>.Default.Compare).Method;
  private static readonly MethodInfo DefaultComparerDirectCompareMethod = typeof(Comparer<int>).GetMethod(nameof(Comparer<int>.Compare), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, binder: null, types: [typeof(int), typeof(int)], modifiers: null)!;

  private static readonly Expression CustomComparerExpression = Constant(CustomInt32Comparer.Instance);
  private static readonly MethodInfo CustomComparerCompareMethod = new Comparison<int>(CustomInt32Comparer.Instance.Compare).Method;

  private static readonly ConstantExpression Zero = Constant(0);
  private static readonly ConstantExpression One = Constant(1);
  private static readonly ConstantExpression MinusOne = Constant(-1);

  private Comparison<int> CompareToDelegate { get; } = (x, y) => x.CompareTo(y);
  private Comparison<int> DefaultComparerDelegate { get; } = Comparer<int>.Default.Compare;
  private Comparison<int> CustomComparerDelegate { get; } = CustomInt32Comparer.Instance.Compare;
  private Comparison<int> ConditionDelegate { get; } = (x, y) => x < y ? -1 : (y < x ? 1 : 0);

  private Comparison<int> CompareToByMethodHandleLambda { get; } = BuildLambda((x, y) => Call(x, CompareToMethodByMethodHandle, y));
  private Comparison<int> CompareToByMethodNameLambda { get; } = BuildLambda((x, y) => Call(x, CompareToMethodByMethodName, y));
  private Comparison<int> DefaultComparerAbstractLambda { get; } = BuildLambda((x, y) => Call(DefaultComparerExpression, DefaultComparerAbstractCompareMethod, x, y));
  private Comparison<int> DefaultComparerDirectLambda { get; } = BuildLambda((x, y) => Call(DefaultComparerExpression, DefaultComparerDirectCompareMethod, x, y));
  private Comparison<int> CustomComparerLambda { get; } = BuildLambda((x, y) => Call(CustomComparerExpression, CustomComparerCompareMethod, x, y));
  private Comparison<int> ConditionLambda { get; } = BuildLambda((x, y) => Condition(LessThan(x, y), MinusOne, Condition(Equal(x, y), Zero, One)));

  private Comparison<int> CompareToFromExpression { get; } = CompileExpression((x, y) => x.CompareTo(y));
  private Comparison<int> DefaultComparerFromExpression { get; } = CompileExpression((x, y) => Comparer<int>.Default.Compare(x, y));
  private Comparison<int> CustomComparerFromExpression { get; } = CompileExpression((x, y) => CustomInt32Comparer.Instance.Compare(x, y));
  private Comparison<int> ConditionFromExpression { get; } = CompileExpression((x, y) => x < y ? -1 : (y < x ? 1 : 0));

  [Benchmark(Baseline = true)]
  public int NativeCompareTo() => Value1.CompareTo(Value2);

  [Benchmark]
  public int NativeDefaultComparer() => Comparer<int>.Default.Compare(Value1, Value2);

  [Benchmark]
  public int NativeCustomComparer() => CustomInt32Comparer.Instance.Compare(Value1, Value2);

  [Benchmark]
  public int NativeCondition() => Value1 < Value2 ? -1 : (Value2 < Value1 ? 1 : 0);

  [Benchmark]
  public int DelegateCompareTo() => CompareToDelegate(Value1, Value2);

  [Benchmark]
  public int DelegateDefaultComparer() => DefaultComparerDelegate(Value1, Value2);

  [Benchmark]
  public int DelegateCustomComparer() => CustomComparerDelegate(Value1, Value2);

  [Benchmark]
  public int DelegateCondition() => ConditionDelegate(Value1, Value2);

  [Benchmark]
  public int LambdaCompareToByMethodHandle() => CompareToByMethodHandleLambda(Value1, Value2);

  [Benchmark]
  public int LambdaCompareToByMethodName() => CompareToByMethodNameLambda(Value1, Value2);

  [Benchmark]
  public int LambdaDefaultComparerAbstract() => DefaultComparerAbstractLambda(Value1, Value2);

  [Benchmark]
  public int LambdaDefaultComparerDirect() => DefaultComparerDirectLambda(Value1, Value2);

  [Benchmark]
  public int LambdaCustomComparer() => CustomComparerLambda(Value1, Value2);

  [Benchmark]
  public int LambdaCondition() => ConditionLambda(Value1, Value2);

  [Benchmark]
  public int ExpressionCompareTo() => CompareToFromExpression(Value1, Value2);

  [Benchmark]
  public int ExpressionDefaultComparer() => DefaultComparerFromExpression(Value1, Value2);

  [Benchmark]
  public int ExpressionCustomComparer() => CustomComparerFromExpression(Value1, Value2);

  [Benchmark]
  public int ExpressionCondition() => ConditionFromExpression(Value1, Value2);

  private static Comparison<int> BuildLambda(Func<Expression, Expression, Expression> builder) {
    var x = Parameter(typeof(int), "x");
    var y = Parameter(typeof(int), "y");
    var expression = builder(x, y);
    var lambda = Lambda<Comparison<int>>(expression, x, y);
    return CompileExpression(lambda);
  }

  private static Comparison<int> CompileExpression(Expression<Comparison<int>> expression) => expression.Compile();

  private sealed class CustomInt32Comparer : Comparer<int>
  {
    private CustomInt32Comparer() { }

    public static CustomInt32Comparer Instance { get; } = new();

    public override int Compare(int x, int y) => x < y ? -1 : (y < x ? 1 : 0);
  }
}
