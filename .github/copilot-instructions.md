# NinjaTrader.UnitTest AI Instructions

## Project Overview
NinjaTrader.UnitTest is a custom unit testing framework for NinjaTrader 8, modeled after Python's `unittest` module. It allows developers to write and run tests within the NinjaTrader environment.

## Architecture & Core Components
- **`TestCase`** ([TestCase.cs](TestCase.cs)): The base class for all test cases.
  - Uses reflection to execute a specific method by name (default: "RunTest").
  - Provides `SetUp()` and `TearDown()` hooks.
  - **Crucial**: Does NOT support attribute-based discovery (like `[Test]`). Tests are defined by method name strings.
- **`Assert`** ([Assert.cs](Assert.cs)): Provides assertion methods.
  - **Naming Convention**: Uses `AssertEqual`, `AssertTrue`, `AssertIs` (NOT `AreEqual`, `IsTrue`).
  - Throws generic `Exception` on failure, caught by `TestCase`.
- **`TestSuite`** ([TestSuite.cs](TestSuite.cs)): A collection of `TestCase` instances.
- **`TextTestRunner`** ([TextTestRunner.cs](TextTestRunner.cs)): Executes a `TestSuite` and logs results to the NinjaTrader output window (`NinjaTrader.NinjaScript.NinjaScript.Log`).

## Developer Workflow
1.  **Create Test Class**: Inherit from `NinjaTrader.UnitTest.TestCase`.
2.  **Define Constructor**: Must expose a constructor taking `string name` and passing it to `base(name)`.
3.  **Define Tests**: Public void methods (no arguments).
4.  **Execution**:
    - Manually instantiate the test class for *each* test method you want to run.
    - Add instances to a `TestSuite`.
    - Run using `TextTestRunner.Run(suite)`.

## Code Patterns & Examples

### Correct Test Class Structure
The README contains errors. Use this pattern instead:

```csharp
using NinjaTrader.UnitTest;

public class MyMathTests : TestCase
{
    // REQUIRED: Pass method name to base
    public MyMathTests(string methodName) : base(methodName) { }

    public void TestAddition()
    {
        int result = 2 + 2;
        // Note: AssertEqual, not AreEqual
        Assert.AssertEqual(4, result, "Addition failed");
    }
}
```

### Running Tests
Tests are not auto-discovered. You must compose them:

```csharp
public void RunMyTests()
{
    TestSuite suite = new TestSuite();
    
    // Explicitly register each test method by name
    suite.Add(new MyMathTests("TestAddition"));
    
    // Run and log to NT8 Output Window
    TextTestRunner.Run(suite);
}
```

## Build & Deploy
- **Framework**: .NET Framework 4.8 (`v4.8`).
- **Output**: `NinjaTrader.UnitTest.dll`.
- **Installation**:
  1. Build solution.
  2. Copy DLL to NinjaTrader 8 `bin/Custom` folder.
  3. Add Reference in NinjaScript Editor.

## Common Pitfalls
- **Method Names**: Ensure the string passed to the constructor EXACTLY matches the method name. Reflection is case-sensitive.
- **Assertions**: Do not use NUnit/MSTest syntax (`Assert.AreEqual`). Use `Assert.AssertEqual`.
- **Constructors**: If you don't define the constructor `(string name) : base(name)`, you can't run specific tests easily.
