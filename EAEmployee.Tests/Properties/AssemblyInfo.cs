using NUnit.Framework;

// Run different test classes in parallel (one browser per class), but keep tests
// within a class sequential so they share the seeded admin login state cleanly.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]
