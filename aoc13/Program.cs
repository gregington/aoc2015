using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        var (people, happinessMatrix) = await Parse(input);

        var task = part switch
        {
            1 => Part1(people, happinessMatrix),
            2 => Part2(people, happinessMatrix),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(string[] people, int[][] matrix)
    {
        var permutations = Permute(Enumerable.Range(0, people.Length).ToArray());
        var scores = permutations.Select(p => (Seating: p, Score: Score(p, matrix)));

        Console.WriteLine(scores.Select(s => s.Score).Max());

        return Task.CompletedTask;
    }

    public static Task Part2(string[] people, int[][] matrix)
    {
        return Task.CompletedTask;
    }

    public static async Task<(string[] People, int[][] happinessMatrix)> Parse(string input)
    {
        var regex = Regex();

        var matches = await File.ReadLinesAsync(input)
            .Select(line => regex.Match(line))
            .ToArrayAsync();

        var people = matches
            .SelectMany(match => new string[] { match.Groups["person"].Value, match.Groups["nextTo"].Value } as IEnumerable<string> )
            .Distinct()
            .Order()
            .ToArray();

        var peopleMap = people.Zip(Enumerable.Range(0, people.Length))
            .ToDictionary(x => x.First, x => x.Second);

        var matrix = people.Select(_ => people.Select(_ => 0).ToArray()).ToArray();

        foreach(var match in matches)
        {
            var groups = match.Groups;
            var sign = groups["sign"].Value == "gain" ? 1 : -1;
            var score = sign * Convert.ToInt32(groups["amount"].Value);
            var person = peopleMap[groups["person"].Value];
            var nextTo = peopleMap[groups["nextTo"].Value];
            
            matrix[person][nextTo] = score;
        }

        return (people, matrix);
    }

    public static int Score(int[] seating, int[][] matrix)
    {
        var count = seating.Length;

        var score = 0;
        for (var i = 0; i < count; i++)
        {
            var person = seating[i];
            var left = seating[(i - 1 + count) % count];
            var right = seating[(i + 1) % count];

            score += matrix[person][left];
            score += matrix[person][right];
        }
        return score;
    }

    public static IEnumerable<int[]> Permute(int[] items)
    {
        if (items.Length == 1)
        {
            yield return items;
        }
        foreach (var item in items)
        {
            var rest = items.Where(i => i != item).ToArray();
            foreach (var p in Permute(rest))
            {
                var newArr = new int[rest.Length + 1];
                newArr[0] = item;
                Array.Copy(p, 0, newArr, 1, p.Length);
                yield return newArr;
            }
        }        
    }

    [GeneratedRegex(@"^(?<person>.+) would (?<sign>gain|lose) (?<amount>\d+) happiness units by sitting next to (?<nextTo>.+).$")]
    private static partial Regex Regex();
}