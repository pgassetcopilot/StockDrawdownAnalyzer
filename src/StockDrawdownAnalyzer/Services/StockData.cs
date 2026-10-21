using System.Globalization;

namespace StockDrawdownAnalyzer.Services;

public static class StockData
{
    // Expects a header with "time" (Unix seconds) and "close" columns.
    public static List<Candle> ParseCsv(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length < 2) return [];

        var header = lines[0].Split(',').Select(h => h.Trim().ToLowerInvariant()).ToArray();
        int timeIdx = Array.IndexOf(header, "time");
        int closeIdx = Array.IndexOf(header, "close");
        if (timeIdx < 0 || closeIdx < 0) return [];

        var candles = new List<Candle>();
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length <= Math.Max(timeIdx, closeIdx)) continue;
            if (!long.TryParse(parts[timeIdx], NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix)) continue;
            if (!double.TryParse(parts[closeIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var close)) continue;
            candles.Add(new Candle(DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime, close));
        }

        return candles.OrderBy(c => c.Time).ToList();
    }

    public static StockSummary Summarize(string symbol, List<Candle> candles)
    {
        // Peak/bottom = highest/lowest close in the whole data set.
        // Running peak/bottom are used for max drawdown and max rise from bottom.
        var peak = candles.MaxBy(c => c.Close)!;
        var bottom = candles.MinBy(c => c.Close)!;
        var last = candles[^1];

        double runningPeak = candles[0].Close;
        double runningBottom = candles[0].Close;
        double maxDd = 0;
        double maxUp = 0;
        foreach (var c in candles)
        {
            runningPeak = Math.Max(runningPeak, c.Close);
            runningBottom = Math.Min(runningBottom, c.Close);
            maxDd = Math.Max(maxDd, Drop(runningPeak, c.Close));
            maxUp = Math.Max(maxUp, Rise(runningBottom, c.Close));
        }

        return new StockSummary(
            symbol, candles.Count, peak.Close, peak.Time, last.Close, last.Time,
            Drop(peak.Close, last.Close), maxDd,
            bottom.Close, bottom.Time, Rise(bottom.Close, last.Close), maxUp);
    }

    // 1..5, 10, then steps of 5 up to 100.
    public static readonly int[] FallPercents = [1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100];

    // Price = peak fallen by p%. UpPrice = bottom risen by p%.
    public static List<FallLevel> FallLevels(double peak, double bottom) =>
        FallPercents.Select(p => new FallLevel(p, peak * (1 - p / 100.0), bottom * (1 + p / 100.0))).ToList();

    // Percent fall from basePrice to price (positive = down, negative = up).
    public static double Drop(double basePrice, double price) =>
        basePrice <= 0 ? 0 : (basePrice - price) / basePrice * 100;

    // Percent rise from basePrice to price (positive = up).
    public static double Rise(double basePrice, double price) =>
        basePrice <= 0 ? 0 : (price - basePrice) / basePrice * 100;
}
