using System;

namespace NinjaTrader.UnitTest
{
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
    /// Skips the test if the specified condition property or method returns true.
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
    /// Skips the test unless the specified condition property or method returns true.
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
}
