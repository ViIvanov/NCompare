using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests
{
  using static TestCompare;

  [TestClass]
  public sealed class Recursion
  {
    [TestMethod]
    public void RecursiveComparerForLinkedList() {
      //var xxx = ComparerBuilder.Y<int, int, int>((f, x, y) => x > y ? f(f, x - 1, y + 1) : x * y);
      //var yyy = xxx.Compile();
      //var zzz = yyy(5, 1);
      //var builder = new ComparerBuilder<LinkedListNode<string>>();
      //var lazy = new Lazy<ComparerBuilder<LinkedListNode<string>>>(() => builder);
      //var equalityComparer = default(IEqualityComparer<LinkedListNode<string>>);
      //var lazy = new Lazy<ComparerBuilder<string>>(() => new ComparerBuilder<string>().Add(item => item), false);
      //var lazy = ComparerBuilder.Lazy<LinkedListNode<string>>();

      //var builder = new ComparerBuilder<LinkedListNode<string>>();
      //var lazyEqualityComparer = builder.CreateLazyEqualityComparer();

      //builder.Interception.
      //builder.Add(value => value.Value, StringComparer.Ordinal);
      //builder.Add(value => value.Depth);
      //builder.Add(value => value.Next, lazyEqualityComparer!);

      //var equalityComparer = builder.CreateEqualityComparerIntercepted();
      //var interceptor = new Interceptor();
      //var equalityComparer = builder.CreateEqualityComparerIntercepted(interceptor);
      //var equalityComparer = builder.CreateEqualityComparer();
      //var comparer = builder.CreateComparerIntercepted();
      //var comparer = builder.CreateComparer();
      //var equalityComparer = EqualityComparer<LinkedListNode<string>>.Default;
      //var equalityComparer = lazyEqualityComparer.Value;

      //LinkedListNode<string> Node(string value, LinkedListNode<string>? next = null) => new LinkedListNode<string>(value, next);

      //var list1 = Node("1", Node("2", Node("3")));
      //var list2 = Node("1", Node("2", Node("C")));
      //var list3 = Node("1", Node("2", Node("3")));

      //var builder = new ComparerBuilder<LinkedListNode<string>>();
      //var lazyEqualityComparer = builder.CreateLazyEqualityComparer();
      //builder.Add(value => value.Value, StringComparer.Ordinal);
      //builder.Add(value => value.Depth);
      //builder.Add(value => value.Next, lazyEqualityComparer!);
      ////builder.Add(
      //var equalityComparer = lazyEqualityComparer.Value;
      //Assert.IsNotNull(equalityComparer, nameof(equalityComparer));

      //var test1 = equalityComparer.Equals(list1, list2); // False
      //var test2 = equalityComparer.Equals(list2, list3); // False
      //var test3 = equalityComparer.Equals(list1, list3); // True

      //try {
      //  var xtest2 = equalityComparer.Equals(data1, data4);
      //} catch(Exception) {
      //  throw;
      //}

      static LinkedListNode<string> Node(string value, LinkedListNode<string>? next = null) => new(value, next);

      var list1 = Node("1", Node("2", Node("3")));
      var list2 = Node("1", Node("2", Node("C")));
      var list3 = Node("1", Node("2", Node("3")));

      var builder = new ComparerBuilder<LinkedListNode<string>>();
      builder.Add(value => value.Value, StringComparer.Ordinal);
      builder.Add(value => value.Depth);
      builder.Add(value => value.Next, builder!);
      var equalityComparer = builder.CreateEqualityComparer();
      Assert.IsNotNull(equalityComparer, nameof(equalityComparer));

      TestEqualityComparer("1/2", list1, list2, expected: false, equalityComparer);
      TestEqualityComparer("2/3", list2, list3, expected: false, equalityComparer);
      TestEqualityComparer("1/3", list1, list3, expected: true, equalityComparer);

      //var test1 = equalityComparer.Equals(list1, list2); // False
      //var test2 = equalityComparer.Equals(list2, list3); // False
      //var test3 = equalityComparer.Equals(list1, list3); // True
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

    //internal sealed class Interceptor : IComparerBuilderInterception
    //{
    //  public int InterceptCompare<T>(int value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => value;
    //  public bool InterceptEquals<T>(bool value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => value;
    //  public int InterceptGetHashCode<T>(int value, T obj, ComparerBuilderInterceptionArgs<T> args) => value;
    //}

    [TestMethod]
    public void RecursiveComparerForListNode() {
      var listNodeBuilder = new ComparerBuilder<ListNode<int>>();
      var listNotesBuilder = new ComparerBuilder<ListNotes<int>>();

      listNodeBuilder.Add(item => item.Value);
      //listNodeBuilder.Add(item => item.Previous, lazyListNodeEqualityComparer!);
      listNodeBuilder.Add(item => item.Next!);
      //listNodeBuilder.Add(item => item.Previous!);
      //listNodeBuilder.Add(item => item.Next!);
      listNodeBuilder.Add(item => item.Notes, listNotesBuilder);

      //listNotesBuilder.Add(item => item.Root, lazyListNodeEqualityComparer);
      //listNotesBuilder.Add(item => item.Root, listNodeBuilder);
      listNotesBuilder.Add(item => item.SavePath, StringComparer.OrdinalIgnoreCase);

      var equalityComparer = listNodeBuilder.CreateEqualityComparer();

      static ListNode<int> Build(params string[] paths) {
        var root = new ListNode<int>(0, "Root");

        if(paths != null) {
          var index = root.Value + 1;
          var previous = root;
          foreach(var item in paths) {
            var child = new ListNode<int>(index++, $"Child {item}", previous: previous);
            previous.Next = child;
            previous = child;
          }//for
        }//if

        return root;
      }

      //var list1 = Build("X1", "X2", "X3", "X4");
      //var list2 = Build("X1", "X2", "Y3", "X4");
      //var list3 = Build("X1", "X2", "X3", "X4");

      var list1 = Build("X1");
      var list2 = Build("X2");
      var list3 = Build("X1");

      TestEqualityComparer("1/2", list1, list2, expected: false, equalityComparer);
      TestEqualityComparer("2/3", list2, list3, expected: false, equalityComparer);
      TestEqualityComparer("1/3", list1, list3, expected: true, equalityComparer);
      //var equals12 = equalityComparer.Equals(list1, list2);
      //Assert.IsFalse(equals12, "1 <=> 2");
      //var equals23 = equalityComparer.Equals(list2, list3);
      //Assert.IsFalse(equals12, "2 <=> 3");
      //var equals13 = equalityComparer.Equals(list1, list3);
      //Assert.IsTrue(equals12, "1 <=> 3");
    }

    class ListNode<T>
    {
      public ListNode(T value, string savePath, ListNode<T>? next = default, ListNode<T>? previous = default)
        => (Value, Next, Previous, Notes) = (value, next, previous, new ListNotes<T>(this, savePath));

      public T Value { get; }
      public ListNode<T>? Previous { get; set; }
      public ListNode<T>? Next { get; set; }

      public ListNotes<T> Notes { get; }

      public override string ToString() => $"{nameof(Value)}: {Value}, {nameof(Notes)}: {Notes}";
    }

    class ListNotes<T>
    {
      public ListNotes(ListNode<T> root, string savePath = "") => (Root, SavePath) = (root, savePath);

      public ListNode<T> Root { get; }
      public string SavePath { get; set; }

      public override string ToString() => $"{nameof(SavePath)}: {SavePath}, {nameof(Root)}: {Root.Value}";
    }

    //class XParam
    //{
    //  public List<YParam> YParams { get; }
    //}

    //class YParam
    //{
    //}
  }
}
