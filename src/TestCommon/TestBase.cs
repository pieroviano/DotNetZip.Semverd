// TestBase.cs
// ------------------------------------------------------------------
//
// The DotNetZip tests narrate what they are doing through ~820 calls to
// TestContext.WriteLine, and that narration is most of what a failing run has
// to explain itself with. xUnit has no static output sink: output belongs to
// one running test and reaches it through an ITestOutputHelper handed to the
// test class constructor.
//
// So TestContext becomes an instance member supplied by this base class. Every
// existing `TestContext.WriteLine(...)` call site then compiles untouched and
// its output still lands in the test report.
//
// ------------------------------------------------------------------

using System;

using Xunit.Abstractions;

namespace Ionic.Tests
{
    /// <summary>
    ///   Base class for the DotNetZip test fixtures. Supplies <see cref="TestContext"/>.
    /// </summary>
    public abstract class TestBase
    {
        protected TestBase(ITestOutputHelper output)
        {
            TestContext = new TestOutput(output);
        }

        /// <summary>
        ///   Per-test output sink, named to match the MSTest/NUnit call sites.
        /// </summary>
        protected TestOutput TestContext { get; }
    }


    /// <summary>
    ///   Writes to the running test's xUnit output.
    /// </summary>
    public sealed class TestOutput
    {
        private readonly ITestOutputHelper _output;

        public TestOutput(ITestOutputHelper output)
        {
            _output = output;
        }

        public void WriteLine()
        {
            WriteLine(String.Empty);
        }

        public void WriteLine(string message)
        {
            if (_output == null) return;

            try
            {
                _output.WriteLine(message ?? String.Empty);
            }
            catch (InvalidOperationException)
            {
                // xUnit rejects output written outside a running test (from a
                // fixture's disposal, say). Losing a diagnostic line is not a
                // reason to fail the test that produced it.
            }
        }

        public void WriteLine(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                WriteLine(format);
                return;
            }

            string message;
            try
            {
                message = String.Format(format, args);
            }
            catch (FormatException)
            {
                message = format;
            }

            WriteLine(message);
        }
    }
}
