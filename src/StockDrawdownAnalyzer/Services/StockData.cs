using System.Globalization;
using StockDrawdownAnalyzer.Client;

namespace StockDrawdownAnalyzer.Services;

public class StockData(IConfiguration config)
{
    private readonly string _folder = config["DataFolder"] ?? @"C:\data";

    public List<StockSummary> GetSummaries() =>
        Load().Select(x => Summarize(x.Name, x.Candles)).ToList();

    public StockDetail? GetDetail(string symbol)
    {
        var item = Load().FirstOrDefault(x => x.Name.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        return item.Name is null ? null : new StockDetail(Summarize(item.Name, item.Candles), item.Candles);
    }

    // Reads every CSV on each call so new/updated files are picked up without restarting.
    private List<(string Name, List<Candle> Candles)> Load()
    {
        if (!Directory.Exists(_folder)) return [];

        return Directory.GetFiles(_folder, "*.csv")
            .OrderBy(f => f)
            .Select(f => (Name: Path.GetFileNameWithoutExtension(f), Candles: ReadCsv(f)))
            .Where(x => x.Candles.Count > 0)
            .ToList();
    }

    // Expected columns: time (unix seconds), open, high, low, close, Volume
    private static List<Candle> ReadCsv(string path) =>
        File.ReadLines(path)
            .Skip(1)
            .Where(l => l.Trim().Length > 0)
            .Select(l => l.Split(','))
            .Select(p => new Candle(
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(p[0], CultureInfo.InvariantCulture)).ToLocalTime().DateTime,
                decimal.Parse(p[4], CultureInfo.InvariantCulture)))
            .OrderBy(c => c.Time)
            .ToList();

    private static StockSummary Summarize(string name, List<Candle> candles)
    {
        var last = candles[^1];
        var allTimePeak = candles.MaxBy(c => c.Close)!;

        // Max drawdown: largest peak-to-trough fall, using a running peak.
        var runningPeak = candles[0];
        var ddPeak = candles[0];
        var ddTrough = candles[0];
        decimal maxDown = 0;
        foreach (var c in candles)
        {
            if (c.Close > runningPeak.Close) runningPeak = c;
            var down = (runningPeak.Close - c.Close) / runningPeak.Close * 100;
            if (down > maxDown)
            {
                maxDown = down;
                ddPeak = runningPeak;
                ddTrough = c;
            }
        }

        return new StockSummary(
            name,
            candles[0].Time,
            last.Time,
            last.Close,
            allTimePeak.Close,
            allTimePeak.Time,
            Math.Round((allTimePeak.Close - last.Close) / allTimePeak.Close * 100, 2),
            Math.Round(maxDown, 2),
            ddPeak.Time,
            ddTrough.Time);
    }
}
