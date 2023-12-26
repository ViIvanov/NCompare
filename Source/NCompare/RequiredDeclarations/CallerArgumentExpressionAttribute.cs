namespace System.Runtime.CompilerServices;

[Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
  public string ParameterName { get; } = parameterName ?? String.Empty;
}
