using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;

public partial class Program
{
    private const string HitPoints = "Hit Points";
    private const string Damage = "Damage";
    private const string Armor = "Armor";

    public record Spell(string Name, int Mana, int Damage, int Heal, int Armor, int DamagePerTurn, int ManaPerTurn, int ShieldTurns, int PoisonTurns, int RechargeTurns);

    public static readonly Spell MagicMissile = new Spell("Magic Missile", 53,  4, 0, 0, 0,   0, 0, 0, 0);
    public static readonly Spell Drain =        new Spell("Drain",         73,  2, 2, 0, 0,   0, 0, 0, 0);
    public static readonly Spell Shield =       new Spell("Shield",        113, 0, 0, 7, 0,   0, 6, 0, 0);
    public static readonly Spell Poison =       new Spell("Poison",        173, 0, 0, 0, 3,   0, 0, 6, 0);
    public static readonly Spell Recharge =     new Spell("Recharge",      229, 0, 0, 0, 0, 101, 0, 0, 5);

    public static readonly ImmutableArray<Spell> Spells = ImmutableArray.Create(MagicMissile, Drain, Shield, Poison, Recharge);

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
        var boss = await ParseBoss(input);

        var task = part switch
        {
            1 => Part1(boss),
            2 => Part2(boss),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(Boss boss)
    {
        var player = new Player(50, 500, 0, 0, 0, 0);
        var minMana = Fight(player, boss); 
        Console.WriteLine(minMana);

        // Console.WriteLine(minManaUsed);
        return Task.CompletedTask;
    }

    public static Task Part2(Boss boss)
    {
        return Task.CompletedTask;
    }

    public static int Fight(Player player, Boss boss)
    {
        var minManaUsed = int.MaxValue;
        var stack = new Stack<World>();

        stack.Push(new World(player, boss));

        while (stack.Count > 0)
        {
            var world = stack.Pop();

            // Player turn
            world = world.ApplyEffects();
            if (world.IsPlayerWin)
            {
                minManaUsed = Math.Min(minManaUsed, world.Player.ManaUsed);
                continue;
            }

            var possibleSpells = world.Player.PossibleSpells();
            if (!possibleSpells.Any())
            {
                continue;
            }

            foreach(var spell in possibleSpells)
            {
                if (world.Player.ManaUsed + spell.Mana > minManaUsed)
                {
                    continue;
                }
                var newWorld = world.Cast(spell);
                if (newWorld.IsPlayerWin)
                {
                    minManaUsed = Math.Min(minManaUsed, newWorld.Player.ManaUsed);
                    continue;
                }

                // Boss turn
                newWorld = newWorld.ApplyEffects();
                if (newWorld.IsPlayerWin)
                {
                    minManaUsed = Math.Min(minManaUsed, newWorld.Player.ManaUsed);
                    continue;
                }

                newWorld = newWorld.Attack();
                if (newWorld.IsBossWin)
                {
                    continue;
                }

                stack.Push(newWorld);
            }
        }
        return minManaUsed;
    }    


    private static async Task<Boss> ParseBoss(string input)
    {
        var propertyRegex = PropertyRegex();
        
        var properties = await File.ReadLinesAsync(input)
            .Select(line => propertyRegex.Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups)
            .ToDictionaryAsync(
                groups => groups["property"].Value, 
                groups => Convert.ToInt32(groups["value"].Value));

        var hitPoints = properties.GetValueOrDefault(HitPoints);
        var damage = properties.GetValueOrDefault(Damage);
        var armor = properties.GetValueOrDefault(Armor);

        return new Boss(hitPoints, damage, armor);
    }
    
    [GeneratedRegex(@"^(?<property>.+): (?<value>\d+)$")]
    private static partial Regex PropertyRegex();


    public record World(Player Player, Boss Boss)
    {
        public World Cast(Spell spell)
        {
            if (Player.Mana < spell.Mana)
            {
                throw new InvalidOperationException("Not enough mana");
            }

            if (spell.ShieldTurns > 0 && Player.ShieldTurns > 0)
            {
                throw new InvalidOperationException("Shield already active");
            }

            if (spell.PoisonTurns > 0 && Player.PoisonTurns > 0)
            {
                throw new InvalidOperationException("Poison already active");
            }

            if (spell.RechargeTurns > 0 && Player.RechargeTurns > 0)
            {
                throw new InvalidOperationException("Recharge already active");
            }

            var player = Player with
            {
                HitPoints = Player.HitPoints + spell.Heal,
                Mana = Player.Mana - spell.Mana,
                ManaUsed = Player.ManaUsed + spell.Mana,
                ShieldTurns = Player.ShieldTurns + spell.ShieldTurns,
                PoisonTurns = Player.PoisonTurns + spell.PoisonTurns,
                RechargeTurns = Player.RechargeTurns + spell.RechargeTurns,
            };
            var boss = Boss with { HitPoints = Boss.HitPoints - spell.Damage };

            return new World(player, boss);
        }

        public World Attack()
        {
            var damage = Math.Max(1, Boss.Damage - Player.Armor);
            var player = Player with { HitPoints = Player.HitPoints - damage };
            return new World(player, Boss);
        }

        public World ApplyEffects()
        {
            var boss = Boss;
            var player = Player;

            var activeSpells = player.ActiveSpells();

            boss = boss with { HitPoints = boss.HitPoints - activeSpells.Sum(s => s.DamagePerTurn) };
            player = player with 
            {
                Mana = player.Mana + activeSpells.Sum(s => s.ManaPerTurn),
                ShieldTurns = player.ShieldTurns > 0 ? player.ShieldTurns - 1 : 0,
                PoisonTurns = player.PoisonTurns > 0 ? player.PoisonTurns - 1 : 0,
                RechargeTurns = player.RechargeTurns > 0 ? player.RechargeTurns - 1 : 0
            };

            return new World(player, boss);
        }

        public bool IsPlayerWin => Boss.IsDead;

        public bool IsBossWin => Player.IsDead || Player.Mana < Spells.Select(s => s.Mana).Min();

        public void Print()
        {
            Console.WriteLine($"Player has {Player.HitPoints} hit points, {Player.Armor} armor, {Player.Mana} mana");
            Console.WriteLine($"Boss has {Boss.HitPoints} hit points");
        }
    }

    public record Player(int HitPoints, int Mana, int ShieldTurns, int PoisonTurns, int RechargeTurns, int ManaUsed)
    {
        public int Armor => ShieldTurns > 0 ? 7 : 0;

        public bool CanCast(Spell spell)
        {
            return Mana >= spell.Mana
                && (spell.ShieldTurns == 0 || ShieldTurns == 0)
                && (spell.PoisonTurns == 0 || PoisonTurns == 0)
                && (spell.RechargeTurns == 0 || RechargeTurns == 0);
        }

        public IEnumerable<Spell> PossibleSpells() => Spells.Where(CanCast);

        public IEnumerable<Spell> ActiveSpells()
        {
            if (ShieldTurns > 0)
            {
                yield return Shield;
            }

            if (PoisonTurns > 0)
            {
                yield return Poison;
            }

            if (RechargeTurns > 0)
            {
                yield return Recharge;
            }
        }

        public bool IsDead => HitPoints <= 0;
    }

    public record Boss(int HitPoints, int Damage, int Armor)
    {
        public bool IsDead => HitPoints <= 0;
    };

}