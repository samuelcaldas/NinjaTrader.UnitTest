# NinjaTrader UnitTest

NinjaTrader UnitTest is an add-on for NinjaTrader 8 that provides a unit testing framework similar to Python's `unittest`, but written in C# and adapted for NinjaTrader.

## Installation

To install NinjaTrader.UnitTest, follow these steps:
1. Download the source code from the GitHub repository.
2. Build the `NinjaTrader.UnitTest.sln` solution using Visual Studio to generate the `NinjaTrader.UnitTest.dll` file.
3. Copy the `NinjaTrader.UnitTest.dll` file to the `bin/Custom` folder of your NinjaTrader 8 installation.
4. In the NinjaScript Editor, right-click on the `References` folder and select `Add Reference...`.
5. In the `Add Reference` window, browse to the `NinjaTrader.UnitTest.dll` file and select it.

## Usage

To use NinjaTrader.UnitTest in your NinjaScript projects, follow these steps:
To use the NinjaTrader UnitTest add-on in your own NinjaScript project, follow these steps:
1. Add a reference to the `NinjaTrader.UnitTest` namespace.
2. Create test cases by subclassing the `NinjaTrader.UnitTest.TestCase` class.

Here is an example of how to create a simple test case using the NinjaTrader UnitTest add-on:

```csharp
using System;
using NinjaTrader.UnitTest;

namespace NinjaTrader.NinjaScript.AddOns
{
    public class MyTestCase : TestCase
    {
        public Tests(string name) : base(name)
        { }

        public void TestAddition()
        {
            int result = 2 + 2;
            int expected = 4;
            Assert.AreEqual(expected, result);
        }
    }
}
```

This code defines a new test case called `MyTestCase` that contains a single test method called `TestAddition`. This test method uses the `Assert.AreEqual` method from the `NinjaTrader.UnitTest.Assert` class to verify that the result of adding 2 and 2 is equal to 4.

To run this test case, you can create an instance of the `MyTestCase` class and call its `Run` method, like this:

```csharp
MyTestCase myTestCase = new MyTestCase();
myTestCase.Run();
```


Alternatively, you can create a `TestSuite` object and add the `MyTestCase` object to it, like this:

```csharp
namespace NinjaTrader.NinjaScript.AddOns
{
	public class TestAddon : NinjaTrader.NinjaScript.AddOnBase
	{
		protected override void OnStateChange()
		{
            if (State == State.SetDefaults)
            {
                Name = "TestAddon";
                Description = "An add-on that runs UnitTests on Ninjatrader";
            }
            else if (State == State.Configure)
            {
			
                // Subscribe to connection updates
                Connection.ConnectionStatusUpdate += OnConnectionStatusUpdate;
            }
            else if (State == State.Terminated)
            {
                // Unsubscribe from connection updates
                Connection.ConnectionStatusUpdate -= OnConnectionStatusUpdate;
            }
		}
		
		// This method is fired on connection status update events
        protected void OnConnectionStatusUpdate(object sender, ConnectionStatusEventArgs connectionStatusUpdate)
        {
            if (connectionStatusUpdate.Status == ConnectionStatus.Connected)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log("Connected for orders at " + DateTime.Now, LogLevel.Information);

                // Run tests
                TestSuite suite = new TestSuite();
                suite.Add(new MyTestCase("TestAddition"));
                TestResult result = suite.Run();

                result.PrintSummary();
   
            }

            else if (connectionStatusUpdate.Status == ConnectionStatus.ConnectionLost)
            {
                NinjaTrader.NinjaScript.NinjaScript.Log("Connection for orders lost at: " + DateTime.Now, LogLevel.Information);
            }
        }
	}
}
```

This will execute the `TestAddition` method and log the results using the `NinjaScript.Log` method.

## Contributing

We welcome contributions! If you have ideas for improving this tool, please feel free to submit a pull request or open an issue.

## License

This project is licensed under the MIT license. See the [LICENSE.txt](LICENSE.txt) file for more information.
