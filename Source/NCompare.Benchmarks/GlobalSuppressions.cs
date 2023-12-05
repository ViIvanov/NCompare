// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

using static Justifications;

[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = BenchmarkingProject)]
[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = BenchmarkingProject)]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = BenchmarkingProject)]

file static class Justifications
{
  public const string BenchmarkingProject = "It is OK to skip warning for the benchmarking project.";
}
