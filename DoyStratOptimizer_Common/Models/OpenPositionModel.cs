using DoyStratOptimizer_Common.Models.Enums;

namespace DoyStratOptimizer_Common.Models
{
    public class OpenPositionModel
    {
        public decimal PnL { get; set; }
        public decimal EntryPrice { get; set; }
        public TradeDecision TradeDecision { get; set; }
    }
}
