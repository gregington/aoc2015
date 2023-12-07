using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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
        var password = new Password(File.ReadAllLines(input)[0]);

        var task = part switch
        {
            1 => Part1(password),
            2 => Part2(password),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(Password password)
    {
        Console.WriteLine(password.NextValid());

        return Task.CompletedTask;
    }

    public static Task Part2(Password password)
    {
        Console.WriteLine(password.NextValid().NextValid());

        return Task.CompletedTask;
    }

    public class Password
    {
        private static char Overflow = (char) ('z' + 1);

        private readonly string password;

        public Password(string password)
        {
            this.password = password;
        }

        public override string ToString() => password;

        public Password Increment()
        {
            var chars = password.ToString().ToCharArray().Reverse().ToArray();

            chars[0]++;

            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == Overflow)
                {
                    chars[i] = 'a';
                    if (i != chars.Length - 1)
                    {
                        chars[i + 1]++;
                    }
                }
            }

            return new Password(new string(chars.Reverse().ToArray()));
        }

        public Password NextValid()
        {
            var p = this.Increment();
            while (true)
            {
                if (p.IsValid())
                {
                    return p;
                }
                p = p.Increment();
            }
        }

        public bool IsValid()
        {
            return HasStraightOf(3)
                && ContainValidCharacters()
                && ContainsAtLeastTwoParsOfDifferentLetters();
        }

        private bool HasStraightOf(int straightLength)
        {
            var substrings = Enumerable.Range(0, password.Length - straightLength)
                .Select(i => password.Substring(i, straightLength));

            return substrings.Any(p => Enumerable
                .Range(0, straightLength - 1)
                .Select(i => p[i+1] - p[i])
                .All(x => x == 1));
        }

        private bool ContainValidCharacters()
        {
            return !password.Any(c => c == 'i' || c == 'o' || c == 'l');
        }

        private bool ContainsAtLeastTwoParsOfDifferentLetters()
        {
            var counts = new List<int>();

            var c = '\0';
            var count = 0;
            for (var i = 0; i < password.Length; i++)
            {
                if (password[i] == c)
                {
                    count++;
                    continue;
                }

                if (count > 0) {
                    counts.Add(count);
                }

                c = password[i];
                count = 1;
            }
            counts.Add(count);

            return counts.Any(c => c >= 4)
                || counts.Where(c => c >=2).Count() >= 2;
        }

    }
}