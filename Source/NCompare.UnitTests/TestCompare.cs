using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests
{
  public enum CompareResult
  {
    Equal,
    LessThan,
    GreaterThan,
  }

  internal static class TestCompare
  {
    public static void TestComparers<T>(string title, T? x, T? y, CompareResult expected, IEqualityComparer<T> equalityComparer, IComparer<T> comparer) {
      TestEqualityComparer(title, x, y, expected == CompareResult.Equal, equalityComparer);
      TestComparer(title, x, y, expected, comparer);
    }

    public static void TestEqualityComparer<T>(string title, T? x, T? y, bool expected, IEqualityComparer<T> equalityComparer) {
      if(equalityComparer is null) {
        throw new ArgumentNullException(nameof(equalityComparer));
      }//if

      var xx = equalityComparer.Equals(x, x);
      Assert.IsTrue(xx, $"{title}: Equals({x}, {x}) failed");

      var yy = equalityComparer.Equals(y, y);
      Assert.IsTrue(yy, $"{title}: Equals({y}, {y}) failed");

      var xy = equalityComparer.Equals(x, y);
      Assert.AreEqual(expected, xy, $"{title}: Equals({x}, {y}) failed");

      var yx = equalityComparer.Equals(y, x);
      Assert.AreEqual(expected, yx, $"{title}: Equals({y}, {x}) failed");

      if(x is not null) {
        var xhash1 = equalityComparer.GetHashCode(x);
        var xhash2 = equalityComparer.GetHashCode(x);
        Assert.IsTrue(xhash1 == xhash2, $"{title}: GetHashCode({x}) is not stable: {xhash1}, {xhash2}");
      }//if

      if(y is not null) {
        var yhash1 = equalityComparer.GetHashCode(y);
        var yhash2 = equalityComparer.GetHashCode(y);
        Assert.IsTrue(yhash1 == yhash2, $"{title}: GetHashCode({y}) is not stable: {yhash1}, {yhash2}");
      }//if

      if(expected && x is not null && y is not null) {
        var xhash = equalityComparer.GetHashCode(x);
        var yhash = equalityComparer.GetHashCode(y);
        Assert.IsTrue(xhash == yhash, $"{title}: GetHashCode({x}) is not symmetric: {xhash}, {yhash}");
      }//if
    }

    public static void TestComparer<T>(string title, T? x, T? y, CompareResult expected, IComparer<T> comparer) {
      if(comparer is null) {
        throw new ArgumentNullException(nameof(comparer));
      }//if

      var xx = comparer.Compare(x, x);
      Assert.AreEqual(CompareResult.Equal, GetCompareResult(xx), $"{title}: Compare({x}, {x}) failed {xx}");

      var yy = comparer.Compare(y, y);
      Assert.AreEqual(CompareResult.Equal, GetCompareResult(yy), $"{title}: Compare({y}, {y}) failed {yy}");

      var xy = comparer.Compare(x, y);
      Assert.AreEqual(expected, GetCompareResult(xy), $"Compare({x}, {y}) failed ({xy})");

      var yx = comparer.Compare(y, x);
      Assert.AreEqual(ReverseCompareResult(expected), GetCompareResult(yx), $"Compare({y}, {x}) failed ({yx})");

      static CompareResult GetCompareResult(int value) => value < 0 ? CompareResult.LessThan : (value > 0 ? CompareResult.GreaterThan : CompareResult.Equal);

      static CompareResult ReverseCompareResult(CompareResult value) => value switch {
        CompareResult.LessThan => CompareResult.GreaterThan,
        CompareResult.GreaterThan => CompareResult.LessThan,
        _ => CompareResult.Equal,
      };
    }
  }
}
