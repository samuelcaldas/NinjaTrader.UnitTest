using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.UnitTest
{
    public class Assert
    {
        public static void AssertEqual(object expected, object actual, string message = null)
        {
            if (!expected.Equals(actual))
            {
                Fail(message ?? $"AssertionError: {expected} != {actual}");
            }
        }

        public static void AssertNotEqual(object expected, object actual, string message = null)
        {
            if (expected.Equals(actual))
            {
                Fail(message ?? $"AssertionError: {expected} == {actual}");
            }
        }

        public static void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                Fail(message ?? "AssertionError: Condition is not true");
            }
        }

        public static void AssertFalse(bool condition, string message = null)
        {
            if (condition)
            {
                Fail(message ?? "AssertionError: Condition is not false");
            }
        }

        public static void AssertIs(object expected, object actual, string message = null)
        {
            if (!ReferenceEquals(expected, actual))
            {
                Fail(message ?? $"AssertionError: {expected} is not the same object as {actual}");
            }
        }

        public static void AssertIsNot(object expected, object actual, string message = null)
        {
            if (ReferenceEquals(expected, actual))
            {
                Fail(message ?? $"AssertionError: {expected} is the same object as {actual}");
            }
        }

        public static void AssertIsNone(object obj, string message = null)
        {
            if (obj != null)
            {
                Fail(message ?? $"AssertionError: {obj} is not None");
            }
        }

        public static void AssertIsNotNone(object obj, string message = null)
        {
            if (obj == null)
            {
                Fail(message ?? "AssertionError: object is None");
            }
        }

        public static void AssertIn(object item, object collection, string message = null)
        {
            if (!((System.Collections.IList)collection).Contains(item))
            {
                Fail(message ?? $"AssertionError: {item} not found in {collection}");
            }
        }

        public static void AssertNotIn(object item, object collection, string message = null)
        {
            if (((System.Collections.IList)collection).Contains(item))
            {
                Fail(message ?? $"AssertionError: {item} found in {collection}");
            }
        }

        public static void AssertIsInstance(object obj, Type type, string message = null)
        {
            if (!(type.IsInstanceOfType(obj)))
            {
                Fail(message ?? $"AssertionError: {obj} is not an instance of {type}");
            }
        }

        public static void AssertNotIsInstance(object obj, Type type, string message = null)
        {
            if (type.IsInstanceOfType(obj))
            {
                Fail(message ?? $"AssertionError: {obj} is an instance of {type}");
            }
        }

        // Outros métodos de assert podem ser adicionados aqui

        private static void Fail(string message)
        {
            NinjaScript.NinjaScript.Log(message, LogLevel.Error);
            throw new Exception(message);
        }
    }
}
