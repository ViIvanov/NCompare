// Ignore Spelling: Nullable

namespace NCompare.UnitTests;

using static TestCompare;

[TestClass]
public sealed partial class General
{
  private static void AssertException<TException>(Action action, string message) where TException : Exception {
    var ex = Assert.Throws<TException>(action);
    Assert.IsNotNull(ex);
    Assert.AreEqual(expected: message, ex.Message);
  }

  #region Regular Tests

  [TestMethod]
  public void IsEmpty() {
    var builder = new ComparerBuilder<TestValue>();
    Assert.IsTrue(builder.IsEmpty, $"{nameof(builder.IsEmpty)} should be true after object creation.");
    Assert.IsNull(builder.Interception, $"{nameof(builder.Interception)} is not null: {builder.Interception}.");
    AssertException(() => builder.CreateEqualityComparer());
    AssertException(() => builder.CreateComparer());

    static void AssertException(Action action) {
      const string Message = "There are no expressions specified.";
      AssertException<InvalidOperationException>(action, Message);
    }
  }

  [TestMethod]
  public void NotEmpty() {
    var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);

    Assert.IsFalse(builder.IsEmpty, $"{nameof(builder.IsEmpty)} should be false after object creation.");

    var equalityComparer = builder.CreateEqualityComparer();
    Assert.IsNotNull(equalityComparer, $"{nameof(builder.CreateEqualityComparer)}() returned null");

    var comparer = builder.CreateComparer();
    Assert.IsNotNull(comparer, $"{nameof(builder.CreateComparer)}() returned null");
  }

  [TestMethod]
  public void IsFrozen() {
    Verify(builder => builder.CreateEqualityComparer());
    Verify(builder => builder.CreateComparer());

    static void Verify<T>(Func<ComparerBuilder<TestValue>, T> create) {
      var builder = new ComparerBuilder<TestValue>().Add(static item => item.Number);
      Assert.IsFalse(builder.IsFrozen, $"{nameof(builder.IsFrozen)} should be false before creating comparator {typeof(T)}.");
      _ = create(builder);
      Assert.IsTrue(builder.IsFrozen, $"{nameof(builder.IsFrozen)} should be true after creating comparator {typeof(T)}.");
      AssertException<InvalidOperationException>(() => builder.Add(static item => item.DateTime), "Comparer(s) already created. It is not possible to modify created comparer(s).");
    }
  }

  [TestMethod]
  public void ThrowIfCreatedEqualityComparer() {
    var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
    _ = builder.CreateEqualityComparer();
    AssertThrowIfCreated(builder);
  }

  [TestMethod]
  public void ThrowIfCreatedComparer() {
    var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
    _ = builder.CreateComparer();
    AssertThrowIfCreated(builder);
  }

  private static void AssertThrowIfCreated<T>(ComparerBuilder<T> builder) where T : notnull {
    AssertException(() => builder.Add(item => item.ToString()));
    AssertException(() => builder.Add(item => item.ToString(), EqualityComparer<string?>.Default, Comparer<string?>.Default));
    AssertException(() => builder.Add(item => item.ToString(), Comparer<string?>.Default));
    AssertException(() => builder.Add(item => item.ToString(), StringComparer.Ordinal));
    AssertException(() => builder.Add(item => item));
    AssertException(() => builder.Add(item => item.ToString(), new ComparerBuilder<string?>()));
    AssertException(() => builder.Add(builder));

    static void AssertException(Action action) {
      const string Message = "Comparer(s) already created. It is not possible to modify created comparer(s).";
      AssertException<InvalidOperationException>(action, Message);
    }
  }

  [TestMethod]
  public void SimpleEqualityComparer() {
    var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
    var equality = builder.CreateEqualityComparer();

    var x = new TestValue(1);
    var y = new TestValue(1);
    Assert.AreEqual(x, y, equality, $"{x.Number} == {y.Number}");

    var z = new TestValue(2);
    Assert.AreNotEqual(z, y, equality, $"{z.Number} != {y.Number}");
  }

  [TestMethod]
  public void SimpleComparer() {
    var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
    var equality = builder.CreateComparer();

    var x = new TestValue(1);
    var y = new TestValue(1);
    var compare = equality.Compare(x, y);
    Assert.AreEqual(expected: 0, compare, $"{x.Number} == {y.Number}");

    var z = new TestValue(2);
    Assert.IsLessThan(0, equality.Compare(x, z), $"{x.Number} < {z.Number}");
    Assert.IsGreaterThan(0, equality.Compare(z, y), $"{z.Number} < {y.Number}");
  }

  [TestMethod]
  public void ComplexComparators() {
    var x = new TestValue(1);
    var y = new TestValue(1);
    var z = new TestValue(2);

    var builder = new ComparerBuilder<TestValue>().Add(item => item.Number);
    TestComparators("Equal TestValues", x, y, CompareResult.Equal, builder);
    TestComparators("Not equal TestValues", x, z, CompareResult.LessThan, builder);
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
      .ConvertTo<DerivedTestValue>()
      .Add(value => value.Other, builderOtherValue!);

    var data1 = new DerivedTestValue(2, DateTime.Now, new OtherValue("M"));
    var data2 = new DerivedTestValue(4, DateTime.Now, new OtherValue("m"));
    var data3 = new DerivedTestValue(6, DateTime.Now, new OtherValue("a"));
    var data4 = new DerivedTestValue(1, null, new OtherValue("b"));

    TestComparators("Compare 1 and 2", data1, data2, expected: CompareResult.Equal, builderData);
    TestComparators("Compare 2 and 3", data2, data3, expected: CompareResult.GreaterThan, builderData);
    TestComparators("Compare 1 and 4", data1, data4, expected: CompareResult.LessThan, builderData);
  }

  [TestMethod]
  public void EqualsNullableProperty() {
    var comparer = new ComparerBuilder<TestValue>()
      .Add(value => value.DateTime)
      .CreateEqualityComparer();

    var null1 = new TestValue(dateTime: null);
    var null2 = new TestValue(dateTime: null);
    TestEqualityComparer("Nulls should be equal", null1, null2, expected: true, comparer);

    var value1 = new TestValue(dateTime: new DateTime(2015, 01, 01));
    TestEqualityComparer("Compare non-null and null", value1, null1, expected: false, comparer);

    var value2 = new TestValue(dateTime: new DateTime(2015, 01, 01));
    TestEqualityComparer("Compare same values", value1, value2, expected: true, comparer);

    var value3 = new TestValue(dateTime: new DateTime(2015, 02, 01));
    TestEqualityComparer("Compare different values", value1, value3, expected: false, comparer);
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

  [TestMethod]
  public void ThrowOnUncomparableType() {
    var builder = new ComparerBuilder<TestValue>().Add(value => new object());
    _ = Assert.ThrowsExactly<ArgumentException>(builder.CreateComparer);
  }
}

file class TestValue
{
  public TestValue(int number = default, DateTime? dateTime = default) => (Number, DateTime) = (number, dateTime);

  public int Number { get; }
  public DateTime? DateTime { get; }

  public override string ToString() => $"{nameof(Number)} = {Number}, {nameof(DateTime)} = {DateTime:yyyy-MM-dd}";
}

file sealed class OtherValue(string? text = default)
{
  public string Text { get; } = text ?? String.Empty;

  public override string ToString() => $"\"{Text}\"";
}

file sealed class DerivedTestValue(int number = default, DateTime? dateTime = default, OtherValue? other = default) : TestValue(number, dateTime)
{
  public OtherValue? Other { get; } = other;

  public override string ToString() => $"{base.ToString()}, {nameof(Other)} = {Other}";
}
