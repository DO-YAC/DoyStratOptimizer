
using DoyStratOptimizer_Common.Models;
using DoyStratOptimizer_Common.Models.Exceptions;
using DoyStratOptimizer_Common.Models.Responses;
using System.Net;
using System.Reflection;

namespace DoyStratOptimizer_Tester;

public class StrategyTester(TesterConfig mConfig)
{
    public async Task<EvaluationResponse> EvaluateStrategy(decimal[] parameterArray)
    {
        var strategyInstance = CreateStrategyInstance(mConfig.StrategyAssembly, parameterArray);
        var evaluator = new EvaluationCalculator(strategyInstance, mConfig.HistoricalData);

        var evaluationResponse = await evaluator.EvaluateStrategy();

        return evaluationResponse;
    }

    private static object CreateStrategyInstance(Assembly strategyAssembly, decimal[] parameterArray)
    {
        var strategyType = strategyAssembly.GetType("DoyStrategyPlayground.Strategy") 
            ?? throw new DoyStratOptimizerException(
                "The Strategytype was not found in the given Assembly",
                HttpStatusCode.NotFound);

        var tmpInstance = Activator.CreateInstance(strategyType, [.. parameterArray.Select(x => x as object)])
            ?? throw new DoyStratOptimizerException(
                "The strategy instance could not be created.",
                HttpStatusCode.BadRequest);

        return tmpInstance;
    }
    

}
