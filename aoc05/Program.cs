using System.CommandLine;
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

    private async static Task Run(string inputFile, int part)
    {
        var input = File.ReadLinesAsync(inputFile);
        if (part == 1)
        {
            await Part1(input);
        }
        else
        {
            await Part2(input);
        }
    }

    public static Task Part1(IAsyncEnumerable<string> input)
    {
        var numNice = input.Where(IsNice).ToEnumerable().Count();
        Console.WriteLine(numNice);
        return Task.CompletedTask;
    }

    public static Task Part2(IAsyncEnumerable<string> input)
    {
        return Task.CompletedTask;
    }

    private static bool IsNice(string input)
    {
        return AtLeast3Vowels(input)
          && HasDoubleLetter(input)
          && !ContainsInvalidCharacters(input);
    }

    [GeneratedRegex(@"^.*[aeiou].*[aeiou].*[aeiou].*$")]
    private static partial Regex AtLeast3VowelsRegex();

    private static bool AtLeast3Vowels(string input)
    {
        var regex = AtLeast3VowelsRegex();
        return regex.Match(input).Success;
    }

    [GeneratedRegex(@"^.*(aa|bb|cc|dd|ee|ff|gg|hh|ii|jj|kk|ll|mm|nn|oo|pp|qq|rr|ss|tt|uu|vv|ww|xx|yy|zz).*$")]
    private static partial Regex HasDoubleLetterRegex();

    private static bool HasDoubleLetter(string input)
    {
        var regex = HasDoubleLetterRegex();
        return regex.Match(input).Success;
    }

    [GeneratedRegex(@"^.*(ab|cd|pq|xy).*$")]
    private static partial Regex InvalidCharactersRegex();

    private static bool ContainsInvalidCharacters(string input)
    {
        var regex = InvalidCharactersRegex();
        return regex.Match(input).Success;
    }

}