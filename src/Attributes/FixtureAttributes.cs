using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Marks a method to be executed before each test method in the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SetUpAttribute : Attribute { }

    /// <summary>
    /// Marks a method to be executed after each test method in the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TearDownAttribute : Attribute { }

    /// <summary>
    /// Marks a static method to be executed once before any test in the class runs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SetUpClassAttribute : Attribute { }

    /// <summary>
    /// Marks a static method to be executed once after all tests in the class have finished.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TearDownClassAttribute : Attribute { }
}
