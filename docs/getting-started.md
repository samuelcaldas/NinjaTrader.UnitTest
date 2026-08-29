# Getting Started with NinjaTrader.UnitTest

This guide walks you through setting up, building, deploying, and writing your first unit tests with **`NinjaTrader.UnitTest`**.

---

## Prerequisites

Before building or running `NinjaTrader.UnitTest`, ensure your environment meets the following requirements:

1. **Operating System:** Windows 10 or Windows 11 (64-bit).
2. **Target Runtime:** .NET Framework 4.8 Developer Pack.
3. **Build Tools:** Visual Studio 2022 (with .NET desktop build tools) or standalone MSBuild.
4. **Trading Platform:** [NinjaTrader 8](https://ninjatrader.com) (64-bit) installed.

---

## Building from Source

The solution [`NinjaTrader.UnitTest.sln`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/NinjaTrader.UnitTest.sln) compiles the framework assembly using MSBuild or Visual Studio.

### Using MSBuild via PowerShell

Open PowerShell and execute:

```powershell
# Build x64 Release (Recommended for NinjaTrader 8)
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="x64"

# Build AnyCPU Release
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="AnyCPU"
```

> [!TIP]
> Always compile for `x64` when deploying to 64-bit NinjaTrader 8 to match its native process architecture.

---

## Deployment to NinjaTrader 8

The project file [`NinjaTrader.UnitTest.csproj`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/NinjaTrader.UnitTest.csproj) contains an automated `PostBuildEvent` that copies the output assembly directly to your local NinjaTrader 8 Custom folder:

```
%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom\NinjaTrader.UnitTest.dll
%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom\NinjaTrader.UnitTest.pdb
```

### Manual Deployment (if needed)

If the automatic copy fails due to file locking while NinjaTrader is open, close NinjaTrader 8 and copy manually:

```powershell
Copy-Item "bin\x64\Release\NinjaTrader.UnitTest.dll" "$HOME\Documents\NinjaTrader 8\bin\Custom\" -Force
Copy-Item "bin\x64\Release\NinjaTrader.UnitTest.pdb" "$HOME\Documents\NinjaTrader 8\bin\Custom\" -Force
```

Once placed in `bin\Custom\`, the `NinjaTrader.UnitTest` namespace is automatically referenced by NinjaTrader 8's internal Roslyn compiler, enabling you to write test suites inside your custom Indicators, Strategies, and Add-Ons.

---

## Writing Your First Test Case

All unit test classes inherit from [`TestCase`](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/src/Execution/TestCase.cs). 

Here is a complete, self-contained test class demonstrating fixture setup, teardown, assertions, and skips:

```csharp
using System;
using System.Collections.Generic;
using NinjaTrader.UnitTest;

public class SimpleMovingAverageTests : TestCase
{
    private List<double> _prices;

    public override void SetUp()
    {
        // Executed before EACH test method
        _prices = new List<double> { 5000.25, 5002.50, 5001.75, 5005.00 };
    }

    public override void TearDown()
    {
        // Executed after EACH test method
        _prices.Clear();
    }

    public void TestMovingAverageCalculation()
    {
        double sum = 0;
        foreach (var price in _prices)
        {
            sum += price;
        }
        double sma = sum / _prices.Count;

        // Exact equality
        AssertEqual(5002.375, sma);

        // Approximate equality with tolerance
        AssertAlmostEqual(5002.38, sma, delta: 0.01);
    }

    public void TestExceptionThrownOnInvalidPeriod()
    {
        // Assert that an exception is raised
        AssertRaises<ArgumentException>(() =>
        {
            CalculateSMA(_prices, period: 0);
        });
    }

    [Skip("Awaiting exchange feed format update")]
    public void TestTickPrecision()
    {
        // This test is skipped automatically by the runner
    }

    private double CalculateSMA(List<double> data, int period)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be positive.", nameof(period));

        return 5000.0;
    }
}
```

---

## Executing Tests in NinjaTrader 8

To execute test suites directly within the NinjaTrader 8 desktop client and view formatted logs in the **NinjaTrader Output Window** (`Tools -> Output Window`), create a custom Add-On:

```csharp
using NinjaTrader.NinjaScript;
using NinjaTrader.UnitTest;

namespace NinjaTrader.NinjaScript.AddOns
{
    public class UnitTestRunnerAddOn : AddOnBase
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "NinjaTrader 8 In-Process Unit Test Runner";
                Name = "UnitTestRunnerAddOn";
            }
        }

        public void ExecuteTests()
        {
            // 1. Auto-discover all test cases in the current assembly
            TestSuite suite = TestLoader.LoadTestsFromAssembly(GetType().Assembly);

            // 2. Run tests with Verbose output (2) routed to the NT Log window
            TestResult result = TextTestRunner.Run(suite, verbosity: 2);

            // 3. Inspect results programmatically
            if (result.WasSuccessful)
            {
                Print($"[SUCCESS] All {result.TestsRun} tests passed!");
            }
            else
            {
                Print($"[FAILURE] Tests finished with {result.Failures.Count} failures and {result.Errors.Count} errors.");
            }
        }
    }
}
```

---

## Executing Tests Standalone / Headless

You can also run your tests in standalone C# console applications, headless runner scripts, or CI/CD pipelines without opening the NinjaTrader UI:

```csharp
using System;
using NinjaTrader.UnitTest;

class Program
{
    static int Main(string[] args)
    {
        // Discover test cases from a specific class
        TestSuite suite = TestLoader.LoadTestsFromTestCase<SimpleMovingAverageTests>();

        // Execute runner writing to Console.Out
        var runner = new TextTestRunner(verbosity: 2, stream: Console.Out);
        TestResult result = runner.Run(suite);

        // Return exit code (0 = success, 1 = failure)
        return result.WasSuccessful ? 0 : 1;
    }
}
```

---

## Next Steps

- Explore lifecycle hooks, skips, and subtests in **[Core Testing Concepts](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/docs/core-concepts.md)**.
- Learn about the full assertion suite in **[Comprehensive Assertion Reference](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/docs/assertions-reference.md)**.
- Discover synthetic market data, instruments, and order state machines in **[Mocking & Harness Kit](file:///C:/Users/samuel/source/repos/NT/refs/ninjatrader-unittest/docs/mocking-kit.md)**.
