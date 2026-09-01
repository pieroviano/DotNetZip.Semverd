// ExpectedExceptionTestCase.cs
// ------------------------------------------------------------------
//
// Runs an [ExpectedExceptionFact] test as an ordinary fact and then inverts
// the verdict: throwing the expected type is the pass, and anything else --
// the wrong type, or no exception at all -- is the failure.
//
// The inversion happens on the message bus rather than around the invocation,
// so this needs none of xUnit's runner or invoker internals.
//
// ------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ionic.Tests
{
    public class ExpectedExceptionTestCase : XunitTestCase
    {
        private string _expectedExceptionTypeName;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public ExpectedExceptionTestCase()
        {
        }

        public ExpectedExceptionTestCase(string expectedExceptionTypeName,
                                         IMessageSink diagnosticMessageSink,
                                         TestMethodDisplay defaultMethodDisplay,
                                         TestMethodDisplayOptions defaultMethodDisplayOptions,
                                         ITestMethod testMethod,
                                         object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
            _expectedExceptionTypeName = expectedExceptionTypeName;
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue(nameof(_expectedExceptionTypeName), _expectedExceptionTypeName);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            _expectedExceptionTypeName = data.GetValue<string>(nameof(_expectedExceptionTypeName));
        }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                        IMessageBus messageBus,
                                                        object[] constructorArguments,
                                                        ExceptionAggregator aggregator,
                                                        CancellationTokenSource cancellationTokenSource)
        {
            var interceptor = new ExpectedExceptionMessageBus(messageBus, ExpectedTypeName());

            RunSummary summary = await base.RunAsync(diagnosticMessageSink,
                                                     interceptor,
                                                     constructorArguments,
                                                     aggregator,
                                                     cancellationTokenSource);

            // base counted the raw outcome; the bus rewrote it, so correct the tally.
            summary.Failed += interceptor.FailuresAdded - interceptor.FailuresRemoved;
            summary.Skipped += interceptor.Skipped;
            return summary;
        }

        /// <summary>
        ///   The simple, non-assembly-qualified name is what xUnit reports as the
        ///   thrown exception's type, so compare on that.
        /// </summary>
        private string ExpectedTypeName()
        {
            if (String.IsNullOrEmpty(_expectedExceptionTypeName)) return null;

            Type resolved = Type.GetType(_expectedExceptionTypeName, throwOnError: false);
            if (resolved != null) return resolved.FullName;

            // Fall back to the leading "Namespace.Type" of the qualified name.
            int comma = _expectedExceptionTypeName.IndexOf(',');
            return comma < 0 ? _expectedExceptionTypeName : _expectedExceptionTypeName.Substring(0, comma).Trim();
        }
    }
}
