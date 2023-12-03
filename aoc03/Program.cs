using System.Collections;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

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
        var positions = Positions(fs);        

        if (part == 1)
        {
            return Part1(positions);
        }
        else
        {
            return Part2(positions);
        }
    }

    public static Task Part1(IEnumerable<Position> positions)
    {
        var uniqueHouses = positions.Distinct().Count();
        Console.WriteLine($"Unique houses: {uniqueHouses}");
        return Task.CompletedTask;
    }

    public static Task Part2(IEnumerable<Position> positions)
    {
        return Task.CompletedTask;
    }

    private static IEnumerable<Position> Positions(Stream stream)
    {
        int c;
        int x = 0, y = 0;
        do 
        {
            var position = new Position(x, y);
            yield return position;
            c = stream.ReadByte();
            switch (c)
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

        } while (c != -1);
    }

    public record Position(int X, int Y);
}