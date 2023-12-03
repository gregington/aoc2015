using System.CommandLine;
using System.Security.Cryptography;
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

    private async static Task Run(string inputFile, int part)
    {
        var input = await File.ReadLinesAsync(inputFile).FirstAsync();
        if (part == 1)
        {
            await Part1(input);
        }
        else
        {
            await Part2(input);
        }
    }

    public static Task Part1(string input)
    {
        var number = Enumerable.Range(0, int.MaxValue)
            .Select(x => (Value: x, Input: $"{input}{x}"))
            .Select(x => (x.Value, Hash: Convert.ToHexString(MD5.HashData(Encoding.ASCII.GetBytes(x.Input)))))
            .First(x => x.Hash.StartsWith("00000")).Value;

        Console.WriteLine(number);
        
        return Task.CompletedTask;
    }

    public static Task Part2(string input)
    {
        return Task.CompletedTask;
    }


}