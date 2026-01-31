namespace DoyStratOptimizer_Common.Models;

public record LineModel(
    DateTime CandleTimestamp,
    decimal Open,
    decimal Low,
    decimal High,
    decimal Close,
    decimal Volume,
    decimal Rsi);
