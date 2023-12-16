using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using Combinatorics.Collections;

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

        rootCommand.SetHandler(Run, partOption, inputOption);

        await rootCommand.InvokeAsync(args);
    }

    private async static Task Run(int part, string input)
    {
        var packageMasses = Parse(input);
        var task = part switch
        {
            1 => Part1(packageMasses),
            2 => Part2(packageMasses),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(ImmutableArray<long> packageMasses)
    {
        Console.WriteLine(MinQuantumEntanglement(packageMasses, 3));

        return Task.CompletedTask;
    }

    public static Task Part2(ImmutableArray<long> packageMasses)
    {
        Console.WriteLine(MinQuantumEntanglement(packageMasses, 4));

        return Task.CompletedTask;
    }

    private static long MinQuantumEntanglement(IReadOnlyList<long> allMasses, int numGroups)
    {
        var massPerGroup = (int) (allMasses.Sum() / numGroups);
        var minQuantumEntanglement = long.MaxValue;
        var minGroupSize = int.MaxValue;

        foreach (var group in GroupsOfMass(allMasses, massPerGroup))
        {
            if (minQuantumEntanglement != long.MaxValue && group.Count > minGroupSize)
            {
                break;
            }

            var candidateQuantumEntanglement = group.Aggregate((a, b) => a * b);
            if (candidateQuantumEntanglement < minQuantumEntanglement 
                && CanGroup(allMasses.Except(group).ToList(), numGroups - 1, massPerGroup))
            {
                minQuantumEntanglement = candidateQuantumEntanglement;
            }

            minGroupSize = Math.Min(minGroupSize, group.Count);
        }

        return minQuantumEntanglement;
    }

    private static IEnumerable<IReadOnlyList<long>> GroupsOfMass(IReadOnlyList<long> packageMasses, int massPerGroup)
    {
        for (var groupSize = 0; groupSize < packageMasses.Count + 1; groupSize++)
        {
            foreach (var combination in new Combinations<long>(packageMasses, groupSize))
            {
                if (combination.Sum() == massPerGroup)
                {
                    yield return combination;
                }
            }
        }
    }

    private static bool CanGroup(IReadOnlyList<long> masses, int numGroups, int massPerGroup)
    {
        if (numGroups == 0)
        {
            return masses.Count == 0;
        }

        foreach (var group in GroupsOfMass(masses, massPerGroup))
        {
            var remainingMasses = masses.Except(group).ToList();
            if (CanGroup(remainingMasses, numGroups - 1, massPerGroup))
            {
                return true;
            }
        }

        return false;
    }

    private static ImmutableArray<long> Parse(string input)
    {
        return File.ReadLinesAsync(input)
            .Select(x => Convert.ToInt64(x))
            .ToEnumerable()
            .ToImmutableArray();
    }
}

