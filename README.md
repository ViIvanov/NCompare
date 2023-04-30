# NCompare

[![NuGet version (NCompare)](https://img.shields.io/nuget/v/NCompare.svg?style=flat-square)](https://www.nuget.org/packages/NCompare/)

The library provides the `ComparerBuilder<T>` type, which helps to create comparators (equality and sort comparators) for comparing objects of type `T`.

`ComparerBuilder<T>` helps to compare complex types. It can create `EqualityComparer<>` and/or a `Comparer<>` objects for your types.
You just need to "add" expressions, that describe what data you want compare and, optionally, comparators - to specify how to compare that data.

The main goal of having this type is to set once and uniformly according to what rules
you need to compare objects, and after that be able
to get both `IEqualityComparer<T>` there and `IComparer<T>`.

You can find reasons for making `ComparerBuilder<>` in this old post on [RSDN](http://rsdn.ru/forum/src/3914421.1) (in Russian).

In the future, I plan to expand the library with comparators for various collections.
This will allow you to quickly and conveniently create comparators for complex and interconnected objects.

Solution includes benchmarks project, that compares comparators, created by this library
with manually implemented comparison in types, manually written comparators and other compare builders.
Adjusted for the correctness of my benchmarks, they show that the comparators built by `ComparerBuilder<>`
usually work no more than twice as slow as those implemented manually.

Now `ComparerBuilder<T>` is written on C# 11.0. it builds for .NET Framework 4.6.1 and .NET Standard 2.0 so could
be used in many projects.

You can compose a comparer builder from other comparer builders.
Also, it allows to intercept compare methods and, for example,
print to a `Debug` when `IEqualityComparer<>::Equals(…, …)` returns `false` or `IComparer<>::Compare(…)` returns not `0`.
"Interception" is helpful in a debugging scenarios.

## Examples

Let's imagine that we need to add comparison logic to the Person type below:
```cs
public sealed class Person
{
  public string Name { get; set; } = String.Empty;
  public DateTime? BirthDate { get; set; }

  public override string ToString() => $"\"{Name}\", #{BirthDate:yyyy-MM-dd}#";
}
```
This can be done like this:
```cs
internal static class PersonComparison
{
  internal static ComparerBuilder<Person> PersonComparerBuilder { get; } = new ComparerBuilder<Person>() // Creating an instance of the builder
    // Add comparison for `Name` property. Using `OrdinalIgnoreCase` comparer for both, equality and sorting.
    // Custom `IEqualityComparer<>` and `IComparer<>` also could be provided, one of them or both.
    .Add(item => item.Name, StringComparer.OrdinalIgnoreCase)
    // Compare only `Date` part with default comparators (for equality and sorting)
    .Add(item => item.BirthDate != null ? item.BirthDate.Value.Date : default(DateTime?));

  // Actually, build comparators
  public static EqualityComparer<Person> PersonEqualityComparer { get; } = PersonComparerBuilder.CreateEqualityComparer();
  public static Comparer<Person> PersonComparer { get; } = PersonComparerBuilder.CreateComparer();
}
```
Results:
```cs
[TestMethod]
public void PersonComparisonTests() {
  var person1 = new Person { Name = "Alex", BirthDate = new DateTime(2000, 01, 01, 07, 00, 00), };
  var person2 = new Person { Name = "aleX", BirthDate = new DateTime(2000, 01, 01, 16, 30, 00), };
  var person3 = new Person { Name = "Bob", };

  Assert.AreEqual(person1, person2, PersonComparison.PersonEqualityComparer); // The first two are equal
  Assert.AreNotEqual(person2, person3, PersonComparison.PersonEqualityComparer); // The third is different

  var compare12 = PersonComparison.PersonComparer.Compare(person1, person2);
  Assert.AreEqual(expected: 0, compare12);

  var compare23 = PersonComparison.PersonComparer.Compare(person2, person3);
  Assert.IsTrue(compare23 < 0);
}
```

### More complicated types

Now let's complicate our type:

```cs
// Declaring types as `partial` because we will define comparison interfaces and operators for these types below on other parts.

public sealed partial class Person
{
  public string Name { get; set; } = String.Empty;
  public DateTime? BirthDate { get; set; }

  public Person? BestFriend { get; set; } // Reference to another `Person` instance as a best friend

  public Address? Address { get; set; } // `Person`'s address

  public override string ToString() => $"\"{Name}\", #{BirthDate:yyyy-MM-dd}#, [{BestFriend}], [{Address}]";
}

public sealed partial class Address
{
  public required string Line1 { get; set; } = String.Empty;
  public string Line2 { get; set; } = String.Empty;
  public required string PostalCode { get; set; } = String.Empty;

  public Person? Owner { get; set; } // Reference to a `Person`, that is owner of this address

  public override string ToString() => $"{PostalCode}, [{Owner}], {Line1}|{Line2}";
}
```

To be able to build comparators for `Person` and `Address` now we need to move the initialization of comparer builder's
into a static constructor because instances contain references to each other:

```cs
internal static class PersonAndAddressComparison
{
  static PersonAndAddressComparison() {
    // At the first, initializing comparer builder for `Person`
    PersonComparerBuilder.Add(item => item.Name, StringComparer.OrdinalIgnoreCase);
    PersonComparerBuilder.Add(item => item.BirthDate != null ? item.BirthDate.Value.Date : default(DateTime?));

    // Using implicitly the same comparer builder to compare value of the same type
    PersonComparerBuilder.Add(item => item.BestFriend);

    // Using another (even not fully initialized) comparer builder
    PersonComparerBuilder.Add(item => item.Address, AddressComparerBuilder);

    // Now we can initialize comparer builder for `Address`

    // Compare both lines at once
    AddressComparerBuilder.Add(item => $"{item.Line1}{Environment.NewLine}{item.Line2}", StringComparer.InvariantCultureIgnoreCase);
    AddressComparerBuilder.Add(item => item.PostalCode);

    // Use another comparer builder instead of comparators
    AddressComparerBuilder.Add(item => item.Owner, PersonComparerBuilder);

    // Now we can build comparators

    PersonEqualityComparer = PersonComparerBuilder.CreateEqualityComparer();
    PersonComparer = PersonComparerBuilder.CreateComparer();

    AddressEqualityComparer = AddressComparerBuilder.CreateEqualityComparer();
    AddressComparer = AddressComparerBuilder.CreateComparer();
  }

  internal static ComparerBuilder<Person> PersonComparerBuilder { get; } = new();
  internal static ComparerBuilder<Address> AddressComparerBuilder { get; } = new();

  public static EqualityComparer<Person> PersonEqualityComparer { get; }
  public static Comparer<Person> PersonComparer { get; }

  public static EqualityComparer<Address> AddressEqualityComparer { get; }
  public static Comparer<Address> AddressComparer { get; }
}
```

After we have comparators for types, we can define interfaces and operators for comparison:
```cs
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
```
