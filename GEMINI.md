# NinjaTrader.UnitTest

## Project Overview
NinjaTrader.UnitTest is a custom add-on for NinjaTrader 8 that introduces a unit testing framework inspired by Python's `unittest`. It allows developers to write and execute unit tests for their NinjaScript strategies and indicators directly within the NinjaTrader environment.

## Technologies & Architecture
*   **Language:** C#
*   **Framework:** .NET Framework 4.8
*   **Dependencies:**
    *   `NinjaTrader.Core.dll`
    *   `NinjaTrader.Gui.dll`
    *   (Note: The project references these DLLs via relative paths expecting a standard NinjaTrader 8 installation).

## Key Components
*   **`TestCase.cs`:** The base class for all test cases. Users subclass this to define their own tests.
*   **`TestSuite.cs`:** A collection of `TestCase` objects that can be run together.
*   **`Assert.cs`:** Provides assertion methods (e.g., `AreEqual`) to verify test outcomes.
*   **`TextTestRunner.cs`:** A utility to run a suite of tests and log the results to the NinjaTrader output window.
*   **`TestResult.cs`:** Captures the results of a test run (passes, failures, errors).

## Building & Installation
1.  **Prerequisites:** NinjaTrader 8 must be installed.
2.  **Build:** Open `NinjaTrader.UnitTest.sln` in Visual Studio and build the solution.
3.  **Deployment:** The `.csproj` file contains a **Post-Build Event** that automatically copies the resulting `NinjaTrader.UnitTest.dll` and `.pdb` files to the user's NinjaTrader Custom bin folder (typically `Documents\NinjaTrader 8\bin\Custom`).

## Usage Pattern
To use this framework in a NinjaScript addon:
1.  Reference `NinjaTrader.UnitTest.dll`.
2.  Create a class inheriting from `NinjaTrader.UnitTest.TestCase`.
3.  Implement test methods using `Assert.*`.
4.  Instantiate a `TestSuite`, add your `TestCase` instances, and run them (often triggered by a NinjaTrader state change or connection event).

## Conventions
*   **Test Structure:** Follows the xUnit/JUnit pattern where tests are methods within a class.
*   **Output:** Results are primarily logged to the NinjaTrader output window via `NinjaScript.Log`.
