using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Test output logger that writes directly to the NinjaTrader Output window via NinjaScript.Log.
    /// </summary>
    public class NinjaTraderOutput : ITestOutput
    {
        public void Write(string message)
        {
            NinjaTrader.NinjaScript.NinjaScript.Log(message, LogLevel.Information);
        }

        public void WriteLine(string message, OutputLevel level = OutputLevel.Information)
        {
            LogLevel ntLevel = ResolveLogLevel(level);
            NinjaTrader.NinjaScript.NinjaScript.Log(message, ntLevel);
        }

        public void WriteError(string message, Exception ex = null)
        {
            string fullMessage = ex != null ? $"{message}\n{ex}" : message;
            NinjaTrader.NinjaScript.NinjaScript.Log(fullMessage, LogLevel.Error);
        }

        private static LogLevel ResolveLogLevel(OutputLevel level)
        {
            switch (level)
            {
                case OutputLevel.Warning:
                    return LogLevel.Warning;
                case OutputLevel.Error:
                    return LogLevel.Error;
                default:
                    return LogLevel.Information;
            }
        }
    }
}
