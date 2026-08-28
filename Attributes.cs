using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Marks a method as a test method to be executed by the test runner.
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
    /// Alias for TestAttribute for compatibility with MSTest/NUnit conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestMethodAttribute : TestAttribute
    {
        public TestMethodAttribute(string description = null) : base(description) { }
    }

    /// <summary>
    /// Unconditionally skips the decorated test method or test class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SkipAttribute : Attribute
    {
        public string Reason { get; }

        public SkipAttribute(string reason = "Test skipped")
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// Skips the test if the specified boolean property/method on the test class evaluates to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SkipIfAttribute : Attribute
    {
        public string ConditionMemberName { get; }
        public string Reason { get; }

        public SkipIfAttribute(string conditionMemberName, string reason = "Test skipped due to condition")
        {
            ConditionMemberName = conditionMemberName;
            Reason = reason;
        }
    }

    /// <summary>
    /// Skips the test unless the specified boolean property/method on the test class evaluates to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SkipUnlessAttribute : Attribute
    {
        public string ConditionMemberName { get; }
        public string Reason { get; }

        public SkipUnlessAttribute(string conditionMemberName, string reason = "Test skipped due to condition")
        {
            ConditionMemberName = conditionMemberName;
            Reason = reason;
        }
    }

    /// <summary>
    /// Marks a test method as an expected failure. If the test fails, it will not count as a failure.
    /// If the test passes, it will count as an unexpected success.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ExpectedFailureAttribute : Attribute
    {
        public string Reason { get; }

        public ExpectedFailureAttribute(string reason = null)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// Marks a method to be run before each test method in the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SetUpAttribute : Attribute { }

    /// <summary>
    /// Marks a method to be run after each test method in the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TearDownAttribute : Attribute { }

    /// <summary>
    /// Marks a static method to be run once before any tests in the class are executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SetUpClassAttribute : Attribute { }

    /// <summary>
    /// Marks a static method to be run once after all tests in the class have finished.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TearDownClassAttribute : Attribute { }
}
