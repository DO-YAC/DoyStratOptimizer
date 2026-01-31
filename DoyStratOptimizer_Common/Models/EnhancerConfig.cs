using System.Reflection;

namespace DoyStratOptimizer_Common.Models;

public record EnhancerConfig(
    decimal[] InitialParameters,
    Assembly StrategyAssebmly,
    LineModel[] HistoricalData, 
    decimal TargetScore,
    int Population,
    decimal MutationFactor)
{ }
