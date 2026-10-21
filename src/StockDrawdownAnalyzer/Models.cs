namespace StockDrawdownAnalyzer;

public record CsvFile(string Name, string Text);

public record Candle(DateTime Time, double Close);

public record StockSummary(
    string Symbol,
    int Candles,
    double PeakClose,
    DateTime PeakTime,
    double LastClose,
    DateTime LastTime,
    double CurrentDownPct,
    double MaxDrawdownPct,
    double BottomClose,
    DateTime BottomTime,
    double UpFromBottomPct,
    double MaxUpFromBottomPct);

public record FallLevel(int Percent, double Price, double UpPrice);
