using System;
using NinjaTrader.Cbi;

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

        // Outros métodos de assert podem ser adicionados aqui

        private static void Fail(string message)
        {
            NinjaScript.Log(message, LogLevel.Error);
            throw new Exception(message);
        }
    }

}
