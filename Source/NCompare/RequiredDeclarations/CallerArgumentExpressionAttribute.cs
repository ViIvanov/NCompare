namespace System.Runtime.CompilerServices;

[Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
  public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName ?? String.Empty;

  public string ParameterName { get; }
}
