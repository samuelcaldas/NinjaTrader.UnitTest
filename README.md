# NinjaTraderUnitTesting

NinjaTraderUnitTesting is a solution for running unit tests within NinjaTrader. It provides a testing framework similar to Python's `unittest` module and allows you to write and run unit tests for your NinjaScript code.

## Installation

To install NinjaTraderUnitTesting, follow these steps:

1. Download the source code from the GitHub repository.
2. Open the solution in Visual Studio and build the project.
3. Copy the built files to the NinjaTrader directory.

## Usage

To use NinjaTraderUnitTesting in your NinjaScript projects, follow these steps:

1. Add a reference to the NinjaTraderUnitTesting assembly in your NinjaScript project.
2. Create test cases by deriving from the `TestCase` class and implementing test methods.
3. Create an instance of the `TestSuite` class, add your test cases to the suite, and run the suite to execute all tests.

Here is an example of how you can write and run a unit test using NinjaTraderUnitTesting:

```csharp
using System;
using NinjaTraderUnitTesting;

public class MyTests : TestCase
{
    public void TestSomething()
    {
        int a = 1 + 1;
        int b = 2;
        AssertEqual(a, b);
    }
}

TestSuite suite = new TestSuite();
suite.Add(new MyTests("TestSomething"));
TestResult result = suite.Run();
```

This code creates a test case called `MyTests` that contains a test method called `TestSomething`. The test method uses the `AssertEqual` method from the `TestCase` base class to check if two variables are equal. It then creates an instance of the `TestSuite` class, adds the test case to the suite, and runs the suite to execute all tests.

## License

NinjaTraderUnitTesting is licensed under the MIT license.
