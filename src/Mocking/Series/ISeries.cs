using System;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Represents a generic series data structure modeling NinjaTrader's ISeries interface.
    /// </summary>
    /// <typeparam name="T">The data type held in the series.</typeparam>
    public interface ISeries<T>
    {
        /// <summary>
        /// Gets or sets the value at the specified bars ago (0 is current bar).
        /// </summary>
        T this[int barsAgo] { get; set; }

        /// <summary>
        /// The total number of elements in the series.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Checks if a valid value has been explicitly set at the specified bars ago index.
        /// </summary>
        bool IsValidDataPoint(int barsAgo);

        /// <summary>
        /// Gets the underlying value at the specified absolute bar index (0 to Count - 1).
        /// </summary>
        T GetValueAt(int index);

        /// <summary>
        /// Resets the validity state of the data point at the specified bars ago index.
        /// </summary>
        void Reset(int barsAgo = 0);
    }
}
