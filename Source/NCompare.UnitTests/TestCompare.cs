using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests;

internal static class TestCompare
{
  public static void TestCollectionComparators<TCollection, TItem>(string title, Func<TItem?[], TCollection> factory, TItem notDefaultItem, ComparerBuilder<TCollection> comparerBuilder) {
    ArgumentNullException.ThrowIfNull(factory);
    ArgumentNullException.ThrowIfNull(comparerBuilder);

    var emptyCollection = factory([]);
    TestComparators($"{title}: Default collections equal", default, default, CompareResult.Equal, comparerBuilder);
    TestComparators($"{title}: Default and Empty collections equal", default, emptyCollection, CompareResult.Equal, comparerBuilder);
    TestComparators($"{title}: Empty collections equal", emptyCollection, emptyCollection, CompareResult.Equal, comparerBuilder);

    var oneDefaultItemCollection = factory([default,]);
    TestComparators($"{title}: Default and One", default, oneDefaultItemCollection, CompareResult.LessThan, comparerBuilder);
    TestComparators($"{title}: Empty and One", emptyCollection, oneDefaultItemCollection, CompareResult.LessThan, comparerBuilder);

    var oneNotDefaultItemCollection = factory(new[] { notDefaultItem, });
    TestComparators($"{title}: Default Item and NotDefault Item", oneDefaultItemCollection, oneNotDefaultItemCollection, CompareResult.LessThan, comparerBuilder);
  }

  public static void TestComparators<T>(string title, T? x, T? y, CompareResult expected, ComparerBuilder<T> comparerBuilder) {
    ArgumentNullException.ThrowIfNull(comparerBuilder);

    var equalityComparer = comparerBuilder.CreateEqualityComparer();
    TestEqualityComparer(title, x, y, expected is CompareResult.Equal, equalityComparer);

    var comparer = comparerBuilder.CreateComparer();
    TestComparer(title, x, y, expected, comparer);
  }

  public static void TestComparators<T>(string title, T? x, T? y, CompareResult expected, IEqualityComparer<T> equalityComparer, IComparer<T> comparer) {
    TestEqualityComparer(title, x, y, expected is CompareResult.Equal, equalityComparer);
    TestComparer(title, x, y, expected, comparer);
  }

  public static void TestEqualityComparer<T>(string title, T? x, T? y, bool expected, IEqualityComparer<T> equalityComparer) {
    ArgumentNullException.ThrowIfNull(equalityComparer);

    Assert.AreEqual(x, x, equalityComparer, $"{title}: Equals(x, x) failed");
    Assert.AreEqual(y, y, equalityComparer, $"{title}: Equals(y, y) failed");

    if(expected) {
      Assert.AreEqual(x, y, equalityComparer, $"{title}: Equals(x, y) failed");
      Assert.AreEqual(y, x, equalityComparer, $"{title}: Equals(y, x) failed");
    } else {
      Assert.AreNotEqual(x, y, equalityComparer, $"{title}: Equals(x, y) failed");
      Assert.AreNotEqual(y, x, equalityComparer, $"{title}: Equals(y, x) failed");
    }//if

    if(x is not null) {
      var xhash1 = equalityComparer.GetHashCode(x);
      var xhash2 = equalityComparer.GetHashCode(x);
      Assert.AreEqual(xhash1, xhash2, $"{title}: GetHashCode[x]({x}) is not stable: {xhash1}, {xhash2}");
    }//if

    if(y is not null) {
      var yhash1 = equalityComparer.GetHashCode(y);
      var yhash2 = equalityComparer.GetHashCode(y);
      Assert.AreEqual(yhash1, yhash2, $"{title}: GetHashCode[y]({y}) is not stable: {yhash1}, {yhash2}");
    }//if

    if(expected && x is not null && y is not null) {
      var xhash = equalityComparer.GetHashCode(x);
      var yhash = equalityComparer.GetHashCode(y);
      Assert.AreEqual(xhash, yhash, $"{title}: GetHashCode({Q(x)}|{Q(y)}) is not symmetric: {xhash}, {yhash}");
    }//if
  }

  public static void TestComparer<T>(string title, T? x, T? y, CompareResult expected, IComparer<T> comparer) {
    ArgumentNullException.ThrowIfNull(comparer);

    var xx = comparer.Compare(x, x);
    Assert.AreEqual(CompareResult.Equal, GetCompareResult(xx), $"{title}: Compare[x, x]({Q(x)}, {Q(x)}) failed {xx}");

    var yy = comparer.Compare(y, y);
    Assert.AreEqual(CompareResult.Equal, GetCompareResult(yy), $"{title}: Compare[y, y]({Q(y)}, {Q(y)}) failed {yy}");

    var xy = comparer.Compare(x, y);
    Assert.AreEqual(expected, GetCompareResult(xy), $"{title}: Compare[x, y]({Q(x)}, {Q(y)}) failed ({xy})");

    var yx = comparer.Compare(y, x);
    Assert.AreEqual(ReverseCompareResult(expected), GetCompareResult(yx), $"{title}: Compare[y, x]({Q(y)}, {Q(x)}) failed ({yx})");

    static CompareResult GetCompareResult(int value) => value < 0 ? CompareResult.LessThan : (value > 0 ? CompareResult.GreaterThan : CompareResult.Equal);

    static CompareResult ReverseCompareResult(CompareResult value) => value switch {
      CompareResult.LessThan => CompareResult.GreaterThan,
      CompareResult.GreaterThan => CompareResult.LessThan,
      _ => CompareResult.Equal,
    };
  }

  private static string Q<T>(T value) => $"{{ {value} }}";
}
