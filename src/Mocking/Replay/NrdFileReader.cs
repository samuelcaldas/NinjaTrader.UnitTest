using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Binary reader for NinjaTrader .nrd Market Replay data files.
    /// </summary>
    public class NrdFileReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryReader _reader;
        private readonly bool _leaveOpen;
        private readonly NrdHeader _header;
        private readonly MockInstrument _instrument;

        public NrdHeader Header => _header;
        public MockInstrument Instrument => _instrument;

        public NrdFileReader(string filePath)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), false)
        {
        }

        public NrdFileReader(Stream stream, bool leaveOpen = false)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
            _reader = new BinaryReader(_stream, Encoding.UTF8, true);

            _header = NrdHeader.Deserialize(_reader);
            _instrument = new MockInstrument(_header.Symbol, _header.TickSize, _header.PointValue);
        }

        public IEnumerable<MarketReplayEvent> ReadEvents(DateTime? from = null, DateTime? to = null)
        {
            while (_stream.Position < _stream.Length)
            {
                byte recordTag = _reader.ReadByte();
                var recordType = (NrdRecordType)recordTag;

                long ticks = _reader.ReadInt64();
                DateTime time = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();

                MarketReplayEvent replayEvent = null;

                switch (recordType)
                {
                    case NrdRecordType.MarketData:
                        var mdType = (MockMarketDataType)_reader.ReadByte();
                        double price = _reader.ReadDouble();
                        long volume = _reader.ReadInt64();
                        double bid = _reader.ReadDouble();
                        double ask = _reader.ReadDouble();

                        if (bid > 0 || ask > 0)
                        {
                            replayEvent = MarketReplayEvent.CreateTickReplay(time, price, bid, ask, volume, _instrument);
                        }
                        else
                        {
                            replayEvent = MarketReplayEvent.CreateTick(time, price, volume, _instrument);
                        }
                        break;

                    case NrdRecordType.MarketDepth:
                        var depthType = (MockMarketDataType)_reader.ReadByte();
                        var op = (MockMarketDepthOperation)_reader.ReadByte();
                        double depthPrice = _reader.ReadDouble();
                        long depthVol = _reader.ReadInt64();
                        int pos = _reader.ReadInt32();
                        string mm = _reader.ReadString();

                        replayEvent = MarketReplayEvent.CreateDepth(time, depthType, op, depthPrice, depthVol, pos, mm, _instrument);
                        break;

                    case NrdRecordType.Bar:
                        double o = _reader.ReadDouble();
                        double h = _reader.ReadDouble();
                        double l = _reader.ReadDouble();
                        double c = _reader.ReadDouble();
                        long v = _reader.ReadInt64();

                        var bar = new MockBar(time, o, h, l, c, v);
                        replayEvent = MarketReplayEvent.CreateBar(bar);
                        break;
                }

                if (replayEvent != null)
                {
                    if (from.HasValue && time < from.Value)
                        continue;
                    if (to.HasValue && time > to.Value)
                        break;

                    yield return replayEvent;
                }
            }
        }

        public List<MarketReplayEvent> ReadAllEvents()
        {
            return new List<MarketReplayEvent>(ReadEvents());
        }

        public void Dispose()
        {
            _reader.Dispose();
            if (!_leaveOpen)
            {
                _stream.Dispose();
            }
        }
    }
}
