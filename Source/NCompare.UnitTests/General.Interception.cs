﻿using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests;

partial class General
{
  [TestMethod]
  public void Interception() {
    var interception = new Interception();
    var builder = new ComparerBuilder<string>(interception).Add<string>(item => item);
    Assert.IsNotNull(builder.Interception, $"{nameof(builder.Interception)} should be not null after object creation.");

    const string EqualsX = nameof(EqualsX);
    const string EqualsY = nameof(EqualsY);
    const string GetHashCodeObj = nameof(GetHashCodeObj);
    const string CompareX = nameof(CompareX);
    const string CompareY = nameof(CompareY);

    var equalityComparer = builder.CreateEqualityComparer();
    _ = equalityComparer.Equals(EqualsX, EqualsY);
    Assert.AreEqual(expected: EqualsX, interception.X);
    Assert.AreEqual(expected: EqualsY, interception.Y);

    _ = equalityComparer.GetHashCode(GetHashCodeObj);
    Assert.AreEqual(expected: GetHashCodeObj, interception.X);
    Assert.IsNull(interception.Y);

    var comparer = builder.CreateComparer();
    _ = comparer.Compare(CompareX, CompareY);
    Assert.AreEqual(expected: CompareX, interception.X);
    Assert.AreEqual(expected: CompareY, interception.Y);
  }

  [TestMethod]
  public void ConvertToNullToInterception() {
    var original = new ComparerBuilder<object>();
    Assert.IsNull(original.Interception, "Original");

    var interception = new Interception();
    var converted = original.ConvertTo<string>(interception);
    Assert.AreEqual(expected: interception, converted.Interception, "Converted");
  }

  [TestMethod]
  public void ConvertToNullToEmpty() {
    var original = new ComparerBuilder<object>();
    Assert.IsNull(original.Interception, "Original");

    var converted = original.ConvertTo<string>();
    Assert.IsNull(converted.Interception, "Converted");
  }

  [TestMethod]
  public void ConvertToInterceptionToNull() {
    var interception = new Interception();
    var original = new ComparerBuilder<object>(interception);
    Assert.AreEqual(expected: interception, original.Interception, "Original");

    var converted = original.ConvertTo<string>(interception: null);
    Assert.IsNull(converted.Interception, "Converted");
  }

  [TestMethod]
  public void ConvertToInterceptionToEmpty() {
    var interception = new Interception();
    var original = new ComparerBuilder<object>(interception);
    Assert.AreEqual(expected: interception, original.Interception, "Original");

    var converted = original.ConvertTo<string>();
    Assert.AreEqual(expected: interception, converted.Interception, "Converted");
  }
}

[DebuggerDisplay($"({nameof(X)} = {{{nameof(X)}}}, {nameof(Y)} = {{{nameof(Y)}}})")]
file sealed class Interception : IComparerBuilderInterception
{
  // Public methods checks, that `ComparerBuilder<>` uses interface implementation instead of these methods.

  public int InterceptCompare<T>(int value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => throw new NotImplementedException();
  public bool InterceptEquals<T>(bool value, T x, T y, ComparerBuilderInterceptionArgs<T> args) => throw new NotImplementedException();
  public int InterceptGetHashCode<T>(int value, T obj, ComparerBuilderInterceptionArgs<T> args) => throw new NotImplementedException();

  public object? X { get; private set; }
  public object? Y { get; private set; }

  public override string ToString() => nameof(Interception);

  int IComparerBuilderInterception.InterceptCompare<T>(int value, T x, T y, ComparerBuilderInterceptionArgs<T> args) {
    (X, Y) = (x, y);
    return value;
  }

  bool IComparerBuilderInterception.InterceptEquals<T>(bool value, T x, T y, ComparerBuilderInterceptionArgs<T> args) {
    (X, Y) = (x, y);
    return value;
  }

  int IComparerBuilderInterception.InterceptGetHashCode<T>(int value, T obj, ComparerBuilderInterceptionArgs<T> args) {
    (X, Y) = (obj, null);
    return value;
  }
}
