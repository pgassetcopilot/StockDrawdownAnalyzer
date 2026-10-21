namespace StockDrawdownAnalyzer.Client;

public record Candle(DateTime Time, decimal Close);

public record StockSummary(
    string Symbol,
    DateTime From,
    DateTime To,
    decimal LastClose,
    decimal PeakClose,
    DateTime PeakDate,
    decimal DownFromPeakPct,
    decimal MaxDrawdownPct,
    DateTime MaxDrawdownPeakDate,
    DateTime MaxDrawdownTroughDate);

public record StockDetail(StockSummary Summary, List<Candle> Candles);
