using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests
{
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
  }
}
