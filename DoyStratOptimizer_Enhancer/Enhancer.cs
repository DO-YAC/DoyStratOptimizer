using DoyStratOptimizer_Common.Models;
using DoyStratOptimizer_Common.Models.Responses;
using DoyStratOptimizer_Tester;
using System.Collections.Concurrent;

namespace DoyStratOptimizer_Enhancer;

public class Enhancer
{
    private readonly EnhancerConfig mConfig;
    private readonly int mCpuCores = Environment.ProcessorCount;
    private readonly StrategyTester mStrategyTester;
    private readonly ConcurrentDictionary<decimal[], EvaluationResponse> mParameterResults = new();
    private readonly OptimizationConsoleInfo mConsoleInfo = new();

    public int CurrentGeneration { get; private set; } = 0;

    public Enhancer(EnhancerConfig config)
    {
        mConfig = config;

        var testerConfig = new TesterConfig(
            mConfig.StrategyAssebmly,
            mConfig.HistoricalData);

        mStrategyTester = new StrategyTester(testerConfig);
    }

    public async Task<KeyValuePair<decimal[], EvaluationResponse>> Optimize()
    {
        var population = GenerateInitialPopulation();

        var tcs = new TaskCompletionSource<KeyValuePair<decimal[], EvaluationResponse>>();

        RunOptimizationLoop(population, tcs);

        return await tcs.Task;
    }

    private void RunOptimizationLoop(List<decimal[]> population, TaskCompletionSource<KeyValuePair<decimal[], EvaluationResponse>> tcs)
    {
        while (true)
        {
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = mCpuCores
            };

            Parallel.ForEach(population, options, async (parameterArray) =>
            {
                mParameterResults.TryAdd(parameterArray, await mStrategyTester.EvaluateStrategy(parameterArray));
            });

            if (ScoreReachedTarget())
            {
                var kvp = mParameterResults
                    .OrderByDescending(kvp => kvp.Value.Score)
                    .First();

                tcs.SetResult(kvp);
                break;
            }

            var topPerformers = mParameterResults
                .OrderByDescending(kvp => kvp.Value.Score)
                .Take(10)
                .Select(kvp => kvp.Key)
                .ToList();

            CreateNewGeneration(topPerformers, population);
            CurrentGeneration++;
            UpdateConsoleInfo();
            mParameterResults.Clear();
            DisplayConsoleInfo();
        }
    }

    private void DisplayConsoleInfo()
    {
        Console.Clear();
        Console.WriteLine($"Generation: {mConsoleInfo.CurrentGeneration}");
        Console.WriteLine($"Max Score: {mConsoleInfo.MaxScore}");
        Console.WriteLine($"Best Parameters: {string.Join(", ", mConsoleInfo.BestParameters)}");
    }

    private void UpdateConsoleInfo()
    {
        mConsoleInfo.CurrentGeneration = CurrentGeneration;
        mConsoleInfo.MaxScore = (double)mParameterResults.Values.Max(result => result.Score);
        mConsoleInfo.BestParameters = mParameterResults
            .OrderByDescending(kvp => kvp.Value.Score)
            .First().Key;
    }

    private void CreateNewGeneration(List<decimal[]> topPerformers, List<decimal[]> population)
    {
        population.Clear();
        population.AddRange(topPerformers);

        var remaining = mConfig.Population - topPerformers.Count;

        foreach(var parent in topPerformers)
        {
            for (int i = 0; i < remaining / topPerformers.Count; i++)
            {
                population.Add(MutateParameters(parent));
            }
        }
    }

    private bool ScoreReachedTarget()
    {
        return mParameterResults.Values.Any(result => result.Score >= mConfig.TargetScore);
    }

    private List<decimal[]> GenerateInitialPopulation()
    {
        var population = new List<decimal[]>
        {
            mConfig.InitialParameters
        };

        for (int i = 0; i < mConfig.Population -1; i++)
        {
            population.Add(MutateParameters(mConfig.InitialParameters));
        }
        return population;
    }

    private decimal[] MutateParameters(decimal[] parameters)
    {
        var random = new Random();
        var mutatedParameters = new decimal[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var mutation = ((decimal)random.NextDouble() * 2 - 1) * mConfig.MutationFactor;
            mutatedParameters[i] = parameters[i] + mutation;
        }
        return mutatedParameters;
    }
}
