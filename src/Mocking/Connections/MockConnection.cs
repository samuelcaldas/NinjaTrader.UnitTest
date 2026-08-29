using System;
using System.Collections.Generic;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Mock trading connection representing a broker or data feed adapter.
    /// </summary>
    public class MockConnection
    {
        public string Name { get; set; }
        public MockConnectionStatus Status { get; private set; } = MockConnectionStatus.Disconnected;
        public List<MockAccount> Accounts { get; } = new List<MockAccount>();

        public event EventHandler<MockConnectionStatusEventArgs> ConnectionStatusChanged;

        public MockConnection(string name = "Simulation", params MockAccount[] accounts)
        {
            Name = name;
            if (accounts != null)
            {
                Accounts.AddRange(accounts);
            }
        }

        public void Connect()
        {
            SetStatus(MockConnectionStatus.Connected);
        }

        public void Disconnect()
        {
            SetStatus(MockConnectionStatus.Disconnected);
        }

        public void SimulateConnectionLoss(string reason = "Simulated network failure")
        {
            SetStatus(MockConnectionStatus.ConnectionLost, reason);
        }

        public void SetStatus(MockConnectionStatus newStatus, string errorMessage = null)
        {
            var prevStatus = Status;
            Status = newStatus;
            ConnectionStatusChanged?.Invoke(this, new MockConnectionStatusEventArgs(newStatus, prevStatus, this, errorMessage));
        }
    }
}
