using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NinjaTrader.UnitTest.Mocking
{
    /// <summary>
    /// Reads and parses NinjaTrader historical and market replay data files (.txt, .csv).
    /// </summary>
    public static class MarketReplayReader
    {
        private static readonly string[] DateTimeFormats = new[]
        {
            "yyyyMMdd HHmmss fffffff",
            "yyyyMMdd HHmmss fff",
            "yyyyMMdd HHmmss",
            "yyyyMMdd",
            "yyyy-MM-dd HH:mm:ss.fffffff",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd"
        };

        public static List<MarketReplayEvent> ReadFromFile(string filePath, MockInstrument instrument = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Market replay file not found: {filePath}", filePath);

            if (filePath.EndsWith(".nrd", StringComparison.OrdinalIgnoreCase))
            {
                return ReadNrdFile(filePath);
            }

            string content = File.ReadAllText(filePath);
            return ReadFromString(content, instrument);
        }

        public static List<MarketReplayEvent> ReadNrdFile(string filePath, DateTime? from = null, DateTime? to = null)
        {
            using (var reader = new NrdFileReader(filePath))
            {
                return new List<MarketReplayEvent>(reader.ReadEvents(from, to));
            }
        }

        public static List<MarketReplayEvent> ReadNrdStream(Stream stream, DateTime? from = null, DateTime? to = null, bool leaveOpen = false)
        {
            using (var reader = new NrdFileReader(stream, leaveOpen))
            {
                return new List<MarketReplayEvent>(reader.ReadEvents(from, to));
            }
        }

        public static List<MarketReplayEvent> ReadFromString(string content, MockInstrument instrument = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<MarketReplayEvent>();

            var events = new List<MarketReplayEvent>();
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
                        continue;

                    var replayEvent = ParseLine(line, instrument);
                    if (replayEvent != null)
                    {
                        events.Add(replayEvent);
                    }
                }
            }

            return events;
        }

        public static MockBarSeries ParseBarsFromText(string content, string instrumentName = "ES", MockBarsPeriodType periodType = MockBarsPeriodType.Minute, int periodValue = 1)
        {
            var series = new MockBarSeries(instrumentName, periodType, periodValue);
            if (string.IsNullOrWhiteSpace(content))
                return series;

            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
                        continue;

                    var parts = line.Split(';', ',');
                    if (parts.Length >= 5)
                    {
                        DateTime time = ParseTimestamp(parts[0]);
                        double open = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        double high = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        double low = double.Parse(parts[3], CultureInfo.InvariantCulture);
                        double close = double.Parse(parts[4], CultureInfo.InvariantCulture);
                        long volume = parts.Length >= 6 ? long.Parse(parts[5], CultureInfo.InvariantCulture) : 0;

                        series.Add(time, open, high, low, close, volume);
                    }
                }
            }

            return series;
        }

        public static List<MarketReplayEvent> ParseDepthFromText(string content, MockInstrument instrument = null)
        {
            var events = new List<MarketReplayEvent>();
            if (string.IsNullOrWhiteSpace(content))
                return events;

            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
                        continue;

                    var parts = line.Split(';', ',');
                    if (parts.Length >= 5)
                    {
                        DateTime time = ParseTimestamp(parts[0]);
                        var type = (MockMarketDataType)Enum.Parse(typeof(MockMarketDataType), parts[1], true);
                        var op = (MockMarketDepthOperation)Enum.Parse(typeof(MockMarketDepthOperation), parts[2], true);
                        double price = double.Parse(parts[3], CultureInfo.InvariantCulture);
                        long volume = long.Parse(parts[4], CultureInfo.InvariantCulture);
                        int pos = parts.Length >= 6 ? int.Parse(parts[5], CultureInfo.InvariantCulture) : 0;
                        string mm = parts.Length >= 7 ? parts[6] : "";

                        events.Add(MarketReplayEvent.CreateDepth(time, type, op, price, volume, pos, mm, instrument));
                    }
                }
            }

            return events;
        }

        private static MarketReplayEvent ParseLine(string line, MockInstrument instrument)
        {
            var parts = line.Split(';', ',');
            if (parts.Length < 2)
                return null;

            DateTime time = ParseTimestamp(parts[0]);

            // Level 2 Depth format: timestamp;type;operation;price;volume;...
            if (parts.Length >= 5 && Enum.TryParse<MockMarketDataType>(parts[1], true, out var depthType) && Enum.TryParse<MockMarketDepthOperation>(parts[2], true, out var depthOp))
            {
                double price = double.Parse(parts[3], CultureInfo.InvariantCulture);
                long volume = long.Parse(parts[4], CultureInfo.InvariantCulture);
                int pos = parts.Length >= 6 ? int.Parse(parts[5], CultureInfo.InvariantCulture) : 0;
                string mm = parts.Length >= 7 ? parts[6] : "";
                return MarketReplayEvent.CreateDepth(time, depthType, depthOp, price, volume, pos, mm, instrument);
            }

            // Tick Replay format: timestamp;last;bid;ask;volume (5 parts)
            if (parts.Length == 5 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                double last = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double bid = double.Parse(parts[2], CultureInfo.InvariantCulture);
                double ask = double.Parse(parts[3], CultureInfo.InvariantCulture);
                long volume = long.Parse(parts[4], CultureInfo.InvariantCulture);
                return MarketReplayEvent.CreateTickReplay(time, last, bid, ask, volume, instrument);
            }

            // Standard Tick format: timestamp;price;volume (3 parts)
            if (parts.Length == 3)
            {
                double price = double.Parse(parts[1], CultureInfo.InvariantCulture);
                long volume = long.Parse(parts[2], CultureInfo.InvariantCulture);
                return MarketReplayEvent.CreateTick(time, price, volume, instrument);
            }

            // Bar format: timestamp;open;high;low;close;volume (6 parts)
            if (parts.Length >= 6)
            {
                double open = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double high = double.Parse(parts[2], CultureInfo.InvariantCulture);
                double low = double.Parse(parts[3], CultureInfo.InvariantCulture);
                double close = double.Parse(parts[4], CultureInfo.InvariantCulture);
                long volume = long.Parse(parts[5], CultureInfo.InvariantCulture);
                var bar = new MockBar(time, open, high, low, close, volume);
                return MarketReplayEvent.CreateBar(bar, parts[0].Length <= 8 ? MarketReplayRecordType.DayBar : MarketReplayRecordType.MinuteBar);
            }

            return null;
        }

        private static DateTime ParseTimestamp(string raw)
        {
            raw = raw.Trim();
            if (DateTime.TryParseExact(raw, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                return dt;
            }

            throw presidentialFormatError(raw);
        }

        private static FormatException presidentialFormatError(string raw)
        {
            return new FormatException($"Unrecognized NinjaTrader timestamp format: '{raw}'");
        }
    }
}
