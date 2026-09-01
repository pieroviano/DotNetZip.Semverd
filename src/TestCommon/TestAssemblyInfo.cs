// TestAssemblyInfo.cs
// ------------------------------------------------------------------
//
// Assembly-level xUnit settings shared by the DotNetZip test projects.
//
// ------------------------------------------------------------------

using Xunit;

// These tests drive the library through the real file system, and they do it by
// creating a temp directory per test and calling Directory.SetCurrentDirectory
// into it. The current directory is process-wide, so two tests running at once
// would silently read and write each other's files. MSTest and NUnit both ran
// them one at a time; xUnit runs test classes in parallel by default, so say
// otherwise here rather than rewrite every fixture to use absolute paths.
//
// (The other half of the old NUnit setting -- FixtureLifeCycle.InstancePerTestCase,
// which these fixtures need because they keep per-test state in instance fields --
// needs no equivalent: constructing the class once per test is xUnit's default.)
[assembly: CollectionBehavior(DisableTestParallelization = true)]
