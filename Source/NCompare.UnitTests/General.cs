using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests
{
  using static TestCompare;

  [TestClass]
  public sealed class General
  {
    #region Regular Tests

    [TestMethod]
    public void IsEmpty() {
      var builder = new ComparerBuilder<TestValue>();
      Assert.IsTrue(builder.IsEmpty, "builder.IsEmpty should be true after object creation.");
      Assert.ThrowsException<InvalidOperationException>(() => builder.CreateEqualityComparer());
      Assert.ThrowsException<InvalidOperationException>(() => builder.CreateComparer());

      builder = builder.Add(item => item.Number);
      Assert.IsFalse(builder.IsEmpty, "builder.IsEmpty should be false after object creation.");
    }

    [TestMethod]
    public void DefaultEqualityComparer() {
      var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
      var equality = builder.CreateEqualityComparer();

      var x = new TestValue(1);
      var y = new TestValue(1);
      Assert.IsTrue(equality.Equals(x, y), $"{x.Number} == {y.Number}");

      var z = new TestValue(2);
      Assert.IsFalse(equality.Equals(z, y), $"{z.Number} != {y.Number}");
    }

    [TestMethod]
    public void DefaultComparer() {
      var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
      var equality = builder.CreateComparer();

      var x = new TestValue(1);
      var y = new TestValue(1);
      Assert.AreEqual(expected: 0, equality.Compare(x, y), $"{x.Number} == {y.Number}");

      var z = new TestValue(2);
      Assert.IsTrue(equality.Compare(x, z) < 0, $"{x.Number} < {z.Number}");
      Assert.IsTrue(equality.Compare(z, y) > 0, $"{z.Number} < {y.Number}");
    }

    #endregion Regular Tests

    [TestMethod]
    public void Overloads() {
      var builder = new ComparerBuilder<TContainer>()
        .Add(item => item.MyValue)
        .Add(item => item.MyObject);
      var equalityComparer = builder.CreateEqualityComparer();

      var x = new TContainer {
        MyObject = new TObject { Value = 1, },
        MyValue = new TValue { Value = 2, },
      };

      var h = equalityComparer.GetHashCode(x);

      var y = new TContainer {
        MyObject = new TObject { Value = 1, },
        MyValue = new TValue { Value = 2, },
      };

      var b = equalityComparer.Equals(x, y);
    }

    private class TContainer
    {
      public TObject? MyObject { get; set; }
      public TValue MyValue { get; set; }
    }

    private abstract class TBaseObject : IEquatable<TBaseObject>, IEquatable<TObject>
    {
      public override bool Equals(object? other) {
        Debug.Print($"{nameof(TBaseObject)}::{nameof(Equals)}(object) called");
        return true;
      }

      public override int GetHashCode() {
        Debug.Print($"{nameof(TBaseObject)}::{nameof(GetHashCode)}() called");
        return 0;
      }

      public bool Equals(TBaseObject? other) {
        Debug.Print($"{nameof(TBaseObject)}::{nameof(Equals)}({nameof(TBaseObject)}) called");
        return true;
      }

      public bool Equals(TObject? other) {
        Debug.Print($"{nameof(TBaseObject)}::{nameof(Equals)}({nameof(TObject)}) called");
        return true;
      }

      public static bool operator ==(TBaseObject? left, TBaseObject? right) => true;
      public static bool operator !=(TBaseObject? left, TBaseObject? right) => true;
    }

    private sealed class TObject : TBaseObject
    {
      public int Value { get; set; }

      public new bool Equals(object? other) {
        Assert.Fail($"{nameof(TObject)}::{nameof(Equals)}(object) called");
        return true;
      }

      public new int GetHashCode() {
        Assert.Fail($"{nameof(TObject)}::{nameof(GetHashCode)}() called");
        return 0;
      }

      public new bool Equals(TObject? other) {
        Assert.Fail($"{nameof(TObject)}::{nameof(Equals)}({nameof(TObject)}) called");
        return true;
      }
    }

#pragma warning disable CS0660, CS0661 // Type defines operator == or operator != but does not override Object.Equals(object o) / Object.GetHashCode()
    private struct TValue
#pragma warning restore CS0660, CS0661 // Type defines operator == or operator != but does not override Object.Equals(object o) / Object.GetHashCode()
    {
      public int Value { get; set; }

      public new bool Equals(object? other) {
        Assert.Fail($"{nameof(TValue)}::{nameof(Equals)}(object) called");
        return true;
      }

      public new int GetHashCode() {
        Assert.Fail($"{nameof(TValue)}::{nameof(GetHashCode)}() called");
        return 0;
      }

      public bool Equals(TValue other) {
        Assert.Fail($"{nameof(TValue)}::{nameof(Equals)}({nameof(TObject)}) called");
        return true;
      }

      public static bool operator ==(TValue left, TValue right) => true;
      public static bool operator !=(TValue left, TValue right) => true;
    }

    [TestMethod]
    public void DerivedAndNested() {
      var builderOtherValue = new ComparerBuilder<OtherValue>()
        .Add(value => value.Text ?? String.Empty, StringComparer.OrdinalIgnoreCase);

      var builderTestValueForNumber = new ComparerBuilder<TestValue>()
        .Add(value => value.Number % 2);

      var builderTestValueForDateTime = new ComparerBuilder<TestValue>()
        .Add(value => value.DateTime != null ? value.DateTime.Value.Date : default);

      var builderData = builderTestValueForNumber
        .Add(builderTestValueForDateTime)
        .ConvertTo<DerivedValue>()
        .Add(value => value.Other, builderOtherValue!);

      var equalityComparer = builderData.CreateEqualityComparer();
      var comparer = builderData.CreateComparer();

      var data1 = new DerivedValue(2, DateTime.Now, new OtherValue("a"));
      var data2 = new DerivedValue(4, DateTime.Now, new OtherValue("A"));
      var data3 = new DerivedValue(6, DateTime.Now, new OtherValue("c"));
      var data4 = new DerivedValue(1, null, new OtherValue("b"));

      var e1 = equalityComparer.Equals(data1, data2);
      Assert.IsTrue(e1);
      var e2 = equalityComparer.Equals(data2, data3);
      Assert.IsFalse(e2);
      var e3 = equalityComparer.Equals(data1, data4);
      Assert.IsFalse(e3);
      var c1 = comparer.Compare(data1, data4);
      Assert.AreEqual(expected: -1, c1);
    }

    [TestMethod]
    public void Nested() {
      var xbuilder = new ComparerBuilder<XObject>();
      var ybuilder = new ComparerBuilder<YObject>();

      xbuilder.Add(item => item.Text.Length > 0 ? item.Text[0].ToString() : String.Empty, StringComparer.OrdinalIgnoreCase);
      xbuilder.Add(item => item.Y, ybuilder!);

      ybuilder.Add(item => item.Value < 0 ? -1 : (item.Value > 0 ? 1 : 0));
      ybuilder.Add(item => item.X, xbuilder!);

      var xcomparer = xbuilder.CreateEqualityComparer();
      var ycomparer = ybuilder.CreateEqualityComparer();

      var x1 = new XObject { Text = "Simple X1", };
      var x2 = new XObject { Text = "Zimple X2", };
      var x3 = new XObject { Text = "simple X3", };

      TestEqualityComparer("x/1/2", x1, x2, expected: false, xcomparer);
      TestEqualityComparer("x/2/3", x2, x3, expected: false, xcomparer!);
      TestEqualityComparer("x/1/3", x1, x3, expected: true, xcomparer!);
      TestEqualityComparer("x/1/null", x1, null, expected: false, xcomparer!);

      var y1 = new YObject { Value = -5, X = x1, };
      var y2 = new YObject { Value = 10, X = x2, };
      var y3 = new YObject { Value = -1, X = x3, };
      var y4 = new YObject { Value = -1, };

      TestEqualityComparer("y/1/2", y1, y2, expected: false, ycomparer);
      TestEqualityComparer("y/2/3", y2, y3, expected: false, ycomparer!);
      TestEqualityComparer("y/1/3", y1, y3, expected: true, ycomparer!);
      TestEqualityComparer("y/2/4", y2, y4, expected: false, ycomparer!);
      TestEqualityComparer("y/1/null", y1, null, expected: false, ycomparer!);

      var x11 = new XObject { Text = "Complex X11", Y = y1, };
      var x12 = new XObject { Text = "gomplex X12", Y = y2, };
      var x13 = new XObject { Text = "complex X13", Y = y3, };
      var x14 = new XObject { Text = "complex X14", Y = y4, };
      var x15 = new XObject { Text = "complex X13", Y = y3, };

      TestEqualityComparer("x/11/12", x11, x12, expected: false, xcomparer);
      TestEqualityComparer("x/12/13", x12, x13, expected: false, xcomparer!);
      TestEqualityComparer("x/11/13", x11, x13, expected: true, xcomparer!);
      TestEqualityComparer("x/14/15", x14, x15, expected: false, xcomparer!);
      TestEqualityComparer("x/11/null", x11, null, expected: false, xcomparer!);
    }

    //private static void TestComparers<T>(string title, [MaybeNull] T x, [MaybeNull] T y, CompareResult expected, IEqualityComparer<T> equalityComparer, IComparer<T> comparer) {
    //  TestEqualityComparer(title, x, y, expected == CompareResult.Equal, equalityComparer);
    //  TestComparer(title, x, y, expected, comparer);
    //}

    //private static void TestEqualityComparer<T>(string title, [MaybeNull] T x, [MaybeNull] T y, bool expected, IEqualityComparer<T> equalityComparer) {
    //  var xx = equalityComparer.Equals(x, x);
    //  Assert.IsTrue(xx, $"{title}: Equals({x}, {x}) failed");

    //  var yy = equalityComparer.Equals(y, y);
    //  Assert.IsTrue(yy, $"{title}: Equals({y}, {y}) failed");

    //  var xy = equalityComparer.Equals(x, y);
    //  Assert.AreEqual(expected, xy, $"{title}: Equals({x}, {y}) failed");

    //  var yx = equalityComparer.Equals(y, x);
    //  Assert.AreEqual(expected, yx, $"{title}: Equals({y}, {x}) failed");

    //  if(x is { }) {
    //    var xhash1 = equalityComparer.GetHashCode(x);
    //    var xhash2 = equalityComparer.GetHashCode(x);
    //    Assert.IsTrue(xhash1 == xhash2, $"{title}: GetHashCode({x}) is not stable: {xhash1}, {xhash2}");
    //  }//if

    //  if(y is { }) {
    //    var yhash1 = equalityComparer.GetHashCode(y);
    //    var yhash2 = equalityComparer.GetHashCode(y);
    //    Assert.IsTrue(yhash1 == yhash2, $"{title}: GetHashCode({y}) is not stable: {yhash1}, {yhash2}");
    //  }//if

    //  if(expected && x is { } && y is { }) {
    //    var xhash = equalityComparer.GetHashCode(x);
    //    var yhash = equalityComparer.GetHashCode(y);
    //    Assert.IsTrue(xhash == yhash, $"{title}: GetHashCode({x}) is not symmetric: {xhash}, {yhash}");
    //  }//if
    //}

    //public enum CompareResult
    //{
    //  Equal,
    //  LessThan,
    //  GreaterThan,
    //}

    //private static void TestComparer<T>(string title, [MaybeNull] T x, [MaybeNull] T y, CompareResult expected, IComparer<T> comparer) {
    //  var xx = comparer.Compare(x, x);
    //  Assert.AreEqual(CompareResult.Equal, GetCompareResult(xx), $"{title}: Compare({x}, {x}) failed {xx}");

    //  var yy = comparer.Compare(y, y);
    //  Assert.AreEqual(CompareResult.Equal, GetCompareResult(yy), $"{title}: Compare({y}, {y}) failed {yy}");

    //  var xy = comparer.Compare(x, y);
    //  Assert.AreEqual(expected, GetCompareResult(xy), $"Compare({x}, {y}) failed ({xy})");

    //  var yx = comparer.Compare(y, x);
    //  Assert.AreEqual(ReverseCompareResult(expected), GetCompareResult(yx), $"Compare({y}, {x}) failed ({yx})");

    //  static CompareResult GetCompareResult(int value) => value < 0 ? CompareResult.LessThan : (value > 0 ? CompareResult.GreaterThan : CompareResult.Equal);

    //  static CompareResult ReverseCompareResult(CompareResult value) => value switch
    //  {
    //    CompareResult.LessThan => CompareResult.GreaterThan,
    //    CompareResult.GreaterThan => CompareResult.LessThan,
    //    _ => CompareResult.Equal,
    //  };
    //}

    class XObject
    {
      public string Text { get; set; } = String.Empty;
      public YObject? Y { get; set; }
      public override string ToString() => $"\"{Text}\", {{{Y}}}";
    }

    class YObject
    {
      public int Value { get; set; }
      public XObject? X { get; set; }
      public override string ToString() => $"[{Value}], {{{X}}}";
    }

    [TestMethod]
    public void EqualsNullableProperty() {
      var comparer = new ComparerBuilder<TestValue>(new Interception())
        .Add(value => value.DateTime)
        .CreateEqualityComparer();

      var null1 = new TestValue(dateTime: null);
      var null2 = new TestValue(dateTime: null);
      Assert.IsTrue(comparer.Equals(null1, null2), "Nulls should be equal");
      Assert.AreEqual(comparer.GetHashCode(null1), comparer.GetHashCode(null2), "Hash codes on nulls should be equal");

      var value1 = new TestValue(dateTime: new DateTime(2015, 01, 01));
      var value2 = new TestValue(dateTime: new DateTime(2015, 02, 01));
      Assert.IsFalse(comparer.Equals(value1, null1), "Non-null and null should not be equal");
      Assert.IsFalse(comparer.Equals(null1, value1), "Null and non-null should not be equal");

      Assert.IsFalse(comparer.Equals(null1, value2), "Varied values should not be equal");
    }

    [TestMethod]
    public void CompareNullableProperty() {
      var comparer = new ComparerBuilder<TestValue>()
        .Add(value => value.DateTime)
        .CreateComparer();
      var items = new[] {
        new TestValue(dateTime: null),
        new TestValue(dateTime: new DateTime(2015, 01, 01)),
        new TestValue(dateTime: null),
        new TestValue(dateTime: new DateTime(2015, 02, 01)),
        new TestValue(dateTime: null),
        new TestValue(dateTime: new DateTime(2015, 03, 01)),
        new TestValue(dateTime: null),
      };
      Array.Sort(items, comparer);

      CollectionAssert.AllItemsAreNotNull(items);
      var previous = default(TestValue);
      foreach(var item in items) {
        if(previous != null) {
          Assert.IsTrue(previous.DateTime is null || previous.DateTime <= item.DateTime,
            $"previous.Test2 is null || previous.Test2 [{previous.DateTime}] <= item.Test2 [{item.DateTime}]");
        }//if
        previous = item;
      }//for
    }

    private sealed class Interception : IComparerBuilderInterception
    {
      public int InterceptCompare<T>(int value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => throw new NotImplementedException();
      public bool InterceptEquals<T>(bool value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => throw new NotImplementedException();
      public int InterceptGetHashCode<T>(int value, T obj, ComparerBuilderInterceptionArgs<T> args) => throw new NotImplementedException();

      int IComparerBuilderInterception.InterceptCompare<T>(int value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => value;
      bool IComparerBuilderInterception.InterceptEquals<T>(bool value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => value;
      int IComparerBuilderInterception.InterceptGetHashCode<T>(int value, T obj, ComparerBuilderInterceptionArgs<T> args) => value;
    }
  }
}
