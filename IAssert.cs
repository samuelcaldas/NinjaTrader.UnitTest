using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Contract for test assertion utilities.
    /// </summary>
    public interface IAssert
    {
        void AssertEqual<T>(T expected, T actual, string message = null);
        void AssertNotEqual<T>(T expected, T actual, string message = null);
        void AssertTrue(bool condition, string message = null);
        void AssertFalse(bool condition, string message = null);
        void AssertIs<T>(T expected, T actual, string message = null);
        void AssertIsNot<T>(T expected, T actual, string message = null);
        void AssertIsNone<T>(T obj, string message = null);
        void AssertIsNotNone<T>(T obj, string message = null);
        void AssertIn<T>(T item, IEnumerable<T> collection, string message = null);
        void AssertNotIn<T>(T item, IEnumerable<T> collection, string message = null);
        void AssertIsInstance(object obj, Type type, string message = null);
        void AssertIsInstance<TExpected>(object obj, string message = null);
        void AssertNotIsInstance(object obj, Type type, string message = null);
        void AssertNotIsInstance<TExpected>(object obj, string message = null);
        TException AssertRaises<TException>(Action action, string message = null) where TException : Exception;
        void AssertAlmostEqual(double expected, double actual, int places = 7, double? delta = null, string message = null);
        void AssertNotAlmostEqual(double expected, double actual, int places = 7, double? delta = null, string message = null);
        void Fail(string message = "Assertion failed");
    }
}
