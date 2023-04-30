using System.Numerics;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests.Samples;

using static TestCompare;
using static PersonAndAddressComparison;

[TestClass]
public sealed class PersonAndAddress
{
  [TestMethod]
  public void PersonComparisonTests() {
    var person1 = new Person {
      Name = "Alex",
      BirthDate = new DateTime(2000, 01, 01, 07, 00, 00),
    };

    var person2 = new Person {
      Name = "alex",
      BirthDate = new DateTime(2000, 01, 01, 16, 30, 00),
    };

    var person3 = new Person {
      Name = "Bob",
    };

    Assert.AreEqual(person1, person2, PersonEqualityComparer);
    Assert.AreNotEqual(person2, person3, PersonEqualityComparer);

    var compare12 = PersonComparer.Compare(person1, person2);
    Assert.AreEqual(expected: 0, compare12);

    var compare23 = PersonComparer.Compare(person2, person3);
    Assert.IsTrue(compare23 < 0);
  }

  [TestMethod]
  public void PersonComparators() {
    var friend = new Person {
      Name = "Friend",
      BirthDate = new DateTime(2002, 02, 02),
      Address = new() {
        Line1 = "New Line",
        PostalCode = "00567",
      },
    };

    var owner = new Person {
      Name = "Owner",
      BirthDate = new DateTime(2003, 02, 02),
      BestFriend = friend,
    };

    var person1 = new Person {
      Name = "Alex",
      BirthDate = new DateTime(2000, 01, 01),
      Address = new() {
        Line1 = "Line",
        PostalCode = "00123",
        Owner = owner,
      },
    };

    var person2 = new Person {
      Name = "Bob",
      BirthDate = new DateTime(2001, 02, 02),
      BestFriend = person1,
      Address = new() {
        Line1 = "Line",
        PostalCode = "00123",
        Owner = owner,
      },
    };

    var person3 = new Person {
      Name = "Bob",
      BirthDate = new DateTime(2001, 02, 02),
      BestFriend = person2,
      Address = new() {
        Line1 = "Line",
        PostalCode = "00123",
        Owner = owner,
      },
    };

    var person4 = new Person {
      Name = "Bob",
      BirthDate = new DateTime(2001, 02, 02),
      BestFriend = person2,
      Address = new() {
        Line1 = "Line",
        PostalCode = "00123",
        Owner = owner,
      },
    };

    TestComparers("1 and 2", person1, person2, CompareResult.LessThan, PersonComparerBuilder);
    TestComparers("2 and 3", person2, person3, CompareResult.LessThan, PersonComparerBuilder);
    TestComparers("3 and 1", person3, person1, CompareResult.GreaterThan, PersonComparerBuilder);
    TestComparers("3 and 4", person3, person4, CompareResult.Equal, PersonComparerBuilder);
  }

  [TestMethod]
  public void PersonOperators() {
    EqualityOperators<Person>(default, default);
    EqualityOperators<Person>(new(), default);
    EqualityOperators<Person>(new(), new());

    ComparisonOperators<Person>(default, default);
    ComparisonOperators<Person>(new(), default);
    ComparisonOperators<Person>(default, new());
    ComparisonOperators<Person>(new(), new());

    var x = new Person { Name = "Left", };
    var y = new Person { Name = "Left", };
    var z = new Person { Name = "Right", };
    var w = new Person { Name = "Right", };
    var o = new Person { Name = "Z", };

    var a = new Address { Line1 = "A", PostalCode = "1", Owner = z, };
    var b = new Address { Line1 = "A", PostalCode = "1", Owner = w, };
    var c = new Address { Line1 = "A", PostalCode = "1", Owner = o, };

    x.Address = a;
    y.Address = b;
    z.Address = c;

    EqualityOperators(x, y);
    EqualityOperators(x, z);
    ComparisonOperators(x, y);
    ComparisonOperators(x, z);
    ComparisonOperators(y, z);
  }

  public static void EqualityOperators<T>(T? left, T? right) where T : IEqualityOperators<T?, T?, bool> {
    var result = EqualityComparer<T>.Default.Equals(left, right);

    Assert.AreEqual(expected: result, left == right, OperatorAssertMessage("==", left, right, result));
    Assert.AreEqual(expected: result, right == left, OperatorAssertMessage("==", right, left, result));
    Assert.AreEqual(expected: !result, left != right, OperatorAssertMessage("!=", left, right, result));
    Assert.AreEqual(expected: !result, right != left, OperatorAssertMessage("!=", right, left, result));
  }

  public static void ComparisonOperators<T>(T? left, T? right) where T : IComparisonOperators<T?, T?, bool> {
    var result = Comparer<T>.Default.Compare(left, right);

    Assert.AreEqual(expected: result < 0, left < right, OperatorAssertMessage("<", left, right, result));
    Assert.AreEqual(expected: result <= 0, left <= right, OperatorAssertMessage("<=", left, right, result));
    Assert.AreEqual(expected: result > 0, left > right, OperatorAssertMessage(">", left, right, result));
    Assert.AreEqual(expected: result >= 0, left >= right, OperatorAssertMessage(">=", left, right, result));

    Assert.AreEqual(expected: result > 0, right < left, OperatorAssertMessage("<", right, left, result));
    Assert.AreEqual(expected: result >= 0, right <= left, OperatorAssertMessage("<=", right, left, result));
    Assert.AreEqual(expected: result < 0, right > left, OperatorAssertMessage(">", right, left, result));
    Assert.AreEqual(expected: result <= 0, right >= left, OperatorAssertMessage(">=", right, left, result));
  }

  private static string OperatorAssertMessage<T, TResult>(string @operator, T left, T right, TResult result,
    [CallerArgumentExpression(nameof(left))] string? leftName = null, [CallerArgumentExpression(nameof(right))] string? rightName = null)
    => $"[{leftName} {@operator} {rightName} is {result}]: {{{left?.ToString() ?? "<null>"}}} {@operator} {{{right?.ToString() ?? "<null>"}}}";
}
