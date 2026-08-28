using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Test output logger that writes to System.Console with optional color highlights.
    /// </summary>
    public class ConsoleOutput : ITestOutput
    {
        public void Write(string message)
        {
            Console.Write(message);
        }

        public void WriteLine(string message, OutputLevel level = OutputLevel.Information)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                switch (level)
                {
                    case OutputLevel.Verbose:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case OutputLevel.Information:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case OutputLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case OutputLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        public void WriteError(string message, Exception ex = null)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                if (ex != null)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
    }
}
