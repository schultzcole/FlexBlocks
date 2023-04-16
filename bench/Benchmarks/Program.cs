using Benchmarks;

Environment.CurrentDirectory = AppContext.BaseDirectory;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, new Config());

Console.WriteLine();
var resultsDir = Path.Combine(Environment.CurrentDirectory, "BenchmarkDotNet.Artifacts", "results");
Console.WriteLine($"""Wrote results to "{resultsDir}" """);
