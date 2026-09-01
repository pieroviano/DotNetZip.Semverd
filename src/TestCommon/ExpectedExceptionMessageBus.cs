// ExpectedExceptionMessageBus.cs
// ------------------------------------------------------------------
//
// Sits between an [ExpectedExceptionFact] test and the runner and swaps its
// verdict:
//
//   threw the expected type   -> pass
//   threw some other type     -> fail, naming both types
//   threw nothing             -> fail, saying so
//
// ------------------------------------------------------------------

using System;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ionic.Tests
{
    internal sealed class ExpectedExceptionMessageBus : IMessageBus
    {
        private readonly IMessageBus _inner;
        private readonly string _expectedTypeName;

        public ExpectedExceptionMessageBus(IMessageBus inner, string expectedTypeName)
        {
            _inner = inner;
            _expectedTypeName = expectedTypeName;
        }

        /// <summary>How many passes this bus turned into failures.</summary>
        public int FailuresAdded { get; private set; }

        /// <summary>How many failures this bus turned into passes.</summary>
        public int FailuresRemoved { get; private set; }

        /// <summary>How many failures this bus turned into skips.</summary>
        public int Skipped { get; private set; }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (message is ITestFailed failed)
            {
                string thrown = failed.ExceptionTypes.Length > 0 ? failed.ExceptionTypes[0] : null;

                // A test can decline to run even when it declares an expected
                // exception -- Compatibility skips itself when the process is
                // not elevated. Honour the skip rather than reporting it as the
                // wrong exception type.
                if (String.Equals(thrown, "Xunit.SkipException", StringComparison.Ordinal))
                {
                    FailuresRemoved++;
                    Skipped++;
                    return _inner.QueueMessage(
                        new TestSkipped(failed.Test, failed.Messages.Length > 0 ? failed.Messages[0] : "skipped"));
                }

                if (String.Equals(thrown, _expectedTypeName, StringComparison.Ordinal))
                {
                    FailuresRemoved++;
                    return _inner.QueueMessage(new TestPassed(failed.Test, failed.ExecutionTime, failed.Output));
                }

                return _inner.QueueMessage(
                    new TestFailed(failed.Test,
                                   failed.ExecutionTime,
                                   failed.Output,
                                   new XunitException(String.Format("Expected {0} but got {1}: {2}",
                                                                    Simplify(_expectedTypeName),
                                                                    Simplify(thrown) ?? "an unnamed exception",
                                                                    failed.Messages.Length > 0 ? failed.Messages[0] : ""))));
            }

            if (message is ITestPassed passed)
            {
                FailuresAdded++;
                return _inner.QueueMessage(
                    new TestFailed(passed.Test,
                                   passed.ExecutionTime,
                                   passed.Output,
                                   new XunitException("Expected " + Simplify(_expectedTypeName)
                                                      + " but no exception was thrown")));
            }

            return _inner.QueueMessage(message);
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        private static string Simplify(string fullTypeName)
        {
            if (String.IsNullOrEmpty(fullTypeName)) return fullTypeName;

            int dot = fullTypeName.LastIndexOf('.');
            return dot < 0 ? fullTypeName : fullTypeName.Substring(dot + 1);
        }
    }
}
