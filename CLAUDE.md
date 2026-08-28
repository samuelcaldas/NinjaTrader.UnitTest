# GEMINI.md

This file provides guidance to Gemini / Antigravity when working with code in this repository.

## Project Overview
`NinjaTrader.UnitTest` is a full-featured unit testing framework and mocking kit for NinjaTrader 8, modeled after Python's `unittest` standard library (`TestCase`, `TestSuite`, `TestLoader`, `TextTestRunner`, `Assert`, `SubTest`). It is built as a .NET Framework 4.8 class library targeting NinjaTrader 8 and logs directly to the NinjaTrader output window (`NinjaTrader.NinjaScript.NinjaScript.Log`) or falls back to `Console` / `TextWriter` for headless execution and CI/CD.

## Build & Deployment Commands

Built using Visual Studio or MSBuild (.NET Framework 4.8):

```powershell
# Build x64 (Recommended for NinjaTrader 8)
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="x64"

# Build AnyCPU (Debug/Release)
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="AnyCPU"
```

*Note on Deployment:* `NinjaTrader.UnitTest.csproj` has a `PostBuildEvent` that automatically copies `NinjaTrader.UnitTest.dll` and `.pdb` to the local user's `%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom\` folder.

## Architecture & Core Components

All types reside in `NinjaTrader.UnitTest` and `NinjaTrader.UnitTest.Mocking`:

- **`TestCase` (`TestCase.cs`):** Base test class subclassing `Assert`.
  - Executes test methods via reflection.
  - Lifecycle hooks: `SetUp()`, `TearDown()`, `SetUpClass()`, `TearDownClass()`, `AddCleanup(action)`.
  - Dynamic skipping (`SkipTest(reason)`), expected failures (`[ExpectedFailure]`), and `SubTest(msg, action)`.
  - Distinguishes between `AssertionException` (Failures) and unexpected runtime crashes (Errors).
- **`TestLoader` (`TestLoader.cs`):** Hybrid test discovery via `Test*` / `test_*` naming conventions or `[Test]` / `[TestMethod]` attributes (`TestLoader.LoadTestsFromTestCase<T>()`, `TestLoader.LoadTestsFromAssembly(asm)`).
- **`Assert` (`Assert.cs`):** Assertion utility with full Python unittest methods (`AssertEqual`, `AssertRaises`, `AssertAlmostEqual`, `AssertIsNone`, `AssertIn`, `AssertGreater`, `AssertRegex`, etc.) and standard C# aliases (`AreEqual`, `Throws`, `AreAlmostEqual`, `IsNull`, `Contains`, `IsTrue`).
- **`TestSuite` (`TestSuite.cs`):** Composite aggregator of test cases and nested suites, coordinating class-level `SetUpClass` and `TearDownClass` fixtures.
- **`TestResult` (`TestResult.cs`):** Tracks successes, failures, errors, skips, expected failures, subtests, and outputs via `ITestOutput`.
- **`TextTestRunner` (`TextTestRunner.cs`):** Runner entry point supporting verbosity, failfast, custom output streams, and summary reports.
- **`NinjaTrader.UnitTest.Mocking`:**
  - `BarSeriesBuilder` & `MockBarSeries`: Fluent OHLCV price series generator with NinjaTrader-style `Close(barsAgo)` indexing.
  - `MockInstrument`: Instrument specs, tick rounding, tick calculations, and asset presets (ES, MES, AAPL, EURUSD, BTCUSD).
  - `MockAccount` & `MockOrder`: Position tracking, fill simulations, and realized/unrealized PnL.
  - `NinjaScriptTestHarness`: State lifecycle and bar-by-bar execution harness for indicators and strategies.

## Dependencies
- **Target Framework:** .NET Framework 4.8 (`v4.8`), C# 7.3 (`x64`).
- **Binary Dependencies:** `NinjaTrader.Core.dll` and `NinjaTrader.Gui.dll` (located at `C:\Program Files\NinjaTrader 8\bin\`).
