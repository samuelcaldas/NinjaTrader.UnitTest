using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Event arguments for simulating OnConnectionStatusUpdate.
    /// </summary>
    public class MockConnectionStatusEventArgs : EventArgs
    {
        public MockConnectionStatus Status { get; set; }
        public MockConnectionStatus PreviousStatus { get; set; }
        public MockConnection Connection { get; set; }
        public DateTime Time { get; set; }
        public string ErrorMessage { get; set; }

        public MockConnectionStatusEventArgs(
            MockConnectionStatus status,
            MockConnectionStatus previousStatus = MockConnectionStatus.Disconnected,
            MockConnection connection = null,
            string errorMessage = null,
            DateTime? time = null)
        {
            Status = status;
            PreviousStatus = previousStatus;
            Connection = connection;
            ErrorMessage = errorMessage;
            Time = time ?? DateTime.Now;
        }
    }
}
