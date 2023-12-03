using System.CommandLine;

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

    private static Task Run(string input, int part)
    {
        if (part == 1)
        {
            return Part1(input);
        }
        else
        {
            return Part2(input);
        }
    }

    public static Task Part1(string input)
    {
        var boxes = File.ReadLinesAsync(input).Select(x => Box.Parse(x));
        var totalArea = boxes.ToEnumerable().Sum(b => b.WrappingArea);
        Console.WriteLine($"Total area: {totalArea}");
        return Task.CompletedTask;
    }

    public static Task Part2(string input)
    {
        var boxes = File.ReadLinesAsync(input).Select(x => Box.Parse(x));
        var totalLength = boxes.ToEnumerable().Sum(b => b.RibbonLength);
        Console.WriteLine($"Total length: {totalLength}");
        return Task.CompletedTask;
    }

    private record Box(int Length, int Width, int Height)
    {
        public static Box Parse(string line)
        {
            var parts = line.Split('x');
            return new Box(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        public int WrappingArea
        {
            get
            {
                var sides = new[] { Length * Width, Width * Height, Height * Length };
                return sides.Sum() * 2 + sides.Min();
            }
        }

        public int RibbonLength
        {
            get
            {
                var sides = new[] { Length, Width, Height }.OrderBy(x => x).ToArray();
                var minPerimeter = sides[0] * 2 + sides[1] * 2;
                var volume = Length * Width * Height;
                return minPerimeter + volume;
            }
                
        }
    }

}
