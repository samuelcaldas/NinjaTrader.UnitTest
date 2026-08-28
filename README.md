# NinjaTrader.UnitTest

`NinjaTrader.UnitTest` is a lightweight, full-featured unit testing and mocking framework for NinjaTrader 8. Modeled after Python's `unittest` standard library (`TestCase`, `TestSuite`, `TestLoader`, `TextTestRunner`, `Assert`), it combines Python-style testing elegance with modern C# features and a dedicated **NinjaTrader Mocking & Harness Kit** for testing custom indicators, strategies, and add-ons.

---

## Features

- **Full Python `unittest` Parity:** `TestCase`, `TestSuite`, `TestLoader`, `TextTestRunner`, `SubTest`, dynamic skipping (`SkipTest`), expected failures (`[ExpectedFailure]`), and class fixtures (`SetUpClass` / `TearDownClass`).
- **Hybrid Test Discovery:** Automatically discover test methods starting with `Test*` / `test_*` or decorated with `[Test]` / `[TestMethod]`.
- **Complete Assertion Suite:** Full Python assertions (`AssertEqual`, `AssertRaises`, `AssertAlmostEqual`, `AssertIn`, `AssertIsNone`, `AssertGreater`, `AssertRegex`, `AssertSequenceEqual`) plus standard C# aliases (`AreEqual`, `Throws`, `AreAlmostEqual`, `Contains`, `IsNull`, `IsTrue`).
- **Strict Error vs. Failure Separation:** Assertions throw `AssertionException` (recorded as Failures), while unhandled runtime exceptions are recorded as Errors.
- **NinjaTrader Mocking & Harness Kit:**
  - `BarSeriesBuilder` & `MockBarSeries` for fluent OHLCV price series creation with NinjaTrader-style `Close(barsAgo)` indexing.
  - `MockInstrument` with tick rounding, tick calculations, and multi-asset presets (ES, MES, AAPL, EURUSD, BTCUSD).
  - `MockAccount` & `MockOrder` with position tracking, fill simulations, and realized/unrealized PnL.
  - `NinjaScriptTestHarness` for step-by-step indicator and strategy execution through state transitions and bars.
- **Pluggable Output Logging:** Logs automatically to `NinjaTrader.NinjaScript.NinjaScript.Log` inside NinjaTrader 8, and falls back gracefully to `Console` or `TextWriter` when running standalone or in CI/CD.

---

## Installation & Build

### Prerequisites
- Windows 10/11
- NinjaTrader 8 (64-bit)
- .NET Framework 4.8 / Visual Studio 2022 / MSBuild

### Build Commands
```powershell
# Build x64 (Recommended for NinjaTrader 8)
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="x64"

# Build AnyCPU
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="AnyCPU"
```

*Note:* The project automatically copies `NinjaTrader.UnitTest.dll` and `.pdb` to your `%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom\` directory on build.

---

## Quick Start & Usage

### 1. Authoring Unit Tests

Subclass `TestCase` and author your test methods:

```csharp
using System;
using System.Collections.Generic;
using NinjaTrader.UnitTest;

public class IndicatorCalculationTests : TestCase
{
    // Optional: parameterless constructor or (string name) constructor
    public IndicatorCalculationTests() : base() { }
    public IndicatorCalculationTests(string name) : base(name) { }

    public override void SetUp()
    {
        // Executed before each test
    }

    public override void TearDown()
    {
        // Executed after each test
    }

    public void TestMovingAverageCalculation()
    {
        double price1 = 5000.25;
        double price2 = 5000.75;
        double average = (price1 + price2) / 2.0;

        AssertEqual(5000.50, average);
        AssertAlmostEqual(5000.50, average, delta: 0.01);
    }

    public void TestExceptionHandling()
    {
        AssertRaises<DivideByZeroException>(() =>
        {
            int zero = 0;
            int result = 10 / zero;
        });
    }

    public void TestSubTests()
    {
        var numbers = new int[] { 2, 4, 6, 8 };
        foreach (var n in numbers)
        {
            SubTest($"Testing {n}", () =>
            {
                AssertEqual(0, n % 2);
            });
        }
    }

