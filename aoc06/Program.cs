using System.CommandLine;
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
        var task = part switch
        {
            1 => Part1(input),
            2 => Part2(input),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(string input)
    {
        var grid = CreateGrid();
        var lines = File.ReadLinesAsync(input);
        var instructions = lines.Select(x => Instruction.Parse(x)).ToEnumerable();

        foreach (var instruction in instructions)
        {
            instruction.Apply(grid);
        }

        var turnedOn = grid.Select(x => x.Count(y => y)).Sum();
        Console.WriteLine($"Lights turned on: {turnedOn}");

        return Task.CompletedTask;
    }

    public static Task Part2(string input)
    {
        return Task.CompletedTask;
    }

    private static bool[][] CreateGrid()
    {
        var grid = new bool[1000][];
        for (var i = 0; i < 1000; i++)
        {
            grid[i] = new bool[1000];
        }

        return grid;
    }

    public record Point(int X, int Y);

    public enum Operation
    {
        TurnOn,
        TurnOff,
        Toggle
    }

    [GeneratedRegex(@"^(?<operation>turn off|turn on|toggle) (?<startX>\d+),(?<startY>\d+) through (?<endX>\d+),(?<endY>\d+)$")]
    public static partial Regex LineRegex();


    public partial record Instruction(Operation Operation, Point Start, Point End)
    {
        public static Instruction Parse(string line)
        {
            var lineRegex = LineRegex();

            var match = lineRegex.Match(line);
            var operation = match.Groups["operation"].Value switch
            {
                "turn on" => Operation.TurnOn,
                "turn off" => Operation.TurnOff,
                "toggle" => Operation.Toggle,
                _ => throw new ArgumentException($"Invalid action: {match.Groups["action"].Value}", nameof(line))
            };

            var start = new Point(
                int.Parse(match.Groups["startX"].Value),
                int.Parse(match.Groups["startY"].Value));
            var end = new Point(
                int.Parse(match.Groups["endX"].Value),
                int.Parse(match.Groups["endY"].Value));

            return new Instruction(operation, start, end);
        }

        public void Apply(bool[][] grid)
        {
            var startX = Math.Min(Start.X, End.X);
            var endX = Math.Max(Start.X, End.X);

            var startY = Math.Min(Start.Y, End.Y);
            var endY = Math.Max(Start.Y, End.Y);

            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY; y <= endY; y++)
                {
                    switch (Operation)
                    {
                        case Operation.TurnOn:
                            grid[x][y] = true;
                            break;
                        case Operation.TurnOff:
                            grid[x][y] = false;
                            break;
                        case Operation.Toggle:
                            grid[x][y] = !grid[x][y];
                            break;
                        default:
                            throw new ArgumentException($"Invalid operation: {Operation}", nameof(Operation));
                    }
                }
            }
        }
    };
}