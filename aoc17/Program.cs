using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;

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

    private async static Task Run(string input, int part)
    {
        var containers = Parse(input);

        var task = part switch
        {
            1 => Part1(containers),
            2 => Part2(),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(ImmutableArray<int> containers)
    {
        var fills = Fill(containers, 150);
        Console.WriteLine(fills.Count());

        return Task.CompletedTask;
    }

    public static Task Part2()
    {
        return Task.CompletedTask;
    }

    private static IEnumerable<Solution> Fill(ImmutableArray<int> capacities, int toFill)
    {
        return Fill(capacities.Select(c => new Container(c)).ToImmutableHashSet(), [], toFill).Distinct();
    }

    private static IEnumerable<Solution> Fill(ImmutableHashSet<Container> unfilled, ImmutableHashSet<Container> filled, int toFill)
    {
        if (toFill == 0)
        {
            return [new Solution(filled)];
        }

        if (!unfilled.Any(x => x.Capacity <= toFill))
        {
            return [];
        }

        return unfilled
            .SelectMany((container) =>
            {
                return Fill(unfilled.Remove(container), filled.Add(container), toFill - container.Capacity);
            })
            .ToImmutableArray();
    }

    private static ImmutableArray<int> Parse(string input) =>
        File.ReadLinesAsync(input)
            .Select(x => Convert.ToInt32(x))
            .ToEnumerable()
            .Order()
            .ToImmutableArray();
}

public class Container
{


    public Container(int capacity)
    {
        Id = Guid.NewGuid();
        Capacity = capacity;
    }

    public Guid Id { get; init; }

    public int Capacity { get; init; }

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj is not Container)
        {
            return false;
        }
        return Id.Equals((obj as Container).Id);
    }

    public override string ToString() => Capacity.ToString();
}

public class Solution
{
    private readonly int hashCode;

    public Solution(IEnumerable<Container> containers)
    {
        Containers = [.. containers.OrderBy(x => x.Capacity)];

        var hash = new HashCode();
        foreach (var x in Containers)
        {
            hash.Add(x);
        }
        hashCode = hash.ToHashCode();
    }

    public ImmutableArray<Container> Containers { get; init; }

    public override bool Equals(object obj)
    {
        if (obj is not Solution)
        {
            return false;
        }
        var other = obj as Solution;

        return Containers.SequenceEqual(other.Containers);
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => $"[{string.Join(", ", Containers.Select(x => x.ToString()))}]";
}
