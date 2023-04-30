namespace NCompare;

internal interface IComparerBuilderContext
{
  Type ComparedType { get; }
  IComparerBuilderInterception? Interception { get; }
}
