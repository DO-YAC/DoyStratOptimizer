using DoyStratOptimizer_Common.Models.Enums;

namespace DoyStratOptimizer_Common.Models;

public record TakenDecisionModel(TradeDecision TradeDecision, DateTime CandleTimeStamp)
{
    public bool IsProfitable { get; set; }
}
