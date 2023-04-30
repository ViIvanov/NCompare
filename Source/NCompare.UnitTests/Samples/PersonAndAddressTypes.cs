﻿using System.Diagnostics;
using System.Numerics;

namespace NCompare.UnitTests.Samples;

using static PersonAndAddressComparators;

public sealed partial class Person
{
  public string Name { get; set; } = String.Empty;
  public DateTime? BirthDate { get; set; }

  public Person? BestFriend { get; set; }

  public Address? Address { get; set; }

  public override string ToString() => $"\"{Name}\", #{BirthDate:yyyy-MM-dd}#, [{BestFriend}], [{Address}]";
}

partial class Person : IEquatable<Person>, IComparable<Person>, IEqualityOperators<Person?, Person?, bool>, IComparisonOperators<Person?, Person?, bool>
{
  public bool Equals(Person? other) => PersonEqualityComparer.Equals(this, other);
  public int CompareTo(Person? other) => PersonComparer.Compare(this, other);

  public override bool Equals(object? obj) => obj is Person other && Equals(other);
  public override int GetHashCode() => PersonEqualityComparer.GetHashCode(this);

  public static bool operator ==(Person? left, Person? right) => Equals(left, right);
  public static bool operator !=(Person? left, Person? right) => !(left == right);

  public static bool operator <(Person? left, Person? right) => right is not null && right.CompareTo(left) > 0;
  public static bool operator <=(Person? left, Person? right) => !(right < left);
  public static bool operator >(Person? left, Person? right) => right < left;
  public static bool operator >=(Person? left, Person? right) => !(left < right);
}

public sealed partial class Address
{
  public required string Line1 { get; set; } = String.Empty;
  public string Line2 { get; set; } = String.Empty;
  public required string PostalCode { get; set; } = String.Empty;

  public Person? Owner { get; set; }

  public override string ToString() => $"{PostalCode}, [{Owner}], {Line1}|{Line2}";
}

partial class Address : IEquatable<Address>, IComparable<Address>, IEqualityOperators<Address?, Address?, bool>, IComparisonOperators<Address?, Address?, bool>
{
  public bool Equals(Address? other) => AddressEqualityComparer.Equals(this, other);
  public int CompareTo(Address? other) => AddressComparer.Compare(this, other);

  public override bool Equals(object? obj) => obj is Address other && Equals(other);
  public override int GetHashCode() => AddressEqualityComparer.GetHashCode(this);

  public static bool operator ==(Address? left, Address? right) => Equals(left, right);
  public static bool operator !=(Address? left, Address? right) => !(left == right);

  public static bool operator <(Address? left, Address? right) => right is not null && right.CompareTo(left) > 0;
  public static bool operator <=(Address? left, Address? right) => !(right < left);
  public static bool operator >(Address? left, Address? right) => right < left;
  public static bool operator >=(Address? left, Address? right) => !(left < right);
}

internal static class PersonAndAddressComparators
{
  static PersonAndAddressComparators() {
    PersonComparerBuilder = new ComparerBuilder<Person>(new DebugInterception());
    AddressComparerBuilder = new ComparerBuilder<Address>(new DebugInterception());

    PersonComparerBuilder.Add(item => item.Name, StringComparer.OrdinalIgnoreCase); // Use `OrdinalIgnoreCase` comparer for both, equality and sorting
    PersonComparerBuilder.Add(item => item.BirthDate != null ? item.BirthDate.Value.Date : default(DateTime?)); // Compare only `Date` part with default comparators (for equality and sorting)
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

  internal static ComparerBuilder<Person> PersonComparerBuilder { get; }
  internal static ComparerBuilder<Address> AddressComparerBuilder { get; }

  public static EqualityComparer<Person> PersonEqualityComparer { get; }
  public static Comparer<Person> PersonComparer { get; }

  public static EqualityComparer<Address> AddressEqualityComparer { get; }
  public static Comparer<Address> AddressComparer { get; }
}

internal sealed class DebugInterception : IComparerBuilderInterception
{
  public int InterceptCompare<T>(int value, T x, T y, ComparerBuilderInterceptionArgs<T> args) {
    Debug.Print($"{nameof(InterceptCompare)}<{typeof(T).FullName}>({nameof(value)}: {value}, {nameof(x)}: {x}, {nameof(y)}: {y}, \"{args.ExpressionText}\")");
    return value;
  }

  public bool InterceptEquals<T>(bool value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => value;
  public int InterceptGetHashCode<T>(int value, T obj, ComparerBuilderInterceptionArgs<T> args) => value;
}
