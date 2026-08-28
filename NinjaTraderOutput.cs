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
            // NinjaTrader.NinjaScript.NinjaScript.Log is line-based
            NinjaTrader.NinjaScript.NinjaScript.Log(message, LogLevel.Information);
        }

        public void WriteLine(string message, OutputLevel level = OutputLevel.Information)
        {
            LogLevel ntLevel;
            switch (level)
            {
                case OutputLevel.Verbose:
                case OutputLevel.Information:
                    ntLevel = LogLevel.Information;
                    break;
                case OutputLevel.Warning:
                    ntLevel = LogLevel.Warning;
                    break;
                case OutputLevel.Error:
                    ntLevel = LogLevel.Error;
                    break;
                default:
                    ntLevel = LogLevel.Information;
                    break;
            }

            NinjaTrader.NinjaScript.NinjaScript.Log(message, ntLevel);
        }

        public void WriteError(string message, Exception ex = null)
        {
            string fullMessage = ex != null ? $"{message}\n{ex}" : message;
            NinjaTrader.NinjaScript.NinjaScript.Log(fullMessage, LogLevel.Error);
        }
    }
}
