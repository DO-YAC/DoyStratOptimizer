using DoyStratOptimizer_Common.Models;

namespace DoyStratOptimizer_Common.Extensions;

public static class OpenPositionModelExtensions
{
    public static bool IsInProfit(this OpenPositionModel openPosition)
    {
        return openPosition.PnL > 0;
    }
}
