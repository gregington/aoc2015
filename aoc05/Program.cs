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
        var numNice = input.Where(IsNicePart1).ToEnumerable().Count();
        Console.WriteLine(numNice);
        return Task.CompletedTask;
    }

    public static Task Part2(IAsyncEnumerable<string> input)
    {
        var numNice = input.Where(IsNicePart2).ToEnumerable().Count();
        Console.WriteLine(numNice);
        return Task.CompletedTask;
    }

    private static bool IsNicePart1(string input)
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

    private static bool IsNicePart2(string input)
    {
        return HasDuplicatedTwoCharacters(input)
          && ContainsRepeatedLetterWithMiddleOtherLetter(input);
    }

    [GeneratedRegex(@"([a-z][a-z]).*\1")]
    private static partial Regex DuplicatedTwoCharactersRegex();
    private static bool HasDuplicatedTwoCharacters(string input)
    {
        var regex = DuplicatedTwoCharactersRegex();
        return regex.Match(input).Success;
    }

    [GeneratedRegex(@"^.*(a.a|b.b|c.c|d.d|e.e|f.f|g.g|h.h|i.i|j.j|k.k|l.l|m.m|n.n|o.o|p.p|q.q|r.r|s.s|t.t|u.u|v.v|w.w|x.x|y.y|z.z).*$")]
    private static partial Regex ContainsRepeatedLetterWithMiddleOtherLetterRegex();

    private static bool ContainsRepeatedLetterWithMiddleOtherLetter(string input)
    {
        var regex = ContainsRepeatedLetterWithMiddleOtherLetterRegex();
        return regex.Match(input).Success;
    }

}