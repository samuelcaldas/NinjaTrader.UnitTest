using System;
using System.IO;

namespace NinjaTrader.UnitTest
{
    /// <summary>
    /// Test output logger that writes to any System.IO.TextWriter (StringWriter, StreamWriter, etc.).
    /// </summary>
    public class TextWriterOutput : ITestOutput
    {
        private readonly TextWriter _writer;

        public TextWriter Writer => _writer;

        public TextWriterOutput(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public void Write(string message)
        {
            _writer.Write(message);
        }

        public void WriteLine(string message, OutputLevel level = OutputLevel.Information)
        {
            _writer.WriteLine(message);
        }

        public void WriteError(string message, Exception ex = null)
        {
            _writer.WriteLine(message);
            if (ex != null)
            {
                _writer.WriteLine(ex.ToString());
            }
        }
    }
}
