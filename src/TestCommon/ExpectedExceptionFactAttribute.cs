// ExpectedExceptionFactAttribute.cs
// ------------------------------------------------------------------
//
// ~97 DotNetZip tests assert failure by declaring the exception they expect on
// the method rather than by wrapping their body: the test passes only when it
// throws an exception of exactly the named type. MSTest spelled that
// [ExpectedException], NUnit dropped it in v3 and it was restored on top of
// IWrapTestMethod, and xUnit has no equivalent at all -- its answer is
// Assert.Throws around the body.
//
// Rewriting 97 bodies would be the largest and least reviewable edit in the
// conversion, so the attribute is restored a third time instead, here on
// xUnit's test-case extensibility point. A test changes by one line:
//
//     [Test]                                  [ExpectedExceptionFact(typeof(ZipException))]
//     [ExpectedException(typeof(ZipException))]   =>   public void Foo() { ... }
//     public void Foo() { ... }
//
// The mechanism is the one Xunit.SkippableFact uses: let the test run exactly
// as a [Fact] would, and rewrite the result message on its way to the runner.
// That keeps this clear of xUnit's invoker internals -- no custom runner, no
// custom invoker, just a filtered message bus.
//
// ------------------------------------------------------------------

using System;

using Xunit;
using Xunit.Sdk;

namespace Ionic.Tests
{
    /// <summary>
    ///   Marks a test that passes only when it throws an exception whose type is
    ///   <em>exactly</em> the expected one.
    /// </summary>
    /// <remarks>
    ///   A derived exception type, a different type, or no exception at all is a
    ///   failure. That is MSTest's rule, deliberately kept: the tests using this
    ///   were written to pin an exact exception type, and widening it to "or any
    ///   subclass" would quietly weaken all 97 of them.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [XunitTestCaseDiscoverer("Ionic.Tests.ExpectedExceptionFactDiscoverer", "Ionic.TestCommon")]
    public sealed class ExpectedExceptionFactAttribute : FactAttribute
    {
        /// <param name="exceptionType">the exact type of the exception the test must throw.</param>
        public ExpectedExceptionFactAttribute(Type exceptionType)
        {
            ExceptionType = exceptionType;
        }

        public Type ExceptionType { get; }
    }
}
