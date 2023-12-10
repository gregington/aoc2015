using System.Collections.Concurrent;
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
        var grid = await Parse(input);
        var task = part switch
        {
            1 => Part1(grid),
            2 => Part2(grid),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(bool[][] grid)
    {
        for (var i = 0; i < 100; i++)
        {
            grid = UpdateGrid(grid);
        }

        var lightsOn = grid.Sum(row => row.Where(x => x).Count());
        Console.WriteLine(lightsOn);

        return Task.CompletedTask;
    }

    public static Task Part2(bool[][] grid)
    {
        TurnOnCorners(grid);
        for (var i = 0; i < 100; i++)
        {
            grid = UpdateGrid(grid);
            TurnOnCorners(grid);
        }

        var lightsOn = grid.Sum(row => row.Where(x => x).Count());
        Console.WriteLine(lightsOn);

        return Task.CompletedTask;
    }

    private static void TurnOnCorners(bool[][] grid)
    {
        var maxRow = grid.Length - 1;
        var maxCol = grid[0].Length - 1;

        grid[0][0] = true;
        grid[0][maxCol] = true;
        grid[maxRow][0] = true;
        grid[maxRow][maxRow] = true;
    }

    private static bool[][] UpdateGrid(bool[][] grid)
    {
        var newGrid = CreateGrid(grid.Length, grid[0].Length);

        for (var row = 0; row < grid.Length; row++)
        {
            for (var col = 0; col < grid[row].Length; col++)
            {
                var neighbors = GetNeighbors(row, col, grid);
                var newState = grid[row][col] switch
                {
                    true => neighbors.Where(x => x).Count() is 2 or 3,
                    false => neighbors.Where(x => x).Count() == 3
                };
                newGrid[row][col] = newState;
            }
        }

        return newGrid;
    }

    private static bool[][] CreateGrid(int rows, int cols) =>
        Enumerable.Range(0, rows)
            .Select(_ => Enumerable.Range(0, cols).Select(_ => false).ToArray())
            .ToArray();

    private static bool[] GetNeighbors(int row, int col, bool[][] grid)
    {
        var minRow = Math.Max(row - 1, 0);
        var maxRow = Math.Min(row + 1 + 1, grid.Length);

        var minCol = Math.Max(col - 1, 0);
        var maxCol = Math.Min(col + 1 + 1, grid[0].Length);

        var neighbors = new List<bool>();
        for (var r = minRow; r < maxRow; r++)
        {
            for (var c = minCol; c < maxCol; c++)
            {
                if (row == r && col == c)
                {
                    continue;
                }

                neighbors.Add(grid[r][c]);
            }
        }

        return [.. neighbors];
    }

    private static void PrintGrid(bool[][] grid)
    {
        Console.WriteLine("--------");
        Console.WriteLine();
        foreach (var line in grid)
        {
            foreach (var b in line)
            {
                Console.Write(b ? '#' : '.');
            }
            Console.WriteLine();
        }
    }

    private static async Task<bool[][]> Parse(string input) => 
        await File.ReadLinesAsync(input)
            .Select(line => line.Select(c => c == '#').ToArray())
            .ToArrayAsync();
}