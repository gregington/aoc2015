using System.Collections.Frozen;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

public partial class Program
{
    private const string HitPoints = "Hit Points";
    private const string Damage = "Damage";
    private const string Armor = "Armor";
    private const string Weapons = "Weapons";
    private const string Rings = "Rings";

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
        var equipment = await ParseEquipment("equipment.txt");
        var boss = await ParseBoss(input);

        var task = part switch
        {
            1 => Part1(equipment, boss),
            2 => Part2(),
            _ => throw new ArgumentException($"Invalid part: {part}", nameof(part))
        };

        await task;
    }

    public static Task Part1(FrozenDictionary<string, ImmutableArray<Equipment>> equipment, Player boss)
    {
        var playerHitPoints = 100;
        var equipmentChoices = EquipmentCombinations(equipment);

        var players = equipmentChoices
            .Select(equipment =>
                (Cost: equipment.Select(e => e.Cost).Sum(), 
                    Player: new Player(playerHitPoints, equipment.Select(e => e.Damage).Sum(), equipment.Select(e => e.Armor).Sum()),
                    Equipment: equipment))
            .OrderBy(x => x.Cost);

        var (cost, outcome, eq) = players.Select(x => (x.Cost, Outcome: Fight(x.Player, boss), x.Equipment))
            .First(x => x.Outcome);

        Console.WriteLine($"{cost} {string.Join(", ", eq.Select(e => e.Name))}");
            
        return Task.CompletedTask;
    }

    public static Task Part2()
    {
        return Task.CompletedTask;
    }

    private static bool Fight(Player player, Player boss)
    {
        var damageToBoss = Math.Max(player.Damage - boss.Armor, 1);
        var damageToPlayer = Math.Max(boss.Damage - player.Armor, 1);
        while (true)
        {
            // Player attacks first
            boss = boss with { HitPoints = boss.HitPoints - damageToBoss };
            if (boss.HitPoints <= 0) 
            {
                return true;
            }

            // Then boss
            player = player with { HitPoints = player.HitPoints - damageToPlayer };
            if (player.HitPoints <= 0)
            {
                return false;
            }
        }
    }

    private static IEnumerable<IEnumerable<Equipment>> EquipmentCombinations(FrozenDictionary<string, ImmutableArray<Equipment>> equipment)
    {
        var weaponList = WeaponCombinations(equipment[Weapons]);
        var armorList = ArmorCombinations(equipment[Armor]);
        var ringList = RingCombinations(equipment[Rings]);

        foreach (var weapons in weaponList)
        {
            foreach (var armors in armorList)
            {
                foreach (var rings in ringList)
                {
                    yield return weapons.Concat(armors).Concat(rings);
                }
            }
        }
    }

    private static IEnumerable<IEnumerable<Equipment>> WeaponCombinations(ImmutableArray<Equipment> weapons) =>
        weapons.Select(w => ImmutableArray.Create(w) as IEnumerable<Equipment>)
            .ToImmutableArray();

    private static IEnumerable<IEnumerable<Equipment>> ArmorCombinations(ImmutableArray<Equipment> armor) =>
        armor.Select(w => ImmutableArray.Create(w) as IEnumerable<Equipment>)
            .Append(Enumerable.Empty<Equipment>())
            .ToImmutableArray();

    private static IEnumerable<IEnumerable<Equipment>> RingCombinations(ImmutableArray<Equipment> rings)
    {
        yield return Enumerable.Empty<Equipment>();
        foreach (var ring in rings)
        {
            yield return ImmutableArray.Create(ring);
        }

        for (var i = 0; i < rings.Length; i++)
        {
            for (var j = 0; j < i; j++)
            {
                yield return ImmutableArray.Create(rings[j], rings[i]);
            }
        }
    }


    private static async Task<Player> ParseBoss(string input)
    {
        var propertyRegex = PropertyRegex();
        
        var properties = await File.ReadLinesAsync(input)
            .Select(line => propertyRegex.Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups)
            .ToDictionaryAsync(
                groups => groups["property"].Value, 
                groups => Convert.ToInt32(groups["value"].Value));

        return new Player(properties[HitPoints], properties[Damage], properties[Armor]);
    }
    
    private static async Task<FrozenDictionary<string, ImmutableArray<Equipment>>> ParseEquipment(string input)
    {
        var typeRegex = TypeRegex();
        var equipmentRegex = EquipmentRegex();

        var dict = new Dictionary<string, ImmutableArray<Equipment>>();
        var type = string.Empty;
        var equipment = new List<Equipment>();
        var lines = await File.ReadAllLinesAsync(input);

        foreach (var line in lines)
        {
            var match = typeRegex.Match(line);
            if (match.Success)
            {
                if (type == string.Empty)
                {
                    type = match.Groups["type"].Value;
                    continue;
                }
                dict[type] = [..equipment];
                type = match.Groups["type"].Value;
                equipment.Clear();
                continue;
            }

            match = equipmentRegex.Match(line);
            if (match.Success)
            {
                equipment.Add(new Equipment(
                    match.Groups["name"].Value,
                    Convert.ToInt32(match.Groups["cost"].Value),
                    Convert.ToInt32(match.Groups["damage"].Value),
                    Convert.ToInt32(match.Groups["armor"].Value)
                ));
            }
        }
        dict[type] = [..equipment];
        return dict.ToFrozenDictionary();
    }

    [GeneratedRegex(@"^(?<type>.*):.*$")]
    private static partial Regex TypeRegex();

    [GeneratedRegex(@"^(?<name>.*?) +(?<cost>\d+) +(?<damage>\d+) +(?<armor>\d+)$")]
    private static partial Regex EquipmentRegex();

    [GeneratedRegex(@"^(?<property>.+): (?<value>\d+)$")]
    private static partial Regex PropertyRegex();
}

public record Equipment(string Name, int Cost, int Damage, int Armor);

public record Player(int HitPoints, int Damage, int Armor);
