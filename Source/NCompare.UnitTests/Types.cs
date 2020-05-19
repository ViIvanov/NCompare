using System;

namespace NCompare.UnitTests
{
  class TestValue
  {
    public TestValue(int number = default, DateTime? dateTime = default) => (Number, DateTime) = (number, dateTime);

    public int Number { get; }
    public DateTime? DateTime { get; }
  }

  class OtherValue
  {
    public OtherValue(string? text = default) => Text = text ?? String.Empty;

    public string Text { get; }
  }

  class DerivedValue : TestValue
  {
    public DerivedValue(int number = default, DateTime? dateTime = default, OtherValue? other = default) : base(number, dateTime) => Other = other;

    public OtherValue? Other { get; }
  }
}
