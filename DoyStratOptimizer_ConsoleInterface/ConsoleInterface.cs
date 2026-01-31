using DoyStratOptimizer_Common.Models;
using DoyStratOptimizer_Common.Models.Exceptions;
using DoyStratOptimizer_Enhancer;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace DoyStratOptimizer_ConsoleInterface;

public class ConsoleInterface()
{
    public static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Welcome to the Doyac Strategy Optimizer!");

        string strategyPath;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Enter the Path to your StrategyAssembly: ");
            strategyPath = Console.ReadLine()!;

            if (File.Exists(strategyPath))
            {
                break;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("File does not exist. Press any key to try again.");
            Console.ReadLine();
            Console.Clear();
        }

        Assembly strategyAssembly = null!;

        try
        {
            strategyAssembly = Assembly.LoadFrom(strategyPath);
        }catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error - Could not load strategy assembly because '{ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            Environment.Exit(1);
        }

        string chartDataPath;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Enter the path to your historical data. Make sure that it is in JSON format and aligns with our line model: ");
            chartDataPath = Console.ReadLine()!;

            if (File.Exists(chartDataPath))
            {
                break;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("File does not exist. Press any key to try again.");
            Console.ReadLine();
            Console.Clear();
        }

        var chartLines = File.ReadAllText(chartDataPath);

        LineModel[] lines = [];
        try
        {
            lines = JsonSerializer.Deserialize<LineModel[]>(chartLines)!;

            if (lines == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error - Could not read Chartdata. Press any key to exit...");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error - Could not read Chartdata because '{ex.Message}' Press any key to exit...");
            Console.ReadLine();
            Environment.Exit(1);
        }

        decimal targetScore;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Enter your desired strategy score (0 - 1000), keep in mind, that higher scores require longer computation: ");

            if (!int.TryParse(Console.ReadLine(), out var inputTargetScore))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error - Failed to parse the score. Did you enter a valid number?");
                Console.ReadLine();
                continue;
            }

            if (inputTargetScore < 0 || inputTargetScore > 1000)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error - The provided targetscore is out of bounds. \n Try again with a valid number...");
                Console.ReadLine();
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Blue;
            }

            targetScore = inputTargetScore;
            break;
        }

        int population;
        while (true)
        {
            Console.ForegroundColor= ConsoleColor.Blue;
            Console.WriteLine("Enter your desired population size per generation. It has to be divisible by 10: ");
            if (!int.TryParse(Console.ReadLine(), out var inputPopulation))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error - Failed to parse population. Did you enter a valid number?");
                Console.ReadLine();
                continue;
            }

            if (inputPopulation % 10 != 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error - The provided populationsize is not divisible by 10.");
                Console.ReadLine();
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            population = inputPopulation;
            break;
        }

        decimal mutationFactor;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Enter the desired mutationfactor(0.0 - 5.0): ");

            if (!decimal.TryParse(Console.ReadLine(), out var inputMutationFactor))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error - Failed to parse mutationfactor, did you enter a valid number?");
                Console.ReadLine();
                continue;
            }

            if (inputMutationFactor < 0 || inputMutationFactor > 5)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Error - Mutationfactor out of bounds.");
                Console.ReadLine();
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            mutationFactor = inputMutationFactor;
            break;
        }

        var parameterNames = CreateParameterNameArray(strategyAssembly);

        List<decimal> parameterValues = [];

        foreach (var parameterName in parameterNames)
        {
            Console.WriteLine($"Enter initial value for '{parameterName}': ");
            parameterValues.Add(decimal.Parse(Console.ReadLine()!));
        }

        var enhancerConfig = new EnhancerConfig([.. parameterValues], strategyAssembly, lines, targetScore, population, mutationFactor);

        var enhancer = new Enhancer(enhancerConfig);

        var result = enhancer.Optimize().GetAwaiter().GetResult();

        var resultPath = Path.Join(AppContext.BaseDirectory, "result.json");

        File.WriteAllText(resultPath, JsonSerializer.Serialize(result));
    }

    private static List<string> CreateParameterNameArray(Assembly strategyAssembly)
    {
        var strategyType = strategyAssembly.GetType("DoyStrategyPlayground.Strategy")
            ?? throw new DoyStratOptimizerException(
                "The Strategytype was not found in the given Assembly",
                HttpStatusCode.NotFound);

        var constructor = strategyType.GetConstructors()?[0] ?? throw new DoyStratOptimizerException("The Constructor was not found in the provided strategy.", HttpStatusCode.NotFound);

        var parameters = constructor.GetParameters() ?? throw new DoyStratOptimizerException("No Parameters found in the constructor.", HttpStatusCode.NotFound);

        List<string> names = [];

        foreach ( var parameter in parameters)
        {
            names.Add(parameter.Name!);
        }

        return names;
    }
}
