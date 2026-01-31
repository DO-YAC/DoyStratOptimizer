namespace DoyStratOptimizer_Common.Models.Responses;

/// <summary>
/// Represents the results of an evaluation, including profit and loss metrics, drawdown statistics, and an overall
/// score.
/// </summary>
/// <param name="PnL">The total profit or loss value calculated during the evaluation.</param>
/// <param name="MaxProfit">The maximum profit achieved at any point during the evaluation period.</param>
/// <param name="MaxDrawdown">The largest observed loss from a peak to a trough during the evaluation period.</param>
/// <param name="TimeInProfit">The total time, in candles, that the evaluation remained in a profitable state.</param>
/// <param name="TimeInDrawdown">The total time, in candles, that the evaluation experienced a drawdown.</param>
/// <param name="Score">The overall score assigned to the evaluation, representing performance or quality.</param>
public record EvaluationResponse
{
    public decimal PnL { get; set; } = 0;
    public decimal MaxProfit { get; set; } = 0;
    public decimal MaxDrawdown { get; set; } = 0;
    public int TimeInProfit { get; set; } = 0;
    public int TimeInDrawdown { get; set; } = 0;
    public decimal Score { get; set; } = 0;
    public List<TakenDecisionModel> TakenDecisions { get; set; } = [];
}
