using NinjaTrader.Cbi;
using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    public interface IAssert
    {
    }

    public partial class Assert : IAssert
    {
        public void AssertEqual<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                Fail(message ?? $"AssertionError: {expected} != {actual}");
        }

        public void AssertNotEqual<T>(T expected, T actual, string message = null)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
                Fail(message ?? $"AssertionError: {expected} == {actual}");
        }

        public void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
                Fail(message ?? "AssertionError: Condition is not true");
        }

        public void AssertFalse(bool condition, string message = null)
        {
            if (condition)
                Fail(message ?? "AssertionError: Condition is not false");
        }

        public void AssertIs<T>(T expected, T actual, string message = null)
        {
            if (!ReferenceEquals(expected, actual))
                Fail(message ?? $"AssertionError: {expected} is not the same T as {actual}");
        }

        public void AssertIsNot<T>(T expected, T actual, string message = null)
        {
            if (ReferenceEquals(expected, actual))
                Fail(message ?? $"AssertionError: {expected} is the same T as {actual}");
        }
    }

    public partial class Assert : IAssert
    {
        public void AssertIsNone<T>(T obj, string message = null)
        {
            if (obj != null)
                Fail(message ?? $"AssertionError: {obj} is not None");
        }

        public void AssertIsNotNone<T>(T obj, string message = null)
        {
            if (obj == null)
                Fail(message ?? "AssertionError: T is None");
        }

        public void AssertIn<T>(T item, IEnumerable<T> collection, string message = null)
        {
            if (!((System.Collections.IList)collection).Contains(item))
                Fail(message ?? $"AssertionError: {item} not found in {collection}");
        }

        public void AssertNotIn<T>(T item, IEnumerable<T> collection, string message = null)
        {
            if (((System.Collections.IList)collection).Contains(item))
                Fail(message ?? $"AssertionError: {item} found in {collection}");
        }

        public void AssertIsInstance(object obj, Type type, string message = null)
        {
            if (!(type.IsInstanceOfType(obj)))
                Fail(message ?? $"AssertionError: {obj} is not an instance of {type}");
        }

        public void AssertNotIsInstance(object obj, Type type, string message = null)
        {
            if (type.IsInstanceOfType(obj))
                Fail(message ?? $"AssertionError:{obj} is an instance of{type}");
        }
    }

    public partial class Assert : IAssert
    {
        private static void Fail(string message)
        {
            // NinjaTrader.NinjaScript.NinjaScript.Log(message, LogLevel.Error);
            throw new Exception(message);
        }
    }
}