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
        var ingredients = await Parse(input);
        var task = part switch
        {
            1 => Part1(ingredients),
            2 => Part2(ingredients),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(List<Ingredient> ingredients)
    {
        var combinations = Combinations(ingredients.Count - 1, 100);
        var scores = combinations.Select(combination => (combination, Score(ingredients, combination)));
        var maxScore = scores.MaxBy(s => s.Item2);

        Console.WriteLine($"[{string.Join(", ", maxScore.Item1)}]: {maxScore.Item2}");

        return Task.CompletedTask;
    }

    public static Task Part2(List<Ingredient> ingredients)
    {
        var combinations = Combinations(ingredients.Count - 1, 100)
            .Where(combination => Enumerable.Range(0, ingredients.Count)
                    .Select(i => ingredients[i].Calories * combination[i])
                    .Sum() == 500);

        var scores = combinations.Select(combination => (combination, Score(ingredients, combination)));
        var maxScore = scores.MaxBy(s => s.Item2);

        Console.WriteLine($"[{string.Join(", ", maxScore.Item1)}]: {maxScore.Item2}");

        return Task.CompletedTask;
    }

    private static int Score(List<Ingredient> ingredients, ImmutableArray<int> combination)
    {
        int capacity = 0, durability = 0, flavor = 0, texture = 0;
        for (int i = 0; i < combination.Length; i++)
        {
            var quantity = combination[i];
            capacity += quantity * ingredients[i].Capacity;
            durability += quantity * ingredients[i].Durability;
            flavor += quantity * ingredients[i].Flavor;
            texture += quantity * ingredients[i].Texture;
        }

        return Math.Max(0, capacity)
            * Math.Max(0, durability)
            * Math.Max(0, flavor)
            * Math.Max(0, texture);
    }

    private static IEnumerable<ImmutableArray<int>> Combinations(int count, int totalQuantity)
    {
        if (count == 0)
        {
            yield return [totalQuantity];
            yield break;
        }

        foreach (var thisQuantity in Enumerable.Range(0, totalQuantity + 1))
        {
            ImmutableArray<int> baseList = [thisQuantity];
            var remainingQuantity = totalQuantity - thisQuantity;
            foreach (var rest in Combinations(count - 1, remainingQuantity))
            {
                yield return baseList.AddRange(rest);
            }
        }
    }

    private static async Task<List<Ingredient>> Parse(string input)
    {
        var regex = IngredientRegex();

        return await File.ReadLinesAsync(input)
            .Select(line => regex.Match(line).Groups)
            .Select(groups =>
            {
                var name = groups["name"].Value;
                var capacity = Convert.ToInt32(groups["capacity"].Value);
                var durability = Convert.ToInt32(groups["durability"].Value);
                var flavor = Convert.ToInt32(groups["flavor"].Value);
                var texture = Convert.ToInt32(groups["texture"].Value);
                var calories = Convert.ToInt32(groups["calories"].Value);
                
                return new Ingredient(name, capacity, durability, flavor, texture, calories);
            })
            .ToListAsync();
    }

    [GeneratedRegex(@"^(?<name>.+) capacity (?<capacity>-?\d+), durability (?<durability>-?\d+), flavor (?<flavor>-?\d+), texture (?<texture>-?\d+), calories (?<calories>-?\d+)$")]
    private static partial Regex IngredientRegex();
}

public record struct Ingredient(string Name, int Capacity, int Durability, int Flavor, int Texture, int Calories);
