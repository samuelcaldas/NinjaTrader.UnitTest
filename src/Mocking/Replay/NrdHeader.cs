using System;
using System.IO;
using System.Text;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Binary file header for NinjaTrader .nrd Market Replay data files.
    /// </summary>
    public class NrdHeader
    {
        public const uint NrdMagic = 0x4E524431; // 'NRD1'
        public const int CurrentVersion = 1;

        public uint Magic { get; set; } = NrdMagic;
        public int Version { get; set; } = CurrentVersion;
        public string Symbol { get; set; } = "ES";
        public double TickSize { get; set; } = 0.25;
        public double PointValue { get; set; } = 50.0;
        public DateTime TimeUniversalFirst { get; set; } = DateTime.MinValue;
        public DateTime TimeUniversalLast { get; set; } = DateTime.MaxValue;
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long TotalVolume { get; set; }
        public int RecordCount { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(Magic);
            writer.Write(Version);

            byte[] symbolBytes = Encoding.UTF8.GetBytes(Symbol ?? "");
            writer.Write(symbolBytes.Length);
            writer.Write(symbolBytes);

            writer.Write(TickSize);
            writer.Write(PointValue);
            writer.Write(TimeUniversalFirst.ToUniversalTime().Ticks);
            writer.Write(TimeUniversalLast.ToUniversalTime().Ticks);
            writer.Write(Open);
            writer.Write(High);
            writer.Write(Low);
            writer.Write(Close);
            writer.Write(TotalVolume);
            writer.Write(RecordCount);
        }

        public static NrdHeader Deserialize(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            uint magic = reader.ReadUInt32();
            if (magic != NrdMagic)
                throw new InvalidDataException($"Invalid NRD magic header: 0x{magic:X8}");

            int version = reader.ReadInt32();

            int symbolLen = reader.ReadInt32();
            byte[] symbolBytes = reader.ReadBytes(symbolLen);
            string symbol = Encoding.UTF8.GetString(symbolBytes);

            double tickSize = reader.ReadDouble();
            double pointValue = reader.ReadDouble();
            DateTime firstTime = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
            DateTime lastTime = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
            double open = reader.ReadDouble();
            double high = reader.ReadDouble();
            double low = reader.ReadDouble();
            double close = reader.ReadDouble();
            long volume = reader.ReadInt64();
            int count = reader.ReadInt32();

            return new NrdHeader
            {
                Magic = magic,
                Version = version,
                Symbol = symbol,
                TickSize = tickSize,
                PointValue = pointValue,
                TimeUniversalFirst = firstTime,
                TimeUniversalLast = lastTime,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                TotalVolume = volume,
                RecordCount = count
            };
        }
    }
}
