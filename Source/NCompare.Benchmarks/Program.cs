using System;
using BenchmarkDotNet.Running;

namespace NCompare.Benchmarks
{
  internal static class Program
  {
    static void Main(string[] args) {
      if(args is null || args.Length == 0) {
        args = new[] { "--filter", "*", };
      }//if

      BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
      Console.ReadLine();
    }
  }
}

