using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class ArgumentExceptionExtensions
{
  extension(ArgumentNullException)
  {
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
      if(argument is null) {
        Throw(paramName);
      }//if
    }
  }

  [DoesNotReturn]
  private static void Throw(string? paramName) => throw new ArgumentNullException(paramName);
}
