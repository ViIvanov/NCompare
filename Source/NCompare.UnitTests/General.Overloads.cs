using System.Diagnostics;

namespace NCompare.UnitTests;

partial class General
{
  [TestMethod]
  public void Overloads() {
    var builder = new ComparerBuilder<TContainer>()
      .Add(item => item.MyObject)
      .Add(item => item.MyValue);

    var x = new TContainer {
      MyObject = new TObject { Value = 1, },
      MyValue = new TValue { Value = 2, },
    };

    var y = new TContainer {
      MyObject = new TObject { Value = 1, },
      MyValue = new TValue { Value = 2, },
    };

    var equalityComparer = builder.CreateEqualityComparer();

    var hx = equalityComparer.GetHashCode(x);
    var hy = equalityComparer.GetHashCode(y);
    Assert.AreEqual(hx, hy, "Hash codes is not equal");

    Assert.AreEqual(x, y, equalityComparer, "Equals returns unexpected value");
  }
}

file sealed class TContainer
{
  public TObject? MyObject { get; set; }
  public TValue MyValue { get; set; }
}

file abstract class TBaseObject : IEquatable<TBaseObject>, IEquatable<TObject>
{
  public override bool Equals(object? other) {
    Debug.Print($"{nameof(TBaseObject)}::{nameof(Equals)}(object) called");
    return true;
  }

  public override int GetHashCode() {
    Debug.Print($"{nameof(TBaseObject)}::{nameof(GetHashCode)}() called");
    return 0;
  }

  public bool Equals(TBaseObject? other) {
    Debug.Print($"{nameof(TBaseObject)}::{nameof(Equals)}({nameof(TBaseObject)}) called");
    return true;
  }

  public bool Equals(TObject? other) {
    Debug.Print($"{nameof(TBaseObject)}::{nameof(Equals)}({nameof(TObject)}) called");
    return true;
  }

  public static bool operator ==(TBaseObject? left, TBaseObject? right) {
    Assert.Fail($"{nameof(TBaseObject)}::operator == called");
    return true;
  }

  public static bool operator !=(TBaseObject? left, TBaseObject? right) {
    Assert.Fail($"{nameof(TBaseObject)}::operator != called");
    return true;
  }
}

file sealed class TObject : TBaseObject
{
  public int Value { get; set; }

#pragma warning disable CA1061 // Do not hide base class methods
  public new bool Equals(object? other) {
    Assert.Fail($"{nameof(TObject)}::{nameof(Equals)}(object) called");
    return true;
  }
#pragma warning restore CA1061 // Do not hide base class methods

  public new int GetHashCode() {
    Assert.Fail($"{nameof(TObject)}::{nameof(GetHashCode)}() called");
    return 0;
  }

  public new bool Equals(TObject? other) {
    Assert.Fail($"{nameof(TObject)}::{nameof(Equals)}({nameof(TObject)}) called");
    return true;
  }
}

#pragma warning disable CS0660, CS0661 // Type defines operator == or operator != but does not override Object.Equals(object o) / Object.GetHashCode()
file readonly struct TValue
#pragma warning restore CS0660, CS0661 // Type defines operator == or operator != but does not override Object.Equals(object o) / Object.GetHashCode()
{
  public int Value { get; init; }

  public new bool Equals(object? other) {
    Assert.Fail($"{nameof(TValue)}::{nameof(Equals)}(object) called");
    return true;
  }

  public new int GetHashCode() {
    Assert.Fail($"{nameof(TValue)}::{nameof(GetHashCode)}() called");
    return 0;
  }

  public bool Equals(TValue other) {
    Assert.Fail($"{nameof(TValue)}::{nameof(Equals)}({nameof(TObject)}) called");
    return true;
  }

  public static bool operator ==(TValue left, TValue right) {
    Assert.Fail($"{nameof(TValue)}::operator == called");
    return true;
  }

  public static bool operator !=(TValue left, TValue right) {
    Assert.Fail($"{nameof(TValue)}::operator != called");
    return true;
  }
}
