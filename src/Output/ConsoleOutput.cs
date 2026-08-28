using System;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Test output logger that writes to System.Console with color highlights.
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
                Console.ForegroundColor = ResolveConsoleColor(level);
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

        private static ConsoleColor ResolveConsoleColor(OutputLevel level)
        {
            switch (level)
            {
                case OutputLevel.Verbose:
                    return ConsoleColor.DarkGray;
                case OutputLevel.Warning:
                    return ConsoleColor.Yellow;
                case OutputLevel.Error:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }
    }
}
