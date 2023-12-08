using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        var contents = File.ReadAllText(input);
        var root = JsonSerializer.Deserialize<JsonNode>(contents);

        var task = part switch
        {
            1 => Part1(root!),
            2 => Part2(root!),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(JsonNode root)
    {
        Console.WriteLine(SumNumbersExcludingObjectWithValue(root));
        return Task.CompletedTask;
    }

    public static Task Part2(JsonNode root)
    {
        Console.WriteLine(SumNumbersExcludingObjectWithValue(root, "red"));
        return Task.CompletedTask;
    }

    public static long SumNumbersExcludingObjectWithValue(JsonNode node, string? value = null)
    {
        if (node is JsonArray)
        {
            var jsonArray = node.AsArray();
            return jsonArray
                .Where(item => item != null)
                .Select(item => SumNumbersExcludingObjectWithValue(item!, value)).Sum();
            
        }
        else if (node is JsonObject)
        {
            var jsonObject = node.AsObject();

            var anyRedValues = jsonObject
                .Select(kvp => kvp.Value)
                .Where(item => item != null)
                .Where(item => item!.GetValueKind() == JsonValueKind.String)
                .Any(item => (string)item! == value);

            if (anyRedValues)
            {
                return 0;
            }

            return jsonObject
                .Select(kvp => kvp.Value)
                .Where(item => item != null)
                .Select(item => SumNumbersExcludingObjectWithValue(item!, value))
                .Sum();
        }
        else if (node is JsonValue)
        {
            var jsonValue = node.AsValue();
            if (jsonValue.GetValueKind() == JsonValueKind.Number)
            {
                return (long) jsonValue;
            }
            return 0;
        }
        else
        {
            throw new Exception($"Unexpected type {node.GetType().Name}");
        }
    }
}