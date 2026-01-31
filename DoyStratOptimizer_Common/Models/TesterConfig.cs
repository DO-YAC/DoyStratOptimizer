using System.Reflection;

namespace DoyStratOptimizer_Common.Models;

public record TesterConfig(
    Assembly StrategyAssembly,
    LineModel[] HistoricalData)
{ }
