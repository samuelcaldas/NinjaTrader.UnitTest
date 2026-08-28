using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Marks a method as an executable test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestAttribute : Attribute
    {
        public string Description { get; set; }

        public TestAttribute(string description = null)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Alias for TestAttribute matching MSTest/NUnit conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestMethodAttribute : TestAttribute
    {
        public TestMethodAttribute(string description = null) : base(description) { }
    }
}
