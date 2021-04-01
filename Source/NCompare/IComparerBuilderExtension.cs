namespace NCompare
{
  public interface IComparerBuilderExtension
  {
    bool Equals<T>(T x, T y, bool value, ComparerExpressionContext<T> context);
    int GetHashCode<T>(T obj, int value, ComparerExpressionContext<T> context);
    int Compare<T>(T x, T y, int value, ComparerExpressionContext<T> context);
  }
}
