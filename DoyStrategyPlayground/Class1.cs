using Newtonsoft.Json.Linq;

namespace DoyStrategyPlayground;

public class Strategy
{
    private readonly decimal mRsiSellLimit;
    private readonly decimal mRsiBuyLimit;
    public Strategy(decimal rsiBuyLimit, decimal rsiSellLimit)
    {
        mRsiBuyLimit = rsiBuyLimit;
        mRsiSellLimit = rsiSellLimit;
    }

    // Important, that method is called Execute and is public
    public TradeDecision Execute(JObject jLine)
    {
        var line = jLine.ToObject<LineModel>();

        if (line == null)
        {
            return TradeDecision.NONE;
        }

        if (line.Rsi < mRsiBuyLimit)
        {
            return TradeDecision.BUY;
        }
        else if (line.Rsi > mRsiSellLimit)
        {
            return TradeDecision.SELL;
        }

        return TradeDecision.NONE;
    }
}

public enum TradeDecision
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
