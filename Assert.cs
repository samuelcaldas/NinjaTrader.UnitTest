using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.UnitTest
{
    public static class Assert
    {
        public static void AreEqual(object expected, object actual, string message = null)
        {
            if (!expected.Equals(actual))
            {
                Fail(message ?? $"AssertionError: {expected} != {actual}");
            }
        }

        public static void AreNotEqual(object expected, object actual, string message = null)
        {
            if (expected.Equals(actual))
            {
                Fail(message ?? $"AssertionError: {expected} == {actual}");
            }
        }

        public static void IsTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                Fail(message ?? "AssertionError: Condition is not true");
            }
        }

        public static void IsFalse(bool condition, string message = null)
        {
            if (condition)
            {
                Fail(message ?? "AssertionError: Condition is not false");
            }
        }

        public static void Is(object expected, object actual, string message = null)
        {
            if (!ReferenceEquals(expected, actual))
            {
                Fail(message ?? $"AssertionError: {expected} is not the same object as {actual}");
            }
        }

        public static void IsNot(object expected, object actual, string message = null)
        {
            if (ReferenceEquals(expected, actual))
            {
                Fail(message ?? $"AssertionError: {expected} is the same object as {actual}");
            }
        }

        public static void IsNone(object obj, string message = null)
        {
            if (obj != null)
            {
                Fail(message ?? $"AssertionError: {obj} is not None");
            }
        }

        public static void IsNotNone(object obj, string message = null)
        {
            if (obj == null)
            {
                Fail(message ?? "AssertionError: object is None");
            }
        }

        public static void IsIn(object item, object collection, string message = null)
        {
            if (!((System.Collections.IList)collection).Contains(item))
            {
                Fail(message ?? $"AssertionError: {item} not found in {collection}");
            }
        }

        public static void IsNotIn(object item, object collection, string message = null)
        {
            if (((System.Collections.IList)collection).Contains(item))
            {
                Fail(message ?? $"AssertionError: {item} found in {collection}");
            }
        }

        public static void IsInstance(object obj, Type type, string message = null)
        {
            if (!(type.IsInstanceOfType(obj)))
            {
                Fail(message ?? $"AssertionError: {obj} is not an instance of {type}");
            }
        }

        public static void IsNotInstance(object obj, Type type, string message = null)
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
