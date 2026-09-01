// Assert.cs
// ------------------------------------------------------------------
//
// The DotNetZip tests were written against MSTest and later run on NUnit, so
// all ~685 assertions in them are spelled the classic way -- AreEqual, IsTrue,
// IsNotNull -- and most carry a failure message explaining what was being
// checked. Xunit.Assert has no message parameter on any of its methods, so a
// literal rewrite to Assert.Equal would throw every one of those explanations
// away and leave "Assert.Equal() Failure" as the only thing a failing run says.
//
// This is that classic surface, implemented over Xunit.Assert, so the call
// sites keep both their shape and their diagnostics. Files that use it take a
// `using Assert = Ionic.Tests.Assert;` alias to pick this over Xunit.Assert.
//
// ------------------------------------------------------------------

using System;
using System.Collections;
using System.Globalization;

namespace Ionic.Tests
{
    /// <summary>
    ///   Classic (MSTest/NUnit-shaped) assertions implemented over xUnit.
    /// </summary>
    public static class Assert
    {
        private static readonly string NL = Environment.NewLine;

        #region Equality

        public static void AreEqual(object expected, object actual)
        {
            AreEqual(expected, actual, null);
        }

        public static void AreEqual(object expected, object actual, string message, params object[] args)
        {
            if (AreEquivalent(expected, actual)) return;

            Xunit.Assert.True(false,
                              Describe(message, args)
                              + NL + "  expected: " + Render(expected)
                              + NL + "  actual:   " + Render(actual));
        }

        public static void AreNotEqual(object expected, object actual)
        {
            AreNotEqual(expected, actual, null);
        }

        public static void AreNotEqual(object expected, object actual, string message, params object[] args)
        {
            if (!AreEquivalent(expected, actual)) return;

            Xunit.Assert.True(false,
                              Describe(message, args)
                              + NL + "  expected: not " + Render(expected)
                              + NL + "  actual:   " + Render(actual));
        }

        #endregion

        #region Booleans

        public static void IsTrue(bool condition)
        {
            IsTrue(condition, null);
        }

        public static void IsTrue(bool condition, string message, params object[] args)
        {
            Xunit.Assert.True(condition, Describe(message, args) + NL + "  expected the condition to be true");
        }

        public static void IsFalse(bool condition)
        {
            IsFalse(condition, null);
        }

        public static void IsFalse(bool condition, string message, params object[] args)
        {
            Xunit.Assert.False(condition, Describe(message, args) + NL + "  expected the condition to be false");
        }

        #endregion

        #region Null

        public static void IsNull(object value)
        {
            IsNull(value, null);
        }

        public static void IsNull(object value, string message, params object[] args)
        {
            Xunit.Assert.True(value == null,
                              Describe(message, args) + NL + "  expected null, actual: " + Render(value));
        }

        public static void IsNotNull(object value)
        {
            IsNotNull(value, null);
        }

        public static void IsNotNull(object value, string message, params object[] args)
        {
            Xunit.Assert.True(value != null, Describe(message, args) + NL + "  expected non-null, actual: null");
        }

        #endregion

        #region Outcomes

        public static void Fail()
        {
            Fail(null);
        }

        public static void Fail(string message, params object[] args)
        {
            Xunit.Assert.True(false, Describe(message, args));
        }

        /// <summary>
        ///   Skips the running test. Only takes effect on a test marked
        ///   <c>[SkippableFact]</c>; on a plain <c>[Fact]</c> it fails instead,
        ///   which is xUnit's own rule, not something this shim can soften.
        /// </summary>
        public static void Ignore(string message, params object[] args)
        {
            Xunit.Skip.If(true, Describe(message, args));
        }

        #endregion

        #region Comparison

        /// <summary>
        ///   Classic AreEqual semantics: numbers of different CLR types compare
        ///   by value, sequences compare element by element, everything else
        ///   falls back to Equals. xUnit's Equal&lt;T&gt; does none of the first
        ///   two, and the existing call sites rely on both.
        /// </summary>
        private static bool AreEquivalent(object expected, object actual)
        {
            if (ReferenceEquals(expected, actual)) return true;
            if (expected == null || actual == null) return false;

            if (IsNumeric(expected) && IsNumeric(actual))
                return Convert.ToDecimal(expected, CultureInfo.InvariantCulture)
                    == Convert.ToDecimal(actual, CultureInfo.InvariantCulture);

            if (!(expected is string) && !(actual is string)
                && expected is IEnumerable && actual is IEnumerable)
                return SequencesMatch((IEnumerable)expected, (IEnumerable)actual);

            return expected.Equals(actual);
        }

        private static bool SequencesMatch(IEnumerable expected, IEnumerable actual)
        {
            IEnumerator e = expected.GetEnumerator();
            IEnumerator a = actual.GetEnumerator();
            try
            {
                while (true)
                {
                    bool eNext = e.MoveNext();
                    bool aNext = a.MoveNext();
                    if (eNext != aNext) return false;
                    if (!eNext) return true;
                    if (!AreEquivalent(e.Current, a.Current)) return false;
                }
            }
            finally
            {
                (e as IDisposable)?.Dispose();
                (a as IDisposable)?.Dispose();
            }
        }

        private static bool IsNumeric(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Formatting

        private static string Describe(string message, object[] args)
        {
            if (String.IsNullOrEmpty(message)) return "Assertion failed.";
            if (args == null || args.Length == 0) return message;

            try
            {
                return String.Format(CultureInfo.InvariantCulture, message, args);
            }
            catch (FormatException)
            {
                // a message that merely looks like a format string is still a
                // useful message; better to show it verbatim than to fail here
                // and hide the assertion that actually broke.
                return message;
            }
        }

        private static string Render(object value)
        {
            if (value == null) return "null";
            if (value is string s) return "\"" + s + "\"";

            if (!(value is IEnumerable items)) return value.ToString();

            var sb = new System.Text.StringBuilder("[");
            int n = 0;
            foreach (object item in items)
            {
                if (n > 0) sb.Append(", ");
                if (n == 10) { sb.Append("..."); break; }
                sb.Append(Render(item));
                n++;
            }
            return sb.Append("]").ToString();
        }

        #endregion
    }
}
