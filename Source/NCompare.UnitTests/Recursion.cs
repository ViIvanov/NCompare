using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests;

using static TestCompare;

[TestClass]
public sealed class Recursion
{
  [TestMethod]
  public void RecursiveComparerForLinkedList() {
    static LinkedListNode<string> Node(string value, LinkedListNode<string>? next = null) => new(value, next);

    var list1 = Node("1", Node("2", Node("3")));
    var list2 = Node("1", Node("2", Node("C")));
    var list3 = Node("1", Node("2", Node("3")));

    var builder = new ComparerBuilder<LinkedListNode<string>>()
      .Add(value => value.Value, StringComparer.Ordinal)
      .Add(value => value.Depth)
      .Add(value => value.Next!);
    var customEqualityComparer = new LinkedListNodeEqualityComparer<string>();

    Test("1/2", list1, list2, expected: CompareResult.LessThan, builder, customEqualityComparer);
    Test("2/3", list2, list3, expected: CompareResult.GreaterThan, builder, customEqualityComparer);
    Test("1/3", list1, list3, expected: CompareResult.Equal, builder, customEqualityComparer);

    static void Test<T>(string title, T? x, T? y, CompareResult expected, ComparerBuilder<T> builder, IEqualityComparer<T> equalityComparer) {
      TestComparers(title, x, y, expected, builder);
      TestEqualityComparer(title, x, y, expected is CompareResult.Equal, EqualityComparer<T>.Default);
      TestEqualityComparer(title, x, y, expected is CompareResult.Equal, equalityComparer);
    }
  }

  private sealed class LinkedListNode<T> : IEquatable<LinkedListNode<T>>
  {
    public LinkedListNode(T value, LinkedListNode<T>? next = null) {
      Value = value;
      Next = next;
      Depth = (Next?.Depth ?? 0) + 1;
    }

    public T Value { get; }
    public int Depth { get; }
    public LinkedListNode<T>? Next { get; }

    public override string ToString() => $"[{Value}@{Depth}] -> {{{Next?.ToString() ?? "<null>"}}}";

    public override bool Equals(object? obj) => Equals(obj as LinkedListNode<T>);

    public override int GetHashCode() => Value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value);

    public bool Equals(LinkedListNode<T>? other) => other is not null && EqualityComparer<T>.Default.Equals(other.Value, Value) && other.Depth == Depth && Equals(other.Next, Next);
  }

  private sealed class LinkedListNodeEqualityComparer<T> : EqualityComparer<LinkedListNode<T>>
  {
    public override bool Equals(LinkedListNode<T>? x, LinkedListNode<T>? y) {
      return Next(x, y);

      static bool Next(LinkedListNode<T>? x, LinkedListNode<T>? y) {
        while(true) {
          if(ReferenceEquals(x, y)) {
            return true;
          } else if(x is not null && y is not null && EqualityComparer<T>.Default.Equals(x.Value, y.Value) && x.Depth == y.Depth) {
            (x, y) = (x.Next, y.Next);
          } else {
            return false;
          }//if
        }//while
      }
    }

    public override int GetHashCode(LinkedListNode<T>? obj)
      => obj is not null ? (obj.Value is null ? -31 : EqualityComparer<T>.Default.GetHashCode(obj.Value)) : -13;
  }

  [TestMethod]
  public void CompareMutuallyRecursiveTypes() {
    var xbuilder = new ComparerBuilder<XParam>();
    var ybuilder = new ComparerBuilder<YParam>();

    xbuilder.Add(item => item.Name);
    xbuilder.Add(item => item.Created);
    xbuilder.Add(item => item.Param, ybuilder);

    ybuilder.Add(item => item.Number);
    ybuilder.Add(item => item.Title);
    ybuilder.Add(item => item.Param, xbuilder);

    var x = CreateXParam();
    var y = CreateXParam();
    var z = FixParam(CreateXParam(), 5);
    var w = FixParam(CreateXParam(), -3);

    TestComparers("x/y", x, y, expected: CompareResult.Equal, xbuilder);
    TestComparers("x/z", x, z, expected: CompareResult.LessThan, xbuilder);
    TestComparers("y/w", y, w, expected: CompareResult.GreaterThan, xbuilder);
    TestComparers("z/w", z, w, expected: CompareResult.GreaterThan, xbuilder);

    static XParam CreateXParam() => new() {
      Name = "A",
      Created = new DateOnly(2023, 01, 01),
      Param = new YParam {
        Number = 1,
        Title = "X",
        Param = new XParam {
          Name = "B",
          Created = new DateOnly(2020, 01, 01),
          Param = new YParam {
            Number = 2,
            Title = "Y",
          },
        },
      },
    };

    static T FixParam<T>(T item, int increment) {
      var last = LastParam(item);
      switch(last) {
      case XParam x:
        x.Created = x.Created.AddDays(increment);
        break;
      case YParam y:
        y.Number += increment;
        break;
      }

      return item;

      static object LastParam(object? item) => item switch { // Returns ".Param" whose ".Param" is null
        null => throw new ArgumentNullException(nameof(item)),
        XParam { Param: null, } => item,
        XParam x => LastParam(x.Param),
        YParam { Param: null, } => item,
        YParam y => LastParam(y.Param),
        _ => throw new ArgumentException($"Unsupported type: {item.GetType()}", nameof(item)),
      };
    }
  }

  private sealed class XParam
  {
    public string Name { get; set; } = String.Empty;
    public DateOnly Created { get; set; }

    public YParam? Param { get; init; }
  }

  private sealed class YParam
  {
    public int Number { get; set; }
    public string Title { get; set; } = String.Empty;

    public XParam? Param { get; init; }
  }
}
