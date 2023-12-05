using System.CommandLine;

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
        var lines = File.ReadLinesAsync(input);
        var task = part switch
        {
            1 => Part1(lines),
            2 => Part2(lines),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(IAsyncEnumerable<string> lines)
    {
        var lengths = lines.Select(line => (Line: line, CharLength: line.Length, MemoryLength: MemoryLength(line)));

        var differences = lengths
            .Select(x => x.CharLength - x.MemoryLength);

        var differenceSum = differences.ToEnumerable().Sum();

        Console.WriteLine(differenceSum);
        return Task.CompletedTask;
    }

    public static Task Part2(IAsyncEnumerable<string> lines)
    {
        return Task.CompletedTask;
    }

    private static int MemoryLength(string line)
    {
        if (!line.StartsWith('"') || !line.EndsWith('"'))
        {
            throw new ArgumentException("Line does not start and end with double quotes", nameof(line));
        }
        var strippedLine = line[1..^1];

        var chars = strippedLine.ToCharArray().ToList();

        var i = 0;
        while (i < chars.Count)
        {
            if (chars[i] == '\\')
            {
                var next = chars[i + 1];
                if (next == 'x')
                {
                    if (!char.IsAsciiHexDigit(chars[i + 2]) || !char.IsAsciiHexDigit(chars[i + 3]))
                    {
                        throw new ArgumentException("Expcted two hex digits after \\x");
                    }
                    var newChar = (char) Convert.ToInt32(new string(new [] { chars[i + 2], chars[i + 3]}), 16);
                    chars[i] = newChar;
                    chars.RemoveRange(i + 1, 3);
                    i++;
                    continue;
                }
                
                if (next == '\\' || next == '"')
                {
                    chars[i] = next;
                    chars.RemoveAt(i + 1);
                    i++;
                    continue;
                }

                throw new ArgumentException($"Unexpected character following backslash: '{next}', line: {line}");
            }

            i++;
        }
        return  chars.Count;
    }

}