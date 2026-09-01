// TestAssemblyInfo.cs
// ------------------------------------------------------------------
//
// Assembly-level NUnit settings shared by the DotNetZip test projects.
//
// ------------------------------------------------------------------

using NUnit.Framework;

// MSTest constructs a fresh instance of the test class for every test method;
// NUnit's default is a single instance shared by every test in the fixture.
// These tests were written against the MSTest model - IonicTestClass and friends
// keep per-test state (TopLevelDir, the list of files to clean up, the progress
// monitor channel) in instance fields - so ask NUnit for the same lifecycle
// rather than auditing every fixture for state that leaks between tests.
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
