using System.CommandLine;
using System.ComponentModel;

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
        using var fs = File.OpenRead(input);
        var enumerable = FloorEnumerable(fs);

        if (part == 1)
        {
            return Part1(enumerable);
        }
        else
        {
            return Part2(enumerable);
        }
    }

    private static Task Part1(IEnumerable<int> enumerable)
    {
        var floor = enumerable.Last();
        Console.WriteLine($"Floor: {floor}");
        return Task.CompletedTask;
    }

    private static Task Part2(IEnumerable<int> enumerable)
    {
        var position = enumerable.Select((f, i) => (floor: f, index: i)).First(t => t.floor == -1).index + 1;
        Console.WriteLine($"Position: {position}");
        return Task.CompletedTask;
    }

    private static IEnumerable<int> FloorEnumerable(Stream stream)
    {
        int c;
        var floor = 0;
        do
        {
            c = stream.ReadByte();
            switch (c)
            {
                case '(':
                    floor++;
                    break;
                case ')':
                    floor--;
                    break;
            }
            yield return floor;
        } while (c != -1);
    }
}