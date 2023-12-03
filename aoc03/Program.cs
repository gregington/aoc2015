using System.CommandLine;

public class Program
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

    private static Task Run(string input, int part)
    {

        if (part == 1)
        {
            return Part1(input);
        }
        else
        {
            return Part2(input);
        }
    }

    public static Task Part1(string input)
    {
        var positions = Positions(File.ReadAllLines(input)[0]);        
        var uniqueHouses = positions.Distinct().Count();
        Console.WriteLine($"Unique houses: {uniqueHouses}");
        return Task.CompletedTask;
    }

    public static Task Part2(string input)
    {
        var directions = File.ReadAllLines(input)[0].ToArray();
        var santaDirections = directions.Where((_, i) => i % 2 == 0);
        var roboSantaDirections = directions.Where((_, i) => i % 2 == 1);

        var santaPositions = Positions(santaDirections);
        var roboSantaPositions = Positions(roboSantaDirections);

        var uniqueHouses = santaPositions.Concat(roboSantaPositions).Distinct().Count();
        Console.WriteLine($"Unique houses: {uniqueHouses}");

        return Task.CompletedTask;
    }

    private static IEnumerable<Position> Positions(IEnumerable<char> directions)
    {
        var x = 0;
        var y = 0;
        yield return new Position(x, y);
        foreach (var direction in directions)
        {
            switch (direction)
            {
                case '^':
                    y++;
                    break;
                case 'v':
                    y--;
                    break;
                case '>':
                    x++;
                    break;
                case '<':
                    x--;
                    break;
            }
            yield return new Position(x, y);
        }
    }

    public record Position(int X, int Y);
}