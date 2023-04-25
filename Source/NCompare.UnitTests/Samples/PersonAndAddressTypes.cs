using System;
using System.Collections.Generic;

namespace NCompare.UnitTests.Samples;

internal sealed class Person
{
  public required string Name { get; set; } = String.Empty;
  public required DateTime BirthDate { get; set; }

  public Person? BestFriend { get; set; }

  public Address? Address { get; set; }

  public override string ToString() => (Name, BirthDate, $"[{BestFriend}]", $"[{Address}]").ToString();
}

internal sealed class Address
{
  public required string Line1 { get; set; } = String.Empty;
  public string Line2 { get; set; } = String.Empty;
  public required string PostalCode { get; set; } = String.Empty;

  public Person? Owner { get; set; }

  public override string ToString() => Line1;
}

internal static class PersonComparators
{
  static PersonComparators() {
    PersonComparerBuilder.Add(item => item.Name, StringComparer.OrdinalIgnoreCase); // Use `OrdinalIgnoreCase` comparer for both, equality and sorting
    PersonComparerBuilder.Add(item => item.BirthDate.Date); // Compare only `Date` part with default comparators (for equality and sorting)
    PersonComparerBuilder.Add(item => item.BestFriend); // Use implicit same comparer builder to compare value of the same type
    PersonComparerBuilder.Add(item => item.Address, AddressComparerBuilder); // Use another (even not initialized) comparer builder instead of comparators

    AddressComparerBuilder.Add(item => $"{item.Line1}{Environment.NewLine}{item.Line2}", StringComparer.InvariantCultureIgnoreCase); // Compare both lines at once
    AddressComparerBuilder.Add(item => item.PostalCode);
    AddressComparerBuilder.Add(item => item.Owner, PersonComparerBuilder); // Use another comparer builder instead of comparators

    PersonEqualityComparer = PersonComparerBuilder.CreateEqualityComparer();
    PersonComparer = PersonComparerBuilder.CreateComparer();

    AddressEqualityComparer = AddressComparerBuilder.CreateEqualityComparer();
    AddressComparer = AddressComparerBuilder.CreateComparer();
  }

  internal static ComparerBuilder<Person> PersonComparerBuilder { get; } = new ComparerBuilder<Person>();
  internal static ComparerBuilder<Address> AddressComparerBuilder { get; } = new ComparerBuilder<Address>();

  public static EqualityComparer<Person> PersonEqualityComparer { get; }
  public static Comparer<Person> PersonComparer { get; }

  public static EqualityComparer<Address> AddressEqualityComparer { get; }
  public static Comparer<Address> AddressComparer { get; }
}
