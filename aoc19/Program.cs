using System.Collections.Frozen;
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
        var (replacements, molecule) = await Parse(input);
        var task = part switch
        {
            1 => Part1(replacements, molecule),
            2 => Part2(replacements, molecule),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(FrozenDictionary<string, ImmutableArray<string>> replacements, string molecule)
    {
        var replacedCount = replacements.SelectMany(kvp => kvp.Value.SelectMany(r => Replace(molecule, kvp.Key, r)))
            .Distinct()
            .Count();

        Console.WriteLine(replacedCount);

        return Task.CompletedTask;
    }

    public static Task Part2(FrozenDictionary<string, ImmutableArray<string>> replacements, string target)
    {
        var reverseReplacements = replacements.SelectMany(kvp => kvp.Value.Select(x => (Replacement: x, Source: kvp.Key)))
            .OrderBy(x => x.Replacement.Length)
            .Reverse();

        var count = 0;
        var molecule = target;

        while (molecule != "e")
        {
            var replaced = false;
            foreach (var (replacement, source) in reverseReplacements)
            {
                var index = molecule.IndexOf(replacement);
                if (index != -1)
                {
                    molecule = string.Concat(molecule.AsSpan(0, index), source, molecule.AsSpan(index + replacement.Length));
                    replaced = true;
                    break;
                }
            }
            count++;
            if (!replaced)
            {
                throw new Exception("No replacement found");
            }
        }

        Console.WriteLine(count);

        return Task.CompletedTask;
    }

    private static IEnumerable<string> Replace(string molecule, string find, string replace)
    {
        var startIndex = 0;
        while (true)
        {
            var i = molecule.IndexOf(find, startIndex);
            if (i == -1)
            {
                break;
            }

            var replacement = string.Concat(molecule.AsSpan(0, i), replace, molecule.AsSpan(i + find.Length));
            yield return replacement;
            startIndex = i + 1;
        }
    }

    private static void PrintReplacements(IReadOnlyDictionary<string, ImmutableArray<string>> replacements)
    {
        foreach(var kvp in replacements.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"{kvp.Key, 5} => {string.Join(", ", kvp.Value)}");
        }
    }

    public static async Task<(FrozenDictionary<string, ImmutableArray<string>> Replacements, string molecule)> Parse(string input)
    {
        var replacements = new Dictionary<string, ImmutableArray<string>>();
     
        var lines = await File.ReadAllLinesAsync (input);     
        foreach (var line in lines)
        {
            if (line.Trim() == string.Empty)
            {
                continue;
            }

            if (line.Contains("=>")) {
                var parts = line.Split("=>", StringSplitOptions.TrimEntries);

                var arr = replacements.TryGetValue(parts[0], out var value) ? value : [];
                replacements[parts[0]] = arr.Add(parts[1]);

                continue;
            }

            return (replacements.ToFrozenDictionary(), line);
        }
        throw new Exception("Could not parse input");
    }
}

public record Candidate(string Molecule, int steps);