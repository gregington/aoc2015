using System.Collections.Frozen;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Security.Cryptography;

public class Program
{
    public static async Task Main(string[] args)
    {
        var partOption = new Option<int>(
            name: "--part",
            description: "Part of the puzzle to solve",
            getDefaultValue: () => 1);

        var rootCommand = new RootCommand();
        rootCommand.AddOption(partOption);

        rootCommand.SetHandler(Run, partOption);

        await rootCommand.InvokeAsync(args);
    }

    private async static Task Run(int part)
    {
        var input = 29_000_000;
        var task = part switch
        {
            1 => Part1(input),
            2 => Part2(input),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(int input)
    {
        FindHouse(10, input);
        return Task.CompletedTask;
    }

    public static Task Part2(int input)
    {
        return Task.CompletedTask;
    }


    private static int FindHouse(int presentsPerElf, int limit)
    {
        var presentCounts = new Dictionary<int, long>();
        var maxPresents = 0;

        var houseNumber = 0;
        while (true)
        {
            houseNumber++;
            var elves = Enumerable.Range(1, houseNumber).Where(x => houseNumber % x == 0);
            var presents = elves.Sum() * presentsPerElf;

            if (presents > maxPresents)
            {
                maxPresents = presents;
                Console.WriteLine($"House {houseNumber:0,0} got {presents:0,0} presents.");
            }

            if (presents >= limit)
            {
                return houseNumber;
            }
        }

    }

}