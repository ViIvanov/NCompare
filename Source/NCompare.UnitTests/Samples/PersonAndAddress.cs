using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests.Samples;

using static TestCompare;
using static PersonComparators;

[TestClass]
public sealed class PersonAndAddress
{
  [TestMethod]
  public void PersonComparators() {
    var person1 = new Person {
      Name = "Alex",
      BirthDate = new DateTime(2000, 01, 01),
    };

    var person2 = new Person {
      Name = "Bob",
      BirthDate = new DateTime(2001, 02, 02),
      BestFriend = person1,
    };

    var person3 = new Person {
      Name = "Bob",
      BirthDate = new DateTime(2001, 02, 02),
      BestFriend = person2,
    };

    var person4 = new Person {
      Name = "Bob",
      BirthDate = new DateTime(2001, 02, 02),
      BestFriend = person2,
    };

    TestComparers("1 and 2", person1, person2, CompareResult.LessThan, PersonComparerBuilder);
    TestComparers("2 and 3", person2, person3, CompareResult.LessThan, PersonComparerBuilder);
    TestComparers("3 and 1", person3, person1, CompareResult.GreaterThan, PersonComparerBuilder);
    TestComparers("3 and 4", person3, person4, CompareResult.Equal, PersonComparerBuilder);
  }
}
