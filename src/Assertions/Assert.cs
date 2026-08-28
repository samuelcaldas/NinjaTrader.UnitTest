using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Base assertion provider implementing Python unittest assertions and C# testing aliases.
    /// </summary>
    public class Assert : IAssert
    {
        #region Python unittest-style Assertions

        public static void AssertEqual<T>(T expected, T actual, string message = null)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
                return;

            Fail(message ?? $"AssertionError: expected {FormatValue(expected)}, but got {FormatValue(actual)}");
        }

        public static void AssertNotEqual<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                return;

            Fail(message ?? $"AssertionError: unexpected {FormatValue(actual)} (should not equal {FormatValue(expected)})");
        }

        public static void AssertTrue(bool condition, string message = null)
        {
            if (condition)
                return;

            Fail(message ?? "AssertionError: Condition is not true");
        }

        public static void AssertFalse(bool condition, string message = null)
        {
            if (!condition)
                return;

            Fail(message ?? "AssertionError: Condition is not false");
        }

        public static void AssertIs<T>(T expected, T actual, string message = null)
        {
            if (ReferenceEquals(expected, actual))
                return;

            Fail(message ?? $"AssertionError: {FormatValue(expected)} is not the same reference as {FormatValue(actual)}");
        }

        public static void AssertIsNot<T>(T expected, T actual, string message = null)
        {
            if (!ReferenceEquals(expected, actual))
                return;

            Fail(message ?? $"AssertionError: {FormatValue(expected)} is the same reference as {FormatValue(actual)}");
        }

        public static void AssertIsNone<T>(T obj, string message = null)
        {
            if (obj == null)
                return;

            Fail(message ?? $"AssertionError: {FormatValue(obj)} is not null (None)");
        }

        public static void AssertIsNotNone<T>(T obj, string message = null)
        {
            if (obj != null)
                return;

            Fail(message ?? "AssertionError: Object is null (None)");
        }

        public static void AssertIn<T>(T item, IEnumerable<T> collection, string message = null)
        {
            if (collection == null)
                Fail(message ?? $"AssertionError: Collection is null when checking for item {FormatValue(item)}");

            if (ContainsItem(collection, item))
                return;

            Fail(message ?? $"AssertionError: Item {FormatValue(item)} not found in collection");
        }

        public static void AssertNotIn<T>(T item, IEnumerable<T> collection, string message = null)
        {
            if (collection == null)
                return;

            if (!ContainsItem(collection, item))
                return;

            Fail(message ?? $"AssertionError: Item {FormatValue(item)} found in collection");
        }

        public static void AssertIsInstance(object obj, Type type, string message = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (obj != null && type.IsInstanceOfType(obj))
                return;

            Fail(message ?? $"AssertionError: {FormatValue(obj)} is not an instance of {type.FullName}");
        }

        public static void AssertIsInstance<TExpected>(object obj, string message = null)
        {
            AssertIsInstance(obj, typeof(TExpected), message);
        }

        public static void AssertNotIsInstance(object obj, Type type, string message = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (obj == null || !type.IsInstanceOfType(obj))
                return;

            Fail(message ?? $"AssertionError: {FormatValue(obj)} is an instance of {type.FullName}");
        }

        public static void AssertNotIsInstance<TExpected>(object obj, string message = null)
        {
            AssertNotIsInstance(obj, typeof(TExpected), message);
        }

        public static TException AssertRaises<TException>(Action action, string message = null) where TException : Exception
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                Fail(message ?? $"AssertionError: Expected exception {typeof(TException).Name} but caught {ex.GetType().Name}: {ex.Message}");
            }

            Fail(message ?? $"AssertionError: Expected exception {typeof(TException).Name} was not thrown");
            return null;
        }

        public static Exception AssertRaises(Type exceptionType, Action action, string message = null)
        {
            if (exceptionType == null)
                throw new ArgumentNullException(nameof(exceptionType));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (exceptionType.IsInstanceOfType(ex))
                    return ex;

                Fail(message ?? $"AssertionError: Expected exception {exceptionType.Name} but caught {ex.GetType().Name}: {ex.Message}");
            }

            Fail(message ?? $"AssertionError: Expected exception {exceptionType.Name} was not thrown");
            return null;
        }

        public static TException AssertRaisesRegex<TException>(Action action, string pattern, string message = null) where TException : Exception
        {
            var ex = AssertRaises<TException>(action, message);
            if (!Regex.IsMatch(ex.Message, pattern))
                Fail(message ?? $"AssertionError: Exception message \"{ex.Message}\" does not match regex pattern \"{pattern}\"");
            return ex;
        }

        public static void AssertAlmostEqual(double expected, double actual, int places = 7, double? delta = null, string message = null)
        {
            double diff = Math.Abs(expected - actual);
            if (delta.HasValue)
            {
                if (diff > delta.Value)
                    Fail(message ?? $"AssertionError: {expected} != {actual} within delta {delta.Value} (diff={diff})");
                return;
            }

            double maxDiff = Math.Pow(10, -places);
            if (diff >= maxDiff)
                Fail(message ?? $"AssertionError: {expected} != {actual} within {places} places (diff={diff})");
        }

        public static void AssertNotAlmostEqual(double expected, double actual, int places = 7, double? delta = null, string message = null)
        {
            double diff = Math.Abs(expected - actual);
            if (delta.HasValue)
            {
                if (diff <= delta.Value)
                    Fail(message ?? $"AssertionError: {expected} == {actual} within delta {delta.Value} (diff={diff})");
                return;
            }

            double maxDiff = Math.Pow(10, -places);
            if (diff < maxDiff)
                Fail(message ?? $"AssertionError: {expected} == {actual} within {places} places (diff={diff})");
        }

        public static void AssertGreater<T>(T val1, T val2, string message = null) where T : IComparable<T>
        {
            if (val1 != null && val1.CompareTo(val2) > 0)
                return;

            Fail(message ?? $"AssertionError: {FormatValue(val1)} is not greater than {FormatValue(val2)}");
        }

        public static void AssertGreaterEqual<T>(T val1, T val2, string message = null) where T : IComparable<T>
        {
            if (val1 != null && val1.CompareTo(val2) >= 0)
                return;

            Fail(message ?? $"AssertionError: {FormatValue(val1)} is not greater than or equal to {FormatValue(val2)}");
        }

        public static void AssertLess<T>(T val1, T val2, string message = null) where T : IComparable<T>
        {
            if (val1 != null && val1.CompareTo(val2) < 0)
                return;

            Fail(message ?? $"AssertionError: {FormatValue(val1)} is not less than {FormatValue(val2)}");
        }

        public static void AssertLessEqual<T>(T val1, T val2, string message = null) where T : IComparable<T>
        {
            if (val1 != null && val1.CompareTo(val2) <= 0)
                return;

            Fail(message ?? $"AssertionError: {FormatValue(val1)} is not less than or equal to {FormatValue(val2)}");
        }

        public static void AssertRegex(string text, string pattern, string message = null)
        {
            if (text != null && Regex.IsMatch(text, pattern))
                return;

            Fail(message ?? $"AssertionError: Regex pattern \"{pattern}\" not found in \"{text}\"");
        }

        public static void AssertNotRegex(string text, string pattern, string message = null)
        {
            if (text == null || !Regex.IsMatch(text, pattern))
                return;

            Fail(message ?? $"AssertionError: Regex pattern \"{pattern}\" unexpectedly matched in \"{text}\"");
        }

        public static void AssertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null)
                Fail(message ?? $"AssertionError: One sequence is null while the other is not");

            var expectedList = expected.ToList();
            var actualList = actual.ToList();

            if (expectedList.Count != actualList.Count)
                Fail(message ?? $"AssertionError: Sequence counts differ. Expected {expectedList.Count}, got {actualList.Count}");

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < expectedList.Count; i++)
            {
                if (!comparer.Equals(expectedList[i], actualList[i]))
                    Fail(message ?? $"AssertionError: Sequences differ at index {i}. Expected {FormatValue(expectedList[i])}, got {FormatValue(actualList[i])}");
            }
        }

        public static void AssertCountEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null)
                Fail(message ?? $"AssertionError: One collection is null while the other is not");

            var expectedCounts = GetElementCounts(expected);
            var actualCounts = GetElementCounts(actual);

            if (expectedCounts.Count != actualCounts.Count)
                Fail(message ?? $"AssertionError: Distinct element counts differ. Expected {expectedCounts.Count}, got {actualCounts.Count}");

            foreach (var kvp in expectedCounts)
            {
                if (!actualCounts.TryGetValue(kvp.Key, out int count) || count != kvp.Value)
                    Fail(message ?? $"AssertionError: Element count mismatch for {FormatValue(kvp.Key)}. Expected {kvp.Value}, got {count}");
            }
        }

        public static void AssertEmpty<T>(IEnumerable<T> collection, string message = null)
        {
            if (collection == null || !collection.Any())
                return;

            Fail(message ?? $"AssertionError: Expected collection to be empty, but had elements");
        }

        public static void AssertNotEmpty<T>(IEnumerable<T> collection, string message = null)
        {
            if (collection != null && collection.Any())
                return;

            Fail(message ?? $"AssertionError: Expected collection to not be empty");
        }

        public static void Fail(string message = "Assertion failed")
        {
            throw new AssertionException(message);
        }

        #endregion

        #region C# / NUnit / MSTest Compatibility Aliases

        public static void AreEqual<T>(T expected, T actual, string message = null) => AssertEqual(expected, actual, message);
        public static void AreNotEqual<T>(T expected, T actual, string message = null) => AssertNotEqual(expected, actual, message);
        public static void IsTrue(bool condition, string message = null) => AssertTrue(condition, message);
        public static void IsFalse(bool condition, string message = null) => AssertFalse(condition, message);
        public static void AreSame<T>(T expected, T actual, string message = null) => AssertIs(expected, actual, message);
        public static void AreNotSame<T>(T expected, T actual, string message = null) => AssertIsNot(expected, actual, message);
        public static void IsNull<T>(T obj, string message = null) => AssertIsNone(obj, message);
        public static void IsNotNull<T>(T obj, string message = null) => AssertIsNotNone(obj, message);
        public static void Contains<T>(T item, IEnumerable<T> collection, string message = null) => AssertIn(item, collection, message);
        public static void DoesNotContain<T>(T item, IEnumerable<T> collection, string message = null) => AssertNotIn(item, collection, message);
        public static TException Throws<TException>(Action action, string message = null) where TException : Exception => AssertRaises<TException>(action, message);
        public static Exception Throws(Type exceptionType, Action action, string message = null) => AssertRaises(exceptionType, action, message);
        public static void AreAlmostEqual(double expected, double actual, int places = 7, double? delta = null, string message = null) => AssertAlmostEqual(expected, actual, places, delta, message);
        public static void IsEmpty<T>(IEnumerable<T> collection, string message = null) => AssertEmpty(collection, message);
        public static void IsNotEmpty<T>(IEnumerable<T> collection, string message = null) => AssertNotEmpty(collection, message);
        public static void IsInstanceOfType(object obj, Type type, string message = null) => AssertIsInstance(obj, type, message);
        public static void IsNotInstanceOfType(object obj, Type type, string message = null) => AssertNotIsInstance(obj, type, message);
        public static void Greater<T>(T val1, T val2, string message = null) where T : IComparable<T> => AssertGreater(val1, val2, message);
        public static void GreaterOrEqual<T>(T val1, T val2, string message = null) where T : IComparable<T> => AssertGreaterEqual(val1, val2, message);
        public static void Less<T>(T val1, T val2, string message = null) where T : IComparable<T> => AssertLess(val1, val2, message);
        public static void LessOrEqual<T>(T val1, T val2, string message = null) where T : IComparable<T> => AssertLessEqual(val1, val2, message);

        #endregion

        #region Instance Interface Implementations

        void IAssert.AssertEqual<T>(T expected, T actual, string message) => AssertEqual(expected, actual, message);
        void IAssert.AssertNotEqual<T>(T expected, T actual, string message) => AssertNotEqual(expected, actual, message);
        void IAssert.AssertTrue(bool condition, string message) => AssertTrue(condition, message);
        void IAssert.AssertFalse(bool condition, string message) => AssertFalse(condition, message);
        void IAssert.AssertIs<T>(T expected, T actual, string message) => AssertIs(expected, actual, message);
        void IAssert.AssertIsNot<T>(T expected, T actual, string message) => AssertIsNot(expected, actual, message);
        void IAssert.AssertIsNone<T>(T obj, string message) => AssertIsNone(obj, message);
        void IAssert.AssertIsNotNone<T>(T obj, string message) => AssertIsNotNone(obj, message);
        void IAssert.AssertIn<T>(T item, IEnumerable<T> collection, string message) => AssertIn(item, collection, message);
        void IAssert.AssertNotIn<T>(T item, IEnumerable<T> collection, string message) => AssertNotIn(item, collection, message);
        void IAssert.AssertIsInstance(object obj, Type type, string message) => AssertIsInstance(obj, type, message);
        void IAssert.AssertIsInstance<TExpected>(object obj, string message) => AssertIsInstance<TExpected>(obj, message);
        void IAssert.AssertNotIsInstance(object obj, Type type, string message) => AssertNotIsInstance(obj, type, message);
        void IAssert.AssertNotIsInstance<TExpected>(object obj, string message) => AssertNotIsInstance<TExpected>(obj, message);
        TException IAssert.AssertRaises<TException>(Action action, string message) => AssertRaises<TException>(action, message);
        void IAssert.AssertAlmostEqual(double expected, double actual, int places, double? delta, string message) => AssertAlmostEqual(expected, actual, places, delta, message);
        void IAssert.AssertNotAlmostEqual(double expected, double actual, int places, double? delta, string message) => AssertNotAlmostEqual(expected, actual, places, delta, message);
        void IAssert.Fail(string message) => Fail(message);

        #endregion

        #region Private Helpers

        private static bool ContainsItem<T>(IEnumerable<T> collection, T item)
        {
            var comparer = EqualityComparer<T>.Default;
            foreach (var element in collection)
            {
                if (comparer.Equals(element, item))
                    return true;
            }
            return false;
        }

        private static string FormatValue(object val)
        {
            if (val == null) return "null";
            if (val is string s) return $"\"{s}\"";
            return val.ToString();
        }

        private static Dictionary<T, int> GetElementCounts<T>(IEnumerable<T> collection)
        {
            var dict = new Dictionary<T, int>();
            foreach (var item in collection)
            {
                if (dict.TryGetValue(item, out int count))
                    dict[item] = count + 1;
                else
                    dict[item] = 1;
            }
            return dict;
        }

        #endregion
    }
}
