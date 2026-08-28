# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
`NinjaTrader.UnitTest` is a lightweight unit testing framework for NinjaTrader 8, modeled after Python's `unittest` standard library (`TestCase`, `TestSuite`, `TextTestRunner`, `Assert`). It is built as a .NET Framework 4.8 class library targeting NinjaTrader 8 and logs directly to the NinjaTrader output window (`NinjaTrader.NinjaScript.NinjaScript.Log`).

## Build & Deployment Commands

Built using Visual Studio or MSBuild (.NET Framework 4.8):

```powershell
# Build AnyCPU (Debug/Release)
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Debug /p:Platform="AnyCPU"
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="AnyCPU"

# Build x64 (Recommended for NinjaTrader 8)
msbuild NinjaTrader.UnitTest.sln /p:Configuration=Release /p:Platform="x64"
```

*Note on Deployment:* `NinjaTrader.UnitTest.csproj` has a `PostBuildEvent` that automatically copies `NinjaTrader.UnitTest.dll` and `.pdb` to the local user's `%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom\` folder.

## Architecture & Core Components

All types reside in the `NinjaTrader.UnitTest` namespace:

- **`TestCase` (`TestCase.cs`):** Base test class subclassing `Assert`.
  - Executes test methods via reflection using the `methodName` string passed to the constructor (`base(methodName)`).
  - Lifecycle hooks: `SetUp()`, `TearDown()`, `SetUpClass()`, `TearDownClass()`.
  - Supports `SkipTest(reason)` and `SubTest(...)`.
  - *Note:* No attribute discovery (`[Test]`). Test methods must be explicitly targeted by name.
- **`Assert` (`Assert.cs`):** Assertion utility.
  - Uses Python `unittest` naming: `AssertEqual`, `AssertNotEqual`, `AssertTrue`, `AssertFalse`, `AssertIs`, `AssertIsNot`, `AssertIsNone`, `AssertIsNotNone`, `AssertIn`, `AssertNotIn`, `AssertIsInstance`, `AssertNotIsInstance`.
  - Throws `Exception` on assertion failure.
- **`TestSuite` (`TestSuite.cs`):** Aggregates multiple `TestCase` instances and executes them sequentially with a shared `TestResult`.
- **`TestResult` (`TestResult.cs`):** Tracks successes, failures, errors, execution duration, and logs output via `NinjaTrader.NinjaScript.NinjaScript.Log`.
- **`TextTestRunner` (`TextTestRunner.cs`):** Static runner entry point (`TextTestRunner.Run(suite)`) executing a `TestSuite` via `BasicTestRunner`.

## Usage & Authoring Pattern

```csharp
using NinjaTrader.UnitTest;

public class MyMathTests : TestCase
{
    // Required: forward method name to base
    public MyMathTests(string methodName) : base(methodName) { }

    public void TestAddition()
    {
        int result = 2 + 2;
        AssertEqual(4, result, "Addition failed");
    }
}

// Running inside a NinjaTrader AddOn / Script:
var suite = new TestSuite();
suite.Add(new MyMathTests("TestAddition"));
TextTestRunner.Run(suite);
```

## Dependencies
- **Target Framework:** .NET Framework 4.8 (`v4.8`), C# 7.3 (`x64`).
- **Binary Dependencies:** `NinjaTrader.Core.dll` and `NinjaTrader.Gui.dll` (located at `C:\Program Files\NinjaTrader 8\bin\`).
