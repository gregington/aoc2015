using System.CommandLine;
using System.CommandLine.Parsing;
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

        rootCommand.SetHandler(Run, partOption, inputOption);

        await rootCommand.InvokeAsync(args);
    }

    private async static Task Run(int part, string input)
    {
        var (row, col) = await Parse(input);
        var task = part switch
        {
            1 => Part1(row, col),
            2 => Part2(row, col),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(int row, int col)
    {
        var x = DiagonalSequence()
            .Zip(Codes(), (a, b) => (a.Row, a.Col, Code: b))
            .Where(y => y.Row == row && y.Col == col)
            .Select(x => x.Code)
            .First();

        Console.WriteLine(x);
        return Task.CompletedTask;
    }

    public static Task Part2(int row, int col)
    {
        return Task.CompletedTask;
    }


    private static IEnumerable<(int Seq, int Row, int Col)> DiagonalSequence()
    {
        var row = 1;
        var col = 1;
        var seq = 1;

        while (true)
        {
            yield return (seq, row, col);
            seq++;
            if (row == 1)
            {
                row = col + 1;
                col = 1;
            }
            else
            {
                col++;
                row--;
            }
        }
    }

    private static IEnumerable<int> Codes()
    {
        var code = 20151125L;

        while (true)
        {
            yield return (int) code;

            code = (code * 252533) % 33554393;
        }
    }

    private static async Task<(int Row, int Column)> Parse(string input)
    {
        var regex = InputRegex();
        var line = await File.ReadLinesAsync(input).FirstAsync();
        var match = regex.Match(line);
        return (Convert.ToInt32(match.Groups["row"].Value), Convert.ToInt32(match.Groups["column"].Value));
    }

    [GeneratedRegex(@"^.*row (?<row>\d+), column (?<column>\d+).*$")]
    private static partial Regex InputRegex();
}

