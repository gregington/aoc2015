using System.Collections.Immutable;
using System.CommandLine;
using System.Numerics;
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
        var (wires, gates) = await Parse(input);
        var task = part switch
        {
            1 => Part1(wires, gates),
            2 => Part2(wires, gates),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(Dictionary<string, ushort?>wires, List<Gate> gates)
    {
        while (wires.Any(x => x.Value == null))
        {
            foreach (var gate in gates)
            {
                // If any of the gate's inputs are unknown, skip
                if (gate.Inputs.Any(x => wires[x] == null))
                {
                    continue;
                }

                // If the gate's output is already known, skip
                if (wires[gate.Output] != null)
                {
                    continue;
                }

                var value = gate.Evaluate(wires);
                wires[gate.Output] = value;
            }
        }

        foreach (var wire in wires.Keys.Order())
        {
            Console.WriteLine($"{wire} -> {wires[wire]}");
        }
        return Task.CompletedTask;
    }

    public static Task Part2(Dictionary<string, ushort?> wires, List<Gate> gates)
    {
        return Task.CompletedTask;
    }

    private static async Task<(Dictionary<string, ushort?> Wires, List<Gate> Gates)> Parse(string input)
    {
        var wireRegex = WireRegex();
        var passthroughRegex = PassThroughRegex();
        var binaryRegex = BinaryRegex();
        var shiftRegex = ShiftRegex();
        var notRegex = NotRegex();

        var wires = new Dictionary<string, ushort?>();
        var gates = new List<Gate>();

        var lines = await File.ReadAllLinesAsync(input);
        foreach (var line in lines)
        {
            Match match;

            match = wireRegex.Match(line);
            if (match.Success)
            {
                var groups = match.Groups;
                wires[groups["wire"].Value] = ushort.Parse(groups["value"].Value);
                continue;
            }

            match = passthroughRegex.Match(line);
            if (match.Success)
            {
                var groups = match.Groups;
                var input1 = groups["input"].Value;
                var output = groups["output"].Value;

                gates.Add(new PassThroughGate(input1, output));
                continue;
            }

            match = binaryRegex.Match(line);
            if (match.Success)
            {
                var groups = match.Groups;
                var input1 = groups["input1"].Value;
                var input2 = groups["input2"].Value;
                var output = groups["output"].Value;
                var @operator = groups["operator"].Value;

                object i1 = ushort.TryParse(input1, out var i1Value) ? i1Value : input1;
                object i2 = ushort.TryParse(input2, out var i2Value) ? i2Value : input2;

                Gate gate = @operator switch
                {
                    "AND" => new And(i1, i2, output),
                    "OR" => new Or(i1, i2, output),
                    _ => throw new ArgumentException($"Invalid operator: {@operator}", nameof(@operator))
                };

                gates.Add(gate);
                continue;
            }

            match = shiftRegex.Match(line);
            if (match.Success)
            {
                var groups = match.Groups;
                var input1 = groups["input"].Value;
                var output = groups["output"].Value;
                var @operator = groups["operator"].Value;
                var shift = int.Parse(groups["shift"].Value);

                Gate gate = @operator switch
                {
                    "LSHIFT" => new LeftShift(input1, shift, output),
                    "RSHIFT" => new RightShift(input1, shift, output),
                    _ => throw new ArgumentException($"Invalid operator: {@operator}", nameof(@operator))
                };

                gates.Add(gate);
                continue;
            }

            match = notRegex.Match(line);
            if (match.Success)
            {
                var groups = match.Groups;
                var input1 = groups["input"].Value;
                var output = groups["output"].Value;

                Gate gate = new Not(input1, output);

                gates.Add(gate);
                continue;
            }

            throw new ArgumentException($"Invalid line: {line}", nameof(line));
        }

        // Add all unknown wires
        var gateWires = gates.SelectMany(g => g.Inputs.AsEnumerable().Concat(new [] {g.Output})).ToImmutableHashSet();
        var unknownWires = gateWires.Except(wires.Keys);
        foreach (var wire in unknownWires)
        {
            wires[wire] = null;
        }

        return (wires, gates);
    }

    [GeneratedRegex(@"^(?<value>\d+) -> (?<wire>\D+)$")]
    private static partial Regex WireRegex();

    [GeneratedRegex(@"^(?<input>[a-z]+) -> (?<output>[a-z]+)$")]
    private static partial Regex PassThroughRegex();

    [GeneratedRegex(@"^(?<input1>.+) (?<operator>(AND|OR)) (?<input2>.+) -> (?<output>\D+)$")]
    private static partial Regex BinaryRegex();

    [GeneratedRegex(@"^(?<input>\D+) (?<operator>(LSHIFT|RSHIFT)) (?<shift>\d+) -> (?<output>\D+)$")]
    private static partial Regex ShiftRegex();
    
    [GeneratedRegex(@"^NOT (?<input>\D+) -> (?<output>\D+)$")]
    private static partial Regex NotRegex();
}


