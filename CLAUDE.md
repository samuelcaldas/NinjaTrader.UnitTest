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

## Architecture & Codebase Structure

The codebase is organized according to Domain-Driven Design (DDD), SOLID, and Object Calisthenics principles:

```
src/
├── Assertions/       # IAssert interface and Assert utility
├── Attributes/       # [Test], [Skip], [SkipIf], [SkipUnless], [ExpectedFailure], [SetUpClass]
├── Discovery/        # TestLoader (reflection & auto-discovery)
├── Exceptions/       # AssertionException, SkipTestException, UnexpectedSuccessException
├── Execution/        # TestCase lifecycle, TestSuite runner, SubTest context
├── Mocking/          # Mock objects for NT8
│   ├── Accounts/     # MockAccount, MockPosition
│   ├── Bars/         # BarSeriesBuilder, MockBar, MockBarSeries
│   ├── Harness/      # MockState, NinjaScriptTestHarness
│   ├── Instruments/  # MockInstrument, MockInstrumentType
│   └── Orders/       # MockOrder, MockOrderAction, MockOrderState, MockOrderType
├── Output/           # ITestOutput, NinjaTraderOutput, ConsoleOutput, TextWriterOutput
└── Results/          # TestResult, TextTestResult, TextTestRunner

tests/
├── Assertions/       # BasicAssertionTests, CollectionAssertionTests, NumericAndRegexAssertionTests
├── Discovery/        # TestLoaderTests
├── Execution/        # LifecycleAndFixtureTests, SkipAndExpectedFailureTests, SubTestTests
├── Mocking/          # MockAccountAndOrderTests, MockBarsTests, MockInstrumentTests, NinjaScriptTestHarnessTests
└── Results/          # TestResultClassificationTests
```

## Dependencies
- **Target Framework:** .NET Framework 4.8 (`v4.8`), C# 7.3 (`x64`).
- **Binary Dependencies:** `NinjaTrader.Core.dll` and `NinjaTrader.Gui.dll` (located in `lib/` and resolved from `C:\Program Files\NinjaTrader 8\bin\`).
