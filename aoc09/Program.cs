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

    private async static Task Run(string input, int part)
    {
        var (nodes, costs) = Parse(input);
        var task = part switch
        {
            1 => Part1(nodes, costs),
            2 => Part2(nodes, costs),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(string[] nodes, int[][] costs)
    {
        var solution = Bfs(costs, OptimisationType.Shortest);
        var stringPath = string.Join(" -> ", solution.Visited.Select(v => nodes[v]));
        Console.WriteLine($"{solution.Cost}: {stringPath}");

        return Task.CompletedTask;
    }

    public static Task Part2(string[] nodes, int[][] costs)
    {
        var solution = Bfs(costs, OptimisationType.Longest);
        var stringPath = string.Join(" -> ", solution.Visited.Select(v => nodes[v]));
        Console.WriteLine($"{solution.Cost}: {stringPath}");

        return Task.CompletedTask;
    }

    private static Path Bfs(int[][] costs, OptimisationType optimisation)
    {
        var nodes = Enumerable.Range(0, costs.Length).ToArray();
        var solution = new Path(optimisation == OptimisationType.Shortest ? int.MaxValue : int.MinValue, []);
        var initialPaths = Enumerable.Range(0, nodes.Length).Select(x => new Path(0, [x]));
        var queue = new Queue<Path>(initialPaths);

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            if (path.Visited.Length == nodes.Length)
            {
                if (optimisation == OptimisationType.Shortest ? path.Cost < solution.Cost : path.Cost > solution.Cost)
                {
                    solution = path;
                }
                continue;
            }

            var from = path.Visited[^1];
            var nextNodes = nodes.Where(n => !path.Visited.Contains(n))
                .Where(to => costs[from][to] != -1);
            var nextPaths = nextNodes
                .Select(to =>
                {
                    var newCost = path.Cost + costs[from][to];
                    var newVisited = new int[path.Visited.Length + 1];
                    Array.Copy(path.Visited, newVisited, path.Visited.Length);
                    newVisited[^1] = to;

                    return new Path(newCost, newVisited);
                })
                .Where(p => optimisation == OptimisationType.Shortest ? p.Cost < solution.Cost : p.Cost > solution.Cost);

            foreach (var newPath in nextPaths)
            {
                queue.Enqueue(newPath);
            }
        }

        return solution;
    }

    private static (string[] Nodes, int[][] Costs) Parse(string input)
    {
        var Regex = EdgeRegex();
        var matches = File.ReadAllLines(input)
            .Select(line => Regex.Match(line))
            .ToArray();

        var nodeNames = matches.SelectMany(m => new [] { m.Groups["from"].Value, m.Groups["to"].Value })
            .Distinct()
            .Order()
            .ToArray();

        var nodeMap = nodeNames.Zip(Enumerable.Range(0, nodeNames.Length))
            .ToDictionary(x => x.First, x => x.Second);

        var costs = Enumerable.Range(0, nodeNames.Length)
            .Select(_ => Enumerable.Range(0, nodeNames.Length).Select(_ => -1).ToArray())
            .ToArray();

        foreach (var match in matches)
        {
            var groups = match.Groups;
            var from = nodeMap[groups["from"].Value];
            var to = nodeMap[groups["to"].Value];
            var cost = int.Parse(groups["cost"].Value);

            costs[from][to] = cost;
            costs[to][from] = cost;
        }

        return (nodeNames, costs);
    }

    [GeneratedRegex(@"^(?<from>\w+) to (?<to>\w+) = (?<cost>\d+)$")]
    private static partial Regex EdgeRegex();
}

public record Edge(string From, string To, int Cost);

public record Path(int Cost, int[] Visited);

public enum OptimisationType
{
    Shortest,
    Longest
}