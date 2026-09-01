// ExpectedExceptionAttribute.cs
// ------------------------------------------------------------------
//
// NUnit 3 dropped MSTest's [ExpectedException]. The DotNetZip test suite uses it
// in ~90 places, so rather than restructure all of those tests, this restores the
// attribute on top of NUnit's command-wrapping extension point.
//
// The semantics deliberately match MSTest's: the test passes only when the test
// method throws an exception whose type is *exactly* the expected type. A derived
// exception type, a different type, or no exception at all is a failure.
//
// ------------------------------------------------------------------

using System;

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Ionic.Tests
{
    /// <summary>
    /// Marks a test method as one that is expected to throw a particular exception.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExpectedExceptionAttribute : NUnitAttribute, IWrapTestMethod
    {
        private readonly Type _expectedExceptionType;

        /// <param name="type">the exact type of the exception the test must throw.</param>
        public ExpectedExceptionAttribute(Type type)
        {
            _expectedExceptionType = type;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ExpectedExceptionCommand(command, _expectedExceptionType);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand
        {
            private readonly Type _expectedType;

            public ExpectedExceptionCommand(TestCommand innerCommand, Type expectedType)
                : base(innerCommand)
            {
                _expectedType = expectedType;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                Type caughtType = null;

                try
                {
                    innerCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    // NUnit wraps exceptions thrown from the test method itself.
                    if (ex is NUnitException && ex.InnerException != null)
                        ex = ex.InnerException;
                    caughtType = ex.GetType();
                }

                if (caughtType == _expectedType)
                {
                    context.CurrentResult.SetResult(ResultState.Success);
                }
                else if (caughtType != null)
                {
                    context.CurrentResult.SetResult(ResultState.Failure,
                        String.Format("Expected {0} but got {1}", _expectedType.Name, caughtType.Name));
                }
                else
                {
                    context.CurrentResult.SetResult(ResultState.Failure,
                        String.Format("Expected {0} but no exception was thrown", _expectedType.Name));
                }

                return context.CurrentResult;
            }
        }
    }
}
