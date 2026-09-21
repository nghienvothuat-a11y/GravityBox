#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.Globalization;

namespace GravityBox.Venom.ChapterProof
{
    // The generated player scenarios retain every assertion without shipping
    // Unity Test Framework or NUnit in the app. Unsupported assertions fail at
    // compilation instead of being silently ignored.
    internal static class Assert
    {
        internal static void Fail(string message) => throw new InvalidOperationException(message);
        internal static void IsTrue(bool value, string message = null) { if (!value) Fail(message ?? "Expected true."); }
        internal static void IsFalse(bool value, string message = null) => IsTrue(!value, message ?? "Expected false.");
        internal static void NotNull(object value, string message = null)
        { IsTrue(value != null && (!(value is UnityEngine.Object unityObject) || unityObject != null), message ?? "Expected non-null value."); }
        internal static void IsEmpty(string value, string message = null) => IsTrue(value != null && value.Length == 0, message ?? "Expected empty string.");
        internal static void AreEqual(object expected, object actual, string message = null)
        { IsTrue(Equals(expected, actual), (message ?? "Values differ.") + $" Expected {expected}, actual {actual}."); }
        internal static void AreNotEqual(object expected, object actual, string message = null)
        { IsTrue(!Equals(expected, actual), message ?? "Expected different values."); }
        internal static void Less(double actual, double expected, string message = null)
        { IsTrue(actual < expected, (message ?? "Expected a smaller value.") + $" {actual} < {expected}"); }
        internal static void Greater(double actual, double expected, string message = null)
        { IsTrue(actual > expected, (message ?? "Expected a larger value.") + $" {actual} > {expected}"); }
        internal static void That(object actual, EqualConstraint expected, string message = null)
        { IsTrue(expected.Matches(actual), (message ?? "Equality constraint failed.") + $" Actual {actual}; {expected}."); }
    }

    internal static class Is
    {
        internal static EqualConstraint EqualTo(object expected) => new EqualConstraint(expected);
    }

    internal sealed class EqualConstraint
    {
        private readonly object expected;
        private double? tolerance;
        internal EqualConstraint(object expected) { this.expected = expected; }
        internal EqualConstraint Within(double delta) { tolerance = delta; return this; }
        internal bool Matches(object actual) => tolerance.HasValue
            ? Math.Abs(Convert.ToDouble(actual, CultureInfo.InvariantCulture) - Convert.ToDouble(expected, CultureInfo.InvariantCulture)) <= tolerance.Value
            : Equals(expected, actual);
        public override string ToString() => $"Expected {expected}, tolerance {tolerance}";
    }
}
#endif
