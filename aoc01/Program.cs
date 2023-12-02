using System.CommandLine;
using System.ComponentModel;

public class Program
{
    public static async Task Main(string[] args)
    {
        var inputOption = new Option<string>(
            name: "--input",
            description: "Input file path",
            getDefaultValue: () => "input.txt");

        var rootCommand = new RootCommand();
        rootCommand.AddOption(inputOption);

        rootCommand.SetHandler(Run, inputOption);

        await rootCommand.InvokeAsync(args);
    }

    private static Task Run(string input)
    {
        using var fs = File.OpenRead(input);

        int c;
        var floor = 0;
        do
        {
            c = fs.ReadByte();
            switch (c)
            {
                case '(':
                    floor++;
                    break;
                case ')':
                    floor--;
                    break;
            }
        } while (c != -1);

        Console.WriteLine($"Floor: {floor}");
        return Task.CompletedTask;
    }
}