public interface Gate
{
    IReadOnlySet<string> Inputs { get; }

    string Output { get; }

    ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values);
}

public class PassThroughGate : Gate
{
    private readonly string input;

    private readonly ImmutableHashSet<string> inputs;

    public PassThroughGate(string input, string output)
    {
        this.input = input;
        inputs = [input];
        Output = output;
    }

    public IReadOnlySet<string> Inputs => inputs;

    public string Output { get; init; }

    public ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values)
    {
        return values[input];
    }
}

public class And : Gate
{
    private readonly object input1;

    private readonly object input2;

    private readonly ImmutableHashSet<string> inputs;

    public And(object input1, object input2, string output)
    {
        this.input1 = input1;
        this.input2 = input2;
        inputs = new object[] {input1, input2}
            .Where(x => x is string)
            .Select(x => (string)x)
            .ToImmutableHashSet();
        Output = output;
    }

    public IReadOnlySet<string> Inputs => inputs;

    public string Output { get; init; }

    public ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values)
    {
        ushort? value1;
        ushort? value2;

        if (input1 is string v)
        {
            if (values[v] == null)
            {
                return null;
            }
            value1 = values[v];
        }
        else
        {
            value1 = (ushort) input1;
        }

        if (input2 is string w)
        {
            if (values[w] == null)
            {
                return null;
            }
            value2 = values[w];
        }
        else
        {
            value2 = (ushort) input2;
        }

        if (value1 == null || value2 == null)
        {
            return null;
        }

        return (ushort)(value1 & value2);
    }
}

public class Or : Gate
{
    private readonly object input1;

    private readonly object input2;

    private readonly ImmutableHashSet<string> inputs;

    public Or(object input1, object input2, string output)
    {
        this.input1 = input1;
        this.input2 = input2;
        inputs = new object[] {input1, input2}
            .Where(x => x is string)
            .Select(x => (string)x)
            .ToImmutableHashSet();
        Output = output;
    }

    public IReadOnlySet<string> Inputs => inputs;

    public string Output { get; init; }

    public ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values)
    {
        ushort? value1;
        ushort? value2;

        if (input1 is string v)
        {
            if (values[v] == null)
            {
                return null;
            }
            value1 = values[v];
        }
        else
        {
            value1 = (ushort) input1;
        }

        if (input2 is string w)
        {
            if (values[w] == null)
            {
                return null;
            }
            value2 = values[w];
        }
        else
        {
            value2 = (ushort) input2;
        }

        if (value1 == null || value2 == null)
        {
            return null;
        }

        return (ushort)(value1 | value2);
    }
}

public class LeftShift : Gate
{
    private readonly string input;

    private readonly int shift;

    private readonly ImmutableHashSet<string> inputs;

    public LeftShift(string input, int shift, string output)
    {
        this.input = input;
        this.shift = shift;
        inputs = [input];
        Output = output;
    }

    public IReadOnlySet<string> Inputs => inputs;

    public string Output { get; init; }

    public ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values)
    {
        if (values[input] == null)
        {
            return null;
        }

        return (ushort)(values[input]! << shift);
    }
}

public class RightShift : Gate
{
    private readonly string input;

    private readonly int shift;

    private readonly ImmutableHashSet<string> inputs;

    public RightShift(string input, int shift, string output)
    {
        this.input = input;
        this.shift = shift;
        inputs = [input];
        Output = output;
    }

    public IReadOnlySet<string> Inputs => inputs;

    public string Output { get; init; }

    public ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values)
    {
        if (values[input] == null)
        {
            return null;
        }

        return (ushort)(values[input]! >> shift);
    }
}

public class Not : Gate
{
    private readonly string input;

    private readonly ImmutableHashSet<string> inputs;

    public Not(string input, string output)
    {
        this.input = input;
        inputs = [input];
        Output = output;
    }

    public IReadOnlySet<string> Inputs => inputs;

    public string Output { get; init; }

    public ushort? Evaluate(IReadOnlyDictionary<string, ushort?> values)
    {
        if (values[input] == null)
        {
            return null;
        }

        return (ushort) ~values[input]!;
    }
}