# NCompare

[![NuGet version (NCompare)](https://img.shields.io/nuget/v/NCompare.svg?style=flat-square)](https://www.nuget.org/packages/NCompare/)
[![Build Status](https://dev.azure.com/viivanov/NCompare/_apis/build/status%2FViIvanov.NCompare?branchName=main)](https://dev.azure.com/viivanov/NCompare/_build/latest?definitionId=7&branchName=main)

The library provides the `ComparerBuilder<T>` type, which helps to create comparators (equality and sort comparators) for comparing objects of type `T`.

`ComparerBuilder<T>` helps to compare complex types. It can create `EqualityComparer<>` and/or a `Comparer<>` objects for your types.
You just need to "add" expressions (as `System.Linq.Expressions`), that describe what data you want compare and,
optionally, comparators - to specify how to compare that data.

When implementing equality comparison operations in a type, the user is usually forced to duplicate the comparison logic
(for example, enumerate the same properties and/or fields) in the `Equals` and `GetHashCode` implementations.

If we add the implementation of `IComparable<>`, then we will have to duplicate the "comparison logic"
(enumerating parts, that should be compared and how) again for consistency.

The main goal of having this type is to set once and uniformly according to what rules
you need to compare objects, and after that be able
to get both `IEqualityComparer<T>` there and `IComparer<T>`.

You can find reasons for making `ComparerBuilder<>` in this old post on [RSDN](http://rsdn.ru/forum/src/3914421.1) (in Russian).

In the future, I plan to expand the library with comparators for various collections and other scenarios.
This will allow you to quickly and conveniently create comparators for complex and interconnected objects.

Now `ComparerBuilder<T>` is written on C# 14.0. it builds for .NET Framework 4.6.2 and .NET Standard 2.0 so could
be used in many projects.

You can compose a comparer builder from other comparer builders.
Also, it allows to intercept compare methods and, for example,
print to a `Debug` when `IEqualityComparer<>::Equals(…, …)` returns `false` or `IComparer<>::Compare(…)` returns not `0`.
"Interception" is helpful in a debugging scenarios.

## Examples

Let's imagine that we need to add comparison logic to the `Person` type below:
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

## Benchmarks

The solution includes a benchmarks project, that compares the performance of comparators, created by this library
with manually implemented comparisons, manually written comparators, and other comparer builders.
Adjusted for the correctness of my benchmarks, they show that the comparators built by `ComparerBuilder<>`
usually work no more than four as slow as those implemented manually.

<details>
  <summary>Expand for benchmarks</summary>

#### Legend

* Type Kind: The type of reference or value is being compared
* Operation: The method being evaluated
* Comparer: The comparer used to compare objects
  * `IEquatable<T>` / `IComparable<T>` - handwritten in-place implementation of comparison
  * `EqualityComparer<T>` / `Comparer<T>` - handwritten custom comparer
  * `ComparerBuilder<T>` - comparators, created by this library
  * `IFullComparer<T>` - comparator, created by [Nito.Comparers](https://github.com/StephenCleary/Comparers) library

Runtime: .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

| Type Kind |   Operation |              Comparer |          Mean |      StdErr |        StdDev | Ratio |
|---------- |------------ |---------------------- |--------------:|------------:|--------------:|------:|
|   `class` |      Equals |       `IEquatable<T>` |      4.063 ns |   0.0318 ns |     0.2271 ns |  1.00 |
|   `class` |      Equals | `EqualityComparer<T>` |      4.548 ns |   0.0256 ns |     0.1556 ns |  1.12 |
|   `class` |      Equals |  `ComparerBuilder<T>` |      9.388 ns |   0.0542 ns |     0.2235 ns |  2.23 |
|   `class` |      Equals |    `IFullComparer<T>` |     45.409 ns |   0.2435 ns |     1.1929 ns | 11.00 |
|           |             |                       |               |             |               |       |
|  `struct` |      Equals |       `IEquatable<T>` |      2.529 ns |   0.0162 ns |     0.0605 ns |  1.00 |
|  `struct` |      Equals | `EqualityComparer<T>` |      4.017 ns |   0.0312 ns |     0.1898 ns |  1.62 |
|  `struct` |      Equals |  `ComparerBuilder<T>` |      8.969 ns |   0.0587 ns |     0.3759 ns |  3.47 |
|  `struct` |      Equals |    `IFullComparer<T>` |     48.289 ns |   0.0611 ns |     0.2116 ns | 19.11 |
|           |             |                       |               |             |               |       |
|   `class` | GetHashCode |       `IEquatable<T>` |     10.551 ns |   0.0222 ns |     0.0861 ns |  1.00 |
|   `class` | GetHashCode | `EqualityComparer<T>` |     13.095 ns |   0.0947 ns |     0.9467 ns |  1.25 |
|   `class` | GetHashCode |  `ComparerBuilder<T>` |     11.133 ns |   0.0634 ns |     0.3169 ns |  1.05 |
|   `class` | GetHashCode |    `IFullComparer<T>` |     35.578 ns |   0.0959 ns |     0.3716 ns |  3.37 |
|           |             |                       |               |             |               |       |
|  `struct` | GetHashCode |       `IEquatable<T>` |     10.935 ns |   0.0656 ns |     0.3214 ns |  1.00 |
|  `struct` | GetHashCode | `EqualityComparer<T>` |     13.053 ns |   0.0728 ns |     0.3258 ns |  1.19 |
|  `struct` | GetHashCode |  `ComparerBuilder<T>` |     12.951 ns |   0.0808 ns |     0.5235 ns |  1.19 |
|  `struct` | GetHashCode |    `IFullComparer<T>` |     36.684 ns |   0.2175 ns |     1.6276 ns |  3.30 |
|           |             |                       |               |             |               |       |
|   `class` |   CompareTo |      `IComparable<T>` |      7.013 ns |   0.0502 ns |     0.4196 ns |  1.00 |
|   `class` |   CompareTo |         `Comparer<T>` |      6.519 ns |   0.0440 ns |     0.3081 ns |  0.92 |
|   `class` |   CompareTo |  `ComparerBuilder<T>` |     10.369 ns |   0.0675 ns |     0.5005 ns |  1.47 |
|   `class` |   CompareTo |    `IFullComparer<T>` |     44.518 ns |   0.2642 ns |     2.0801 ns |  6.33 |
|           |             |                       |               |             |               |       |
|  `struct` |   CompareTo |      `IComparable<T>` |      4.445 ns |   0.0315 ns |     0.1577 ns |  1.00 |
|  `struct` |   CompareTo |         `Comparer<T>` |      6.080 ns |   0.0421 ns |     0.2454 ns |  1.37 |
|  `struct` |   CompareTo |  `ComparerBuilder<T>` |     10.785 ns |   0.0702 ns |     0.4967 ns |  2.45 |
|  `struct` |   CompareTo |    `IFullComparer<T>` |     45.007 ns |   0.2528 ns |     1.4074 ns | 10.09 |
|           |             |                       |               |             |               |       |
|   `class` |        Sort |      `IComparable<T>` |  3,662.370 ns |  20.6500 ns |   157.2659 ns |  1.00 |
|   `class` |        Sort |         `Comparer<T>` |  4,244.162 ns |  24.6312 ns |   200.1049 ns |  1.16 |
|   `class` |        Sort |  `ComparerBuilder<T>` |  5,066.787 ns |  24.6862 ns |   104.7348 ns |  1.40 |
|   `class` |        Sort |    `IFullComparer<T>` | 10,670.499 ns | 104.9246 ns | 1,038.7006 ns |  2.93 |
|           |             |                       |               |             |               |       |
|  `struct` |        Sort |      `IComparable<T>` |  4,927.141 ns |  28.3844 ns |   218.0250 ns |  1.00 |
|  `struct` |        Sort |         `Comparer<T>` |  4,001.959 ns |  21.3152 ns |   114.7858 ns |  0.81 |
|  `struct` |        Sort |  `ComparerBuilder<T>` |  5,223.683 ns |  30.1112 ns |   251.9286 ns |  1.07 |
|  `struct` |        Sort |    `IFullComparer<T>` |  9,691.713 ns |  39.5429 ns |   153.1491 ns |  1.97 |

</details>
