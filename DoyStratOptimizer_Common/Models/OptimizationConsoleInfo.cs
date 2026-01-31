namespace DoyStratOptimizer_Common.Models;

public record OptimizationConsoleInfo
{
    public double MaxScore { get; set; }
    public decimal[] BestParameters { get; set; }
    public int CurrentGeneration { get; set; }
}
