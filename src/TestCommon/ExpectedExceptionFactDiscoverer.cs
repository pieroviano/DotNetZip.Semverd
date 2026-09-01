// ExpectedExceptionFactDiscoverer.cs
// ------------------------------------------------------------------
//
// Turns each [ExpectedExceptionFact] into an ExpectedExceptionTestCase.
// See ExpectedExceptionFactAttribute.cs for why this exists.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ionic.Tests
{
    public class ExpectedExceptionFactDiscoverer : IXunitTestCaseDiscoverer
    {
        public ExpectedExceptionFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            DiagnosticMessageSink = diagnosticMessageSink;
        }

        protected IMessageSink DiagnosticMessageSink { get; }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions,
                                                    ITestMethod testMethod,
                                                    IAttributeInfo factAttribute)
        {
            var expected = factAttribute.GetNamedArgument<Type>(nameof(ExpectedExceptionFactAttribute.ExceptionType));

            // The attribute takes a Type, but a test case has to survive being
            // serialized across to the runner, so carry the name rather than
            // the Type itself.
            yield return new ExpectedExceptionTestCase(expected?.AssemblyQualifiedName,
                                                       DiagnosticMessageSink,
                                                       discoveryOptions.MethodDisplayOrDefault(),
                                                       discoveryOptions.MethodDisplayOptionsOrDefault(),
                                                       testMethod);
        }
    }
}
