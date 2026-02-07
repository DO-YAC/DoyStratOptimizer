using DoyStratOptimizer_Common.Extensions;
using DoyStratOptimizer_Common.Models;
using DoyStratOptimizer_Common.Models.Enums;
using DoyStratOptimizer_Common.Models.Exceptions;
using DoyStratOptimizer_Common.Models.Responses;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace DoyStratOptimizer_Tester;

public class EvaluationCalculator
{
    private readonly object mStrategyInstance;
    private readonly LineModel[] mHistoricalData;
    private readonly MethodInfo mExecuteStrategyMethodInfo;
    // Took spread from real exchange data
    private readonly decimal mSpread = 0.00014m;
    private readonly int mLotSize = 5;
    private bool mIsInPosition = false;
    private OpenPositionModel? mOpenPosition = null!;
    private const decimal INITIAL_EQUITY = 100000;
    private decimal mEquity = INITIAL_EQUITY;
    private readonly EvaluationResponse mEvaluationResult = new EvaluationResponse();

    public EvaluationCalculator (object strategyInstance, LineModel[] historicalData) 
    {
        mStrategyInstance = strategyInstance;
        mHistoricalData = historicalData;
        mExecuteStrategyMethodInfo = GetMethodInfo(strategyInstance);
    }

    public async Task<EvaluationResponse> EvaluateStrategy()
    {
        // Go through each candle in the historical data
        foreach (var line in mHistoricalData)
        {
            var jLine = ConvertLineToJObject(line);

            var rawDecision = mExecuteStrategyMethodInfo.Invoke(mStrategyInstance, [jLine]);

            if (!Enum.TryParse<TradeDecision>(rawDecision!.ToString(), out TradeDecision decision))
            {
                throw new DoyStratOptimizerException(
                    "Strategy Execute method did not return a valid StrategyDecision.",
                    System.Net.HttpStatusCode.BadRequest);
            }

            // Process the decision (BUY, SELL, NONE)

            if (!mIsInPosition && decision != TradeDecision.NONE)
            {
                HandleIsNotInPosition(decision, line);
            }
            else if (mIsInPosition)
            {
                HandleIsInPosition(decision, line);
            }
        }

        // TODO: Score
        CalculateScore(mEvaluationResult);
        return mEvaluationResult;
    }

    private void CalculateScore(EvaluationResponse evaluationResponse)
    {
        const decimal maxScore = 1000;
        const decimal profitNormalizationBase = 500000;
        const decimal optimalCandlesPerProfitableTrade = 1800;
        
        const decimal timeDrawdownWeight = 0.30m;
        const decimal maxDrawdownWeight = 0.30m;
        const decimal pnlPenaltyWeight = 0.18m;
        
        const decimal timeInProfitWeight = 0.05m;
        const decimal maxProfitWeight = 0.03m;
        const decimal pnlRewardWeight = 0.02m;
        const decimal tradeEfficiencyWeight = 0.12m;
        
        decimal totalCandles = mHistoricalData.Length;

        // Calculate profitable trades count
        int profitableTradesCount = evaluationResponse.TakenDecisions.Count(d => d.IsProfitable == true);
        
        // Calculate trade efficiency (candles per profitable trade)
        decimal candlesPerProfitableTrade = profitableTradesCount > 0 ? totalCandles / profitableTradesCount : totalCandles;
        decimal tradeEfficiencyNormalized = profitableTradesCount > 0 
            ? 1 - Math.Min(Math.Abs(candlesPerProfitableTrade - optimalCandlesPerProfitableTrade) / optimalCandlesPerProfitableTrade, 1) 
            : 0;

        // Normalize metrics to 0-1 range
        decimal timeInDrawdownRatio = totalCandles > 0 ? (decimal)evaluationResponse.TimeInDrawdown / totalCandles : 0;
        decimal maxDrawdownNormalized = Math.Min(evaluationResponse.MaxDrawdown / profitNormalizationBase, 1);
        decimal pnlNormalized = evaluationResponse.PnL / profitNormalizationBase;
        decimal timeInProfitNormalized = totalCandles > 0 ? (decimal)evaluationResponse.TimeInProfit / totalCandles : 0;
        decimal maxProfitNormalized = Math.Min(evaluationResponse.MaxProfit / profitNormalizationBase, 1);

        // Calculate penalty scores (lower is worse)
        decimal timeDrawdownScore = (1 - timeInDrawdownRatio) * timeDrawdownWeight;
        decimal maxDrawdownScore = (1 - maxDrawdownNormalized) * maxDrawdownWeight;
        decimal pnlPenalty = pnlNormalized < 0 ? (1 + Math.Min(pnlNormalized, -1)) * pnlPenaltyWeight : pnlPenaltyWeight;

        // Calculate reward scores (higher is better)
        decimal timeInProfitScore = timeInProfitNormalized * timeInProfitWeight;
        decimal maxProfitScore = maxProfitNormalized * maxProfitWeight;
        decimal pnlReward = Math.Min(pnlNormalized, 1) * pnlRewardWeight;
        decimal tradeEfficiencyScore = tradeEfficiencyNormalized * tradeEfficiencyWeight;

        // Combine scores
        decimal totalScore = (timeDrawdownScore + maxDrawdownScore + pnlPenalty + 
                              timeInProfitScore + maxProfitScore + pnlReward + tradeEfficiencyScore) * maxScore;

        // Ensure score is within 0-1000 range
        evaluationResponse.Score = Math.Max(0, Math.Min(totalScore, maxScore));
    }

