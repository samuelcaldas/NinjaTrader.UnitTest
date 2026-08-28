using System;
using NinjaTrader.UnitTest;

namespace NinjaTrader.UnitTest.Tests.Assertions
{
    public class BasicAssertionTests : TestCase
    {
        public void TestAssertEqualAndNotEqual()
        {
            AssertEqual(42, 42);
            AssertEqual("hello", "hello");
            AssertNotEqual(42, 99);
            AssertNotEqual("apple", "orange");

            // Aliases
            AreEqual(100, 100);
            AreNotEqual("foo", "bar");
        }

        public void TestAssertTrueAndFalse()
        {
            AssertTrue(1 + 1 == 2);
            AssertFalse(1 + 1 == 3);

            // Aliases
            IsTrue(true);
            IsFalse(false);
        }

        public void TestAssertIsAndIsNot()
        {
            var obj1 = new object();
            var obj2 = obj1;
            var obj3 = new object();

            AssertIs(obj1, obj2);
            AssertIsNot(obj1, obj3);

            // Aliases
            AreSame(obj1, obj2);
            AreNotSame(obj1, obj3);
        }

        public void TestAssertIsNoneAndIsNotNone()
        {
            string nullStr = null;
            string validStr = "ninja";

            AssertIsNone(nullStr);
            AssertIsNotNone(validStr);

            // Aliases
            IsNull(nullStr);
            IsNotNull(validStr);
        }

        public void TestAssertIsInstance()
        {
            object text = "Sample Text";
            AssertIsInstance<string>(text);
            AssertIsInstance(text, typeof(string));
            AssertNotIsInstance<int>(text);

            // Aliases
            IsInstanceOfType(text, typeof(string));
            IsNotInstanceOfType(text, typeof(int));
        }
    }
}