    [Skip("Waiting on API update")]
    public void TestPendingFeature()
    {
        // Skipped automatically
    }
}
```

### 2. Auto-Discovery & Running Tests in a NinjaTrader AddOn

```csharp
using NinjaTrader.UnitTest;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.NinjaScript.AddOns
{
    public class RunTestsAddon : AddOnBase
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "RunTestsAddon";
                Description = "Runs NinjaTrader Unit Tests";
            }
        }

        public void ExecuteTests()
        {
            // Auto-discover all test methods in IndicatorCalculationTests
            TestSuite suite = TestLoader.LoadTestsFromTestCase<IndicatorCalculationTests>();

            // Run suite and output results to the NinjaTrader Output window
            TestResult result = TextTestRunner.Run(suite, verbosity: 2);
        }
    }
}
```

---

## NinjaTrader Mocking & Harness Kit

### Mocking Bars & Price Series

```csharp
using NinjaTrader.UnitTest.Mocking;

var series = new BarSeriesBuilder("ES 03-26")
    .AddBar(open: 5000.0, high: 5010.0, low: 4995.0, close: 5005.0, volume: 1000)
    .AddBar(open: 5005.0, high: 5020.0, low: 5002.0, close: 5015.0, volume: 1500)
    .Build();

// NinjaTrader-style barsAgo indexing (0 = most recent)
double currentClose = series.Close(0); // 5015.0
double previousClose = series.Close(1); // 5005.0
```

### Mocking Instruments & PnL Calculations

```csharp
var es = MockInstrument.CreateFutures("ES", tickSize: 0.25, pointValue: 50.0);

// Round to tick
double rounded = es.RoundToTick(5000.22); // 5000.25

// Calculate PnL: 2 contracts long bought at 5000 and sold at 5010 = $1,000
double pnl = es.CalculatePnL(entryPrice: 5000.0, exitPrice: 5010.0, quantity: 2, isLong: true);
```

### Testing Indicators / Strategies with `NinjaScriptTestHarness`

```csharp
var bars = new BarSeriesBuilder("ES")
    .AddTrend(barCount: 10, startPrice: 5000, stepPerBar: 2.5)
    .Build();

var harness = new NinjaScriptTestHarness(bars);

harness.OnStateChange(state => {
    // Handle state transitions: SetDefaults -> Configure -> DataLoaded -> Historical
});

harness.OnBarUpdate(barIndex => {
    // Test indicator or strategy calculation per bar
});

harness.RunAllBars();
```

---

## Assertion Reference

| Python `unittest` Method | C# Alias | Description |
| :--- | :--- | :--- |
| `AssertEqual(exp, act)` | `AreEqual` | Checks that `exp` equals `act` |
| `AssertNotEqual(exp, act)` | `AreNotEqual` | Checks that `exp` does not equal `act` |
| `AssertTrue(cond)` | `IsTrue` | Asserts condition is true |
| `AssertFalse(cond)` | `IsFalse` | Asserts condition is false |
| `AssertIs(exp, act)` | `AreSame` | Checks reference equality |
| `AssertIsNot(exp, act)` | `AreNotSame` | Checks reference inequality |
| `AssertIsNone(obj)` | `IsNull` | Asserts object is null |
| `AssertIsNotNone(obj)` | `IsNotNull` | Asserts object is not null |
| `AssertIn(item, coll)` | `Contains` | Checks item is present in generic collection |
| `AssertNotIn(item, coll)` | `DoesNotContain` | Checks item is not in collection |
| `AssertRaises<T>(action)` | `Throws<T>` | Asserts exception of type `T` is thrown |
| `AssertAlmostEqual(exp, act, places, delta)` | `AreAlmostEqual` | Checks floating-point equality within places or delta |
| `AssertGreater(v1, v2)` | `Greater` | Checks `v1 > v2` |
| `AssertGreaterEqual(v1, v2)` | `GreaterOrEqual` | Checks `v1 >= v2` |
| `AssertLess(v1, v2)` | `Less` | Checks `v1 < v2` |
| `AssertLessEqual(v1, v2)` | `LessOrEqual` | Checks `v1 <= v2` |
| `AssertRegex(str, pat)` | - | Matches string against regex pattern |
| `AssertSequenceEqual(s1, s2)` | - | Verifies element-by-element equality of sequences |
| `AssertCountEqual(c1, c2)` | - | Verifies equal item multiset counts regardless of order |
| `AssertEmpty(coll)` | `IsEmpty` | Asserts collection has no items |
| `AssertNotEmpty(coll)` | `IsNotEmpty` | Asserts collection contains items |
| `Fail(msg)` | - | Explicitly fails the test |

---

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.
