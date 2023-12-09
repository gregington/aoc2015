using System.Collections;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Runtime.Serialization;
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
        var sues = await Parse(input);
        var task = part switch
        {
            1 => Part1(sues),
            2 => Part2(),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(IEnumerable<Sue> sues)
    {
        var matches = sues
            .Where(sue => NullOrEqual(sue["children"], 3))
            .Where(sue => NullOrEqual(sue["cats"], 7))
            .Where(sue => NullOrEqual(sue["samoyeds"], 2))
            .Where(sue => NullOrEqual(sue["pomeranians"], 3))
            .Where(sue => NullOrEqual(sue["akitas"], 0))
            .Where(sue => NullOrEqual(sue["vizslas"], 0))
            .Where(sue => NullOrEqual(sue["goldfish"], 5))
            .Where(sue => NullOrEqual(sue["trees"], 3))
            .Where(sue => NullOrEqual(sue["cars"], 2))
            .Where(sue => NullOrEqual(sue["perfumes"], 1));

        foreach (var match in matches)
        {
            Console.WriteLine(match);
        }

        return Task.CompletedTask;
    }

    public static Task Part2()
    {
        return Task.CompletedTask;
    }

    private static bool NullOrEqual(int? value, int expected) => value == null || value == expected; 

    private static async Task<List<Sue>> Parse(string input)
    {
        var regex = SueRegex();

        return await File.ReadLinesAsync(input)
            .Select(line => regex.Match(line).Groups)
            .Select(g => (Id: Convert.ToInt32(g["id"].Value), RawProps: g["props"].Value))            
            .Select(x => (x.Id, Props: x.RawProps.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(y => y.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray()).ToArray()))
            .Select(x => (x.Id, Dict: x.Props.ToDictionary(y => y[0], y => Convert.ToInt32(y[1]))))
            .Select(x => new Sue(x.Id, x.Dict))
            .ToListAsync();
    }

    [GeneratedRegex(@"^Sue (?<id>\d+): (?<props>.+)$")]
    private static partial Regex SueRegex();
}

public class Sue
{
    public Sue(int id, IReadOnlyDictionary<string, int> props)
    {
        Id = id;
        Props = new Dictionary<string, int>(props);
    }

    public int Id { get; init; }

    private Dictionary<string, int> Props { get; init; }

    public int? this[string key]
    {
        get { return Props.TryGetValue(key, out int value) ? value : null; }
    }

    public override string ToString()
    {
        var propsStr = string.Join(", ", Props.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

        return $"{Id}: {propsStr}";
    }
}
