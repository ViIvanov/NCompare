# NCompare

The library provides the `ComparerBuilder<T>` type, which helps to create comparators (equality and sort comparators) for comparing objects of type `T`.

`ComparerBuilder<T>` helps to compare complex types. It can create `EqualityComparer<>` and/or a `Comparer<>` objects for your types.
You just need to "add" expressions, that describe what data you want compare and, optionally, comparators - to specify how to compare that data.

The main goal of having this type is to set once and uniformly according to what rules you need to compare objects, and after that be able
to get both `IEqualityComparer<T>` there and `IComparer<T>`.

You can find reasons for making `ComparerBuilder<>` in this old post on [RSDN](http://rsdn.ru/forum/src/3914421.1) (in Russian).

In the future, I plan to expand the library with comparators for various collections.
This will allow you to quickly and conveniently create comparators for complex and interconnected objects.

Now `ComparerBuilder<T>` is written on C# 14.0. it builds for .NET Framework 4.6.2 and .NET Standard 2.0 so could
be used in many projects.

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

For more complicated examples, including self-references and mutually referential types please visit project's web site https://github.com/ViIvanov/NCompare.