    private void HandleIsInPosition(TradeDecision decision, LineModel line)
    {
        mOpenPosition!.PnL = CalculatePnL(mOpenPosition.TradeDecision, line.Close, mOpenPosition.EntryPrice);
        UpdateEvaluationProperties();

        if (mOpenPosition.IsInProfit())
        {
            mEvaluationResult.TimeInProfit++;
        }
        else
        {
            mEvaluationResult.TimeInDrawdown++;
        }

        var closeTrade = CheckCloseTrade(decision);
        if (closeTrade)
        {
            HandleTradeClosed();
            return;
        }
    }

    private void UpdateEvaluationProperties()
    {
        var unrealisedEquity = mOpenPosition.PnL + mEquity;

        var overAllPnL = unrealisedEquity - INITIAL_EQUITY;

        mEvaluationResult.PnL = overAllPnL;

        if (mEvaluationResult.MaxProfit < overAllPnL)
        {
            mEvaluationResult.MaxProfit = overAllPnL;
        }

        if (mEvaluationResult.MaxDrawdown < overAllPnL * -1)
        {
            mEvaluationResult.MaxDrawdown = overAllPnL * -1;
        }
    }

    private void HandleTradeClosed()
    {
        mEquity += mOpenPosition.PnL;

        var overAllPnL = mEquity - INITIAL_EQUITY;

        mEvaluationResult.PnL = overAllPnL;

        if (mEvaluationResult.MaxProfit < overAllPnL)
        {
            mEvaluationResult.MaxProfit = overAllPnL;
        }

        if (mEvaluationResult.MaxDrawdown < overAllPnL * -1)
        {
            mEvaluationResult.MaxDrawdown = overAllPnL * -1;
        }

        mEvaluationResult.TakenDecisions.Last().IsProfitable = mOpenPosition.PnL > 0;

        mOpenPosition = null;

        mIsInPosition = false;
    }

    private bool CheckCloseTrade(TradeDecision decision)
    {
        return decision switch
        {
            TradeDecision.NONE => false,
             _ when decision != mOpenPosition!.TradeDecision => true,
            _ => false
        };
    }

    private void HandleIsNotInPosition(TradeDecision decision, LineModel line)
    {
        mEvaluationResult.TakenDecisions.Add(new TakenDecisionModel(decision, line.CandleTimestamp));

        mIsInPosition = true;

        var entryPrice = decision == TradeDecision.BUY ? line.Close + mSpread : line.Close - mSpread;

        mOpenPosition = new OpenPositionModel
        {
            EntryPrice = entryPrice,
            TradeDecision = decision,
            PnL = CalculatePnL(decision, line.Close, entryPrice)
        };

        mEvaluationResult.TimeInDrawdown++;
    }

    private decimal CalculatePnL(TradeDecision decision, decimal close, decimal entryPrice)
    {
        var priceDifference = decision == TradeDecision.BUY 
            ? close - entryPrice 
            : entryPrice - close;

        var pnl = priceDifference * mLotSize * 100000;

        return pnl;
    }

    private static MethodInfo GetMethodInfo(object strategyInstance)
    {
        var tmpMethodInfo = strategyInstance.GetType().GetMethod("Execute") 
            ?? throw new DoyStratOptimizerException(
                "Execute Method could not be found. Check your Strategy and ensure that it is written correctly.",
                System.Net.HttpStatusCode.BadRequest);

        return tmpMethodInfo;
    }

    private static JObject ConvertLineToJObject(LineModel line)
    {
        return JObject.FromObject(line);
    }


    /// These methods will not be used in this program, because an implementation that determines a take profit level is needed to calculate SL and Lot Size dynamically.
    /// These efforts here are useful for the dynamic calculation of SL and Lot Size in future implementations.

    //private decimal CalculateSLFromTP(decimal takeProfitPips)
    //{
    //    return takeProfitPips / 3;
    //}

    //private decimal CalculateLotSize(decimal slPips, decimal equity)
    //{
    //    return equity / (slPips * 10000000);
    //}
}
