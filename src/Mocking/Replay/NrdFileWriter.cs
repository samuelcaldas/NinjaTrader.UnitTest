using System;
using System.IO;
using System.Text;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Binary writer for creating NinjaTrader .nrd Market Replay data files.
    /// </summary>
    public class NrdFileWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryWriter _writer;
        private readonly bool _leaveOpen;
        private readonly NrdHeader _header;
        private bool _isFirstRecord = true;

        public NrdHeader Header => _header;

        public NrdFileWriter(string filePath, MockInstrument instrument = null)
            : this(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None), instrument, false)
        {
        }

        public NrdFileWriter(Stream stream, MockInstrument instrument = null, bool leaveOpen = false)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
            _writer = new BinaryWriter(_stream, Encoding.UTF8, true);

            _header = new NrdHeader
            {
                Symbol = instrument?.Name ?? "ES",
                TickSize = instrument?.TickSize ?? 0.25,
                PointValue = instrument?.PointValue ?? 50.0
            };

            // Write placeholder header
            _header.Serialize(_writer);
        }

        public void WriteEvent(MarketReplayEvent e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            DateTime utcTime = e.Time.ToUniversalTime();

            if (_isFirstRecord)
            {
                _header.TimeUniversalFirst = utcTime;
                _header.Open = e.Price;
                _header.High = e.Price;
                _header.Low = e.Price;
                _isFirstRecord = false;
            }

            _header.TimeUniversalLast = utcTime;
            _header.RecordCount++;

            if (e.RecordType == MarketReplayRecordType.MarketDepth && e.MarketDepth != null)
            {
                _writer.Write((byte)NrdRecordType.MarketDepth);
                _writer.Write(utcTime.Ticks);
                _writer.Write((byte)e.MarketDepth.MarketDataType);
                _writer.Write((byte)e.MarketDepth.Operation);
                _writer.Write(e.MarketDepth.Price);
                _writer.Write(e.MarketDepth.Volume);
                _writer.Write(e.MarketDepth.Position);
                _writer.Write(e.MarketDepth.MarketMaker ?? "");
            }
            else if (e.RecordType == MarketReplayRecordType.MinuteBar || e.RecordType == MarketReplayRecordType.DayBar)
            {
                var bar = e.Bar ?? new MockBar(e.Time, e.Price, e.Price, e.Price, e.Price, e.Volume);
                _writer.Write((byte)NrdRecordType.Bar);
                _writer.Write(utcTime.Ticks);
                _writer.Write(bar.Open);
                _writer.Write(bar.High);
                _writer.Write(bar.Low);
                _writer.Write(bar.Close);
                _writer.Write(bar.Volume);

                _header.Close = bar.Close;
                _header.TotalVolume += bar.Volume;
                if (bar.High > _header.High) _header.High = bar.High;
                if (bar.Low < _header.Low) _header.Low = bar.Low;
            }
            else
            {
                // Level 1 Market Data / Tick / Tick Replay
                _writer.Write((byte)NrdRecordType.MarketData);
                _writer.Write(utcTime.Ticks);
                _writer.Write((byte)(e.MarketData?.MarketDataType ?? MockMarketDataType.Last));
                _writer.Write(e.Price);
                _writer.Write(e.Volume);
                _writer.Write(e.BidPrice);
                _writer.Write(e.AskPrice);

                _header.Close = e.Price;
                _header.TotalVolume += e.Volume;
                if (e.Price > _header.High) _header.High = e.Price;
                if (e.Price < _header.Low) _header.Low = e.Price;
            }
        }

        public void Flush()
        {
            if (_stream.CanSeek)
            {
                long currentPos = _stream.Position;
                _stream.Seek(0, SeekOrigin.Begin);
                _header.Serialize(_writer);
                _writer.Flush();
                _stream.Seek(currentPos, SeekOrigin.Begin);
            }
        }

        public void Dispose()
        {
            Flush();
            _writer.Dispose();
            if (!_leaveOpen)
            {
                _stream.Dispose();
            }
        }
    }
}
