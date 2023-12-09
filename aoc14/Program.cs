using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var inputOption = new Option<string>(
            name: "--input",
            description: "Input file path",
            getDefaultValue: () => "input.txt");

        var partOption = new Option<int>(
            name: "--part",
            description: "Part of the puzzle to solve",
            getDefaultValue: () => 1);

        var rootCommand = new RootCommand();
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(partOption);

        rootCommand.SetHandler(Run, inputOption, partOption);

        await rootCommand.InvokeAsync(args);
    }

    private async static Task Run(string input, int part)
    {
        var reindeer = await Parse(input);
        var task = part switch
        {
            1 => Part1(reindeer),
            2 => Part2(reindeer),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(IDictionary<string, IEnumerator<int>> reindeer)
    {
        var distances = reindeer.Values.Select(e =>
        {
            for (var i = 0; i < 2503; i++)
            {
                e.MoveNext();
            }
            return e.Current;
        });

        Console.WriteLine(distances.Max());
        return Task.CompletedTask;
    }

    public static Task Part2(IDictionary<string, IEnumerator<int>> reindeer)
    {
        var scores = reindeer.ToDictionary(kvp => kvp.Key, _ => 0);
        for (var i = 0 ; i < 2503; i++)
        {
            var distances = reindeer.ToDictionary(
                kvp => kvp.Key,
                kvp => 
                {
                    var enumerator = kvp.Value;
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
            );

            var inLead = distances.MaxBy(kvp => kvp.Value);
            scores[inLead.Key] += 1;
        }

        Console.WriteLine(scores.Values.Max());
        return Task.CompletedTask;
    }

    private static async Task<IDictionary<string, IEnumerator<int>>> Parse(string input)
    {
        var regex = ReindeerRegex();

        return await File.ReadLinesAsync(input)
            .Select(line => regex.Match(line).Groups)
            .ToDictionaryAsync(g => g["reindeer"].Value, 
                g => DistanceEnumerator(
                    Convert.ToInt32(g["speed"].Value),
                    Convert.ToInt32(g["flyTime"].Value),
                    Convert.ToInt32(g["restTime"].Value)));
    }

    private static IEnumerator<int> DistanceEnumerator(int speed, int flyTime, int restTime)
    {
        var distance = 0;
        var counter = flyTime;
        var state = ReindeerState.Flying;

        while (true)
        {
            counter--;
            if (state == ReindeerState.Flying)
            {
                distance += speed;
                yield return distance;
                if (counter == 0)
                {
                    state = ReindeerState.Resting;
                    counter = restTime;
                }
            }
            else if (state == ReindeerState.Resting)
            {
                yield return distance;
                if (counter == 0)
                {
                    state = ReindeerState.Flying;
                    counter = flyTime;
                }
            }
            else
            {
                throw new Exception("Unknown state");
            }
        }
    }

    [GeneratedRegex(@"^(?<reindeer>.*) can fly (?<speed>\d+) km\/s for (?<flyTime>\d+) seconds, but then must rest for (?<restTime>\d+) seconds.$")]
    private static partial Regex ReindeerRegex();
}

public enum ReindeerState
{
    Flying,
    Resting
}