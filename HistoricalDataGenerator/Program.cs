using HistoricalDataGenerator;
using System.Text.Json;

var lines = new List<LineModel>();

Console.WriteLine("How many years?: ");
var years = int.Parse(Console.ReadLine()!);

var hours = years * 365 * 24;

var startValuePrice = 5000m;
var startValueRsi = 45m;

for (int i = 0; i < hours; i++)
{
    var line = new LineModel(
        DateTime.UtcNow,
        MutatePrice(startValuePrice),
        MutatePrice(startValuePrice),
        MutatePrice(startValuePrice),
        MutatePrice(startValuePrice),
        100m, MutateRSI(startValueRsi));

    startValuePrice = line.Close;
    startValueRsi = line.Rsi;
    lines.Add(line);
}

var jsonContent = JsonSerializer.Serialize(lines);

var filePath = Path.Combine(AppContext.BaseDirectory, "DummyData.json");
Console.WriteLine(filePath);
File.WriteAllText(filePath, jsonContent);
Console.WriteLine("Finished");
Console.ReadLine();

static decimal MutateRSI(decimal value)
{
    var random = new Random();
    var polarisation = random.Next(2) == 1;

    if (value - 2.5m <= 0)
    {
        polarisation = true;
    }
    else if (value + 2.5m >= 100)
    {
        polarisation = false;
    }

        value += polarisation ? (decimal)random.NextDouble() * 2.5m : (decimal)random.NextDouble() * -2.5m;
    return value;
}

static decimal MutatePrice(decimal value)
{
    var random = new Random();
    var polarisation = random.Next(2) == 1;

    if (value - 15m <= 0)
    {
        polarisation = true;
    }

    value += polarisation ? (decimal)random.NextDouble() * 15m : (decimal)random.NextDouble() * -2.5m;
    return value;
}