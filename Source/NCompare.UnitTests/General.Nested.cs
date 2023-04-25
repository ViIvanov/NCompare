using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCompare.UnitTests;

using static TestCompare;

partial class General
{
  [TestMethod]
  public void Nested() {
    var xbuilder = new ComparerBuilder<XObject>();
    var ybuilder = new ComparerBuilder<YObject>();

    xbuilder.Add(item => item.Text.Length > 0 ? item.Text[0].ToString() : String.Empty, StringComparer.OrdinalIgnoreCase);
    xbuilder.Add(item => item.Y, ybuilder!);

    ybuilder.Add(item => item.Value < 0 ? -1 : (item.Value > 0 ? 1 : 0));
    ybuilder.Add(item => item.X, xbuilder!);

    var xcomparer = xbuilder.CreateEqualityComparer();
    var ycomparer = ybuilder.CreateEqualityComparer();

    var x1 = new XObject { Text = "Simple X1", };
    var x2 = new XObject { Text = "Dimple X2", };
    var x3 = new XObject { Text = "simple X3", };

    TestEqualityComparer("x/1/2", x1, x2, expected: false, xcomparer);
    TestEqualityComparer("x/2/3", x2, x3, expected: false, xcomparer!);
    TestEqualityComparer("x/1/3", x1, x3, expected: true, xcomparer!);
    TestEqualityComparer("x/1/null", x1, null, expected: false, xcomparer!);

    var y1 = new YObject { Value = -5, X = x1, };
    var y2 = new YObject { Value = 10, X = x2, };
    var y3 = new YObject { Value = -1, X = x3, };
    var y4 = new YObject { Value = -1, };

    TestEqualityComparer("y/1/2", y1, y2, expected: false, ycomparer);
    TestEqualityComparer("y/2/3", y2, y3, expected: false, ycomparer!);
    TestEqualityComparer("y/1/3", y1, y3, expected: true, ycomparer!);
    TestEqualityComparer("y/2/4", y2, y4, expected: false, ycomparer!);
    TestEqualityComparer("y/1/null", y1, null, expected: false, ycomparer!);

    var x11 = new XObject { Text = "Complex X11", Y = y1, };
    var x12 = new XObject { Text = "flex X12", Y = y2, };
    var x13 = new XObject { Text = "complex X13", Y = y3, };
    var x14 = new XObject { Text = "complex X14", Y = y4, };
    var x15 = new XObject { Text = "complex X13", Y = y3, };

    TestEqualityComparer("x/11/12", x11, x12, expected: false, xcomparer);
    TestEqualityComparer("x/12/13", x12, x13, expected: false, xcomparer!);
    TestEqualityComparer("x/11/13", x11, x13, expected: true, xcomparer!);
    TestEqualityComparer("x/14/15", x14, x15, expected: false, xcomparer!);
    TestEqualityComparer("x/11/null", x11, null, expected: false, xcomparer!);
  }
}

file sealed class XObject
{
  public string Text { get; set; } = String.Empty;
  public YObject? Y { get; set; }
  public override string ToString() => $"\"{Text}\", {{{Y}}}";
}

file sealed class YObject
{
  public int Value { get; set; }
  public XObject? X { get; set; }
  public override string ToString() => $"[{Value}], {{{X}}}";
}
