﻿namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
  public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName ?? String.Empty;

  public string ParameterName { get; }
}
