using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
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
        var instructions = Parse(input);

        var task = part switch
        {
            1 => Part1(instructions),
            2 => Part2(),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(ImmutableArray<Instruction> instructions)
    {
        var computer = new Computer(instructions, 0, 0, 0);
        computer = computer.Execute();
        Console.WriteLine(computer);

        return Task.CompletedTask;
    }

    public static Task Part2()
    {
        return Task.CompletedTask;
    }

    private static ImmutableArray<Instruction> Parse(string input)
    {
        return File.ReadLinesAsync(input)
            .Select(ParseInstruction)
            .ToEnumerable()
            .ToImmutableArray();
    }

    private static Instruction ParseInstruction(string line)
    {
        var regex = InstructionRegex();

        var match = regex.Match(line);
        var groups = match.Groups;
        var opcode = groups["opcode"].Value;
        var register = groups["register"].Success ? (char?) groups["register"].Value[0] : null;
        var offset = groups["offset"].Success ? (int?) Convert.ToInt32(groups["offset"].Value) : null;

        return new Instruction(opcode, register, offset);
    }

    public record Instruction(string Opcode, char? Register, int? Offset)
    {
        public override string ToString()
        {
            var output = Opcode;
            if (Register.HasValue)
            {
                output += $" {Register.Value}";
            }

            if (Offset.HasValue)
            {
                output += $" {Offset.Value:+0;-0;+0}";
            }
            return output;
        }
    }

    public record Computer(ImmutableArray<Instruction> Instructions, uint InstructionPointer, uint A, uint B)
    {
        public Instruction CurrentInstruction => Instructions[(int)InstructionPointer];

        public bool IsHalted => InstructionPointer >= Instructions.Length;

        public Computer Step()
        {
            Console.WriteLine(ToString());
            var instruction = CurrentInstruction;

            return instruction.Opcode switch
            {
                "hlf" => Hlf(instruction.Register.Value),
                "tpl" => Tpl(instruction.Register.Value),
                "inc" => Inc(instruction.Register.Value),
                "jmp" => Jmp(instruction.Offset.Value),
                "jie" => Jie(instruction.Register.Value, instruction.Offset.Value),
                "jio" => Jio(instruction.Register.Value, instruction.Offset.Value),
                _ => throw new ArgumentException($"Invalid opcode: {instruction.Opcode}", nameof(instruction.Opcode))
            };
        }

        public Computer Execute()
        {
            var computer = this;

            while (!computer.IsHalted)
            {
                computer = computer.Step();
            }
            return computer;
        }

        private Computer Hlf(char register) => ArithmeticOperation(register, x => x / 2);

        private Computer Tpl(char register) => ArithmeticOperation(register, x => x * 3);

        private Computer Inc(char register) => ArithmeticOperation(register, x => x + 1);

        private Computer Jmp(int offset) => this with { InstructionPointer = (uint) (InstructionPointer + offset)};

        private Computer Jie(char register, int offset) => Jump(register, x => x % 2 == 0, offset);

        private Computer Jio(char register, int offset) => Jump(register, x => x == 1, offset);

        private Computer ArithmeticOperation(char register, Func<uint, uint> operation)
        {
            return this with
            {
                A = register == 'a' ? operation(A) : A,
                B = register == 'b' ? operation(B) : B,
                InstructionPointer = InstructionPointer + 1
            };
        }

        private Computer Jump(char register, Func<uint, bool> shouldJump, int offset)
        {
            var registerValue = register == 'a' ? A : B;
            var willJump = shouldJump(registerValue);
            return this with { InstructionPointer = willJump ? (uint) (InstructionPointer + offset) : InstructionPointer + 1 };
        }

        public override string ToString()
        {
            var nextInstruction = IsHalted ? "" : CurrentInstruction.ToString();
            return $"IP: {InstructionPointer}, A: {A}, B: {B}, Next: {nextInstruction}";
        }
    }

    [GeneratedRegex(@"^(?<opcode>[a-z]{3}).?((?<register>[ab]),?)?.*?(?<offset>[+-]\d+)?$")]
    private static partial Regex InstructionRegex();
}