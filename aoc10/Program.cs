using System.CommandLine;
using System.Text;

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
        var str = File.ReadAllLines(input)[0];
        var task = part switch
        {
            1 => Part1(str),
            2 => Part2(str),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(string input)
    {
        for (var i = 0; i < 40; i++)
        {
            input = Expand(input);
        }
        Console.WriteLine(input.Length);
        return Task.CompletedTask;
    }

    public static Task Part2(string input)
    {
        for (var i = 0; i < 50; i++)
        {
            input = Expand(input);
        }
        Console.WriteLine(input.Length);
        return Task.CompletedTask;
    }

    private static string Expand(string input)
    {
        var output = new StringBuilder();
        var lastChar = '\0';
        var lastCount = 0;

        foreach (var c in input.ToCharArray())
        {
            if (c == lastChar)
            {
                lastCount++;
                continue;
            }

            if (lastChar != '\0')
            {
                output.Append(lastCount);
                output.Append(lastChar);
            }
            lastChar = c;
            lastCount = 1;
        }

        output.Append(lastCount);
        output.Append(lastChar);
        return output.ToString();
    }
}