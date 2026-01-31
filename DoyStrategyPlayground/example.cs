using System;

public class Strategy
{
    private readonly decimal mRsiMin;
    private readonly decimal mRsiMax;

    // Important, that constructor is public, and that theres only one constructor.
    public Strategy(decimal rsiMin, decimal rsiMax)
    {
        mRsiMin = rsiMin;
        mRsiMax = rsiMax;
    }

    // Importnat, that method is called Execute and is public
    public TradeDecision Execute(JObject jLine)
    {
        var line = jLine.ToObject<TradeDecision>();

        return line.Rsi switch
        {
            <30 => TradeDecision.BUY,
            >70 => TradeDecision.SELL,
            : => TradeDecision.NONE
        }
    }
}

internal enum TradeDecision
{
    SELL = -1,
    NONE = 0,
    BUY = 1
}

internal record LineModel(
    DateTime CandleTimestamp,
    decimal Open,
    decimal Low,
    decimal High,
    decimal Close,
    decimal Volume,
    decimal Rsi /*only for demonstration*/);