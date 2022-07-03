using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using TimelessEmulator.Data;
using TimelessEmulator.Data.Models;
using TimelessEmulator.Game;
using System.Diagnostics;

using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Collections.Concurrent;

namespace TimelessEmulator;

public static class Program
{

    static Program()
    {

    }

    static readonly Stopwatch timer = new Stopwatch();

    public static void Main(string[] arguments)
    {
        Console.Title = $"{Settings.ApplicationName} Ver. {Settings.ApplicationVersion}";
        if (arguments.Length == 0)
        {
            AnsiConsole.Profile.Width = 220;
            AnsiConsole.MarkupLine("Hello, [green]exile[/]!");
            AnsiConsole.MarkupLine("Loading [green]data files[/]...");
        }

        if (!DataManager.Initialize())
            ExitWithError("Failed to initialize the [yellow]data manager[/].");

        var timelessJewelTypes = new List<string>
        {
            "Glorious Vanity",
            "Lethal Pride",
            "Brutal Restraint",
            "Militant Faith",
            "Elegant Hubris",
        };

        Dictionary<uint, Dictionary<string, TimelessJewelConqueror>> timelessJewelConquerors = new Dictionary<uint, Dictionary<string, TimelessJewelConqueror>>()
        {
            {
                1, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Xibaqua", new TimelessJewelConqueror(1, 0, "Xibaqua") },
                    { "[springgreen3]Zerphi (Legacy)[/]", new TimelessJewelConqueror(2, 0, "[springgreen3]Zerphi (Legacy)[/]") },
                    { "Ahuana", new TimelessJewelConqueror(2, 1,"Ahuana") },
                    { "Doryani", new TimelessJewelConqueror(3, 0,"Doryani") }
                }
            },
            {
                2, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Kaom", new TimelessJewelConqueror(1, 0,"Kaom") },
                    { "Rakiata", new TimelessJewelConqueror(2, 0,"Rakiata") },
                    { "[springgreen3]Kiloava (Legacy)[/]", new TimelessJewelConqueror(3, 0,"[springgreen3]Kiloava (Legacy)[/]") },
                    { "Akoya", new TimelessJewelConqueror(3, 1,"Akoya") }
                }
            },
            {
                3, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Deshret (Legacy)[/]", new TimelessJewelConqueror(1, 0,"[springgreen3]Deshret (Legacy)[/]") },
                    { "Balbala", new TimelessJewelConqueror(1, 1,"Balbala") },
                    { "Asenath", new TimelessJewelConqueror(2, 0,"Asenath") },
                    { "Nasima", new TimelessJewelConqueror(3, 0,"Nasima") }
                }
            },
            {
                4, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Venarius (Legacy)[/]", new TimelessJewelConqueror(1, 0,"[springgreen3]Venarius (Legacy)[/]") },
                    { "Maxarius", new TimelessJewelConqueror(1, 1,"Maxarius") },
                    { "Dominus", new TimelessJewelConqueror(2, 0,"Dominus") },
                    { "Avarius", new TimelessJewelConqueror(3, 0,"Avarius") }
                }
            },
            {
                5, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Cadiro", new TimelessJewelConqueror(1, 0,"Cadiro") },
                    { "Victario", new TimelessJewelConqueror(2, 0,"Victario") },
                    { "[springgreen3]Chitus (Legacy)[/]", new TimelessJewelConqueror(3, 0,"[springgreen3]Chitus (Legacy)[/]") },
                    { "Caspiro", new TimelessJewelConqueror(3, 1,"Caspiro") }
                }
            }
        };

        Dictionary<uint, (uint minimumSeed, uint maximumSeed)> timelessJewelSeedRanges = new Dictionary<uint, (uint minimumSeed, uint maximumSeed)>()
        {
            { 1, (100, 8000) },
            { 2, (10000, 18000) },
            { 3, (500, 8000) },
            { 4, (2000, 10000) },
            { 5, (2000, 160000) }
        };

        var options = new JsonSerializerOptions { WriteIndented = false };
        timer.Start();

        var resultPath = Path.Combine("D:\\timeless", "result");

        for (int i = 0; i < timelessJewelTypes.Count; i++)
        {
            var timelessJewelType = timelessJewelTypes[(int)i];
            var conquerors = timelessJewelConquerors[(uint)i + 1];
            var seedParams = timelessJewelSeedRanges[(uint)i + 1];
            foreach (var conqueror in conquerors)
            {
                for (uint j = seedParams.minimumSeed; j < seedParams.maximumSeed; j++)
                {
                    var allPassivesWithInfo = new ConcurrentDictionary<string, object>();
                    Parallel.ForEach(DataManager.PassiveSkillsInRadiusOfAnyJewel, passiveInRadius =>
                    {
                        var passive = DataManager.GetPassiveSkillByFuzzyValue(passiveInRadius.Identifier);
                        TimelessJewel jewel = GetTimelessJewelFromInput(timelessJewelType, conqueror.Key, j.ToString());
                        Dictionary<string, object> passiveInfo = GeneratePassiveSKillInfo(passive, jewel);
                        allPassivesWithInfo.TryAdd(passive.Identifier, passiveInfo);

                    });
                    var s = JsonSerializer.Serialize(allPassivesWithInfo, options);
                    string v = Path.Combine(resultPath, timelessJewelType, conqueror.Key);
                    Directory.CreateDirectory(v);
                    File.WriteAllTextAsync(Path.Combine(v, $"{j}.json"), s);
                    AnsiConsole.WriteLine($"Iteration {timelessJewelType}, {conqueror.Key}, {j} - done");
                }
            }
        }
        //allPassivesWithInfo.Add(passive.Identifier, fin);
        //break;

        timer.Stop();
        AnsiConsole.WriteLine("Iteration took - {0}", timer.ElapsedMilliseconds);

        WaitForExit();

        PassiveSkill passiveSkill = GetPassiveSkillFromInput(arguments.Length >= 1 ? arguments[0] : null);

        if (passiveSkill == null)
            ExitWithError("Failed to get the [yellow]passive skill[/] from input.");

        TimelessJewel timelessJewel = GetTimelessJewelFromInput(
            arguments.Length >= 2 ? arguments[1] : null,
            arguments.Length >= 3 ? arguments[2] : null,
            arguments.Length >= 4 ? arguments[3] : null
            );

        if (timelessJewel == null)
            ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");

        AnsiConsole.WriteLine();

        GeneratePassiveSKillInfo(passiveSkill, timelessJewel);

        if (arguments.Length == 0)
        {
            WaitForExit();
        }
        else
        {
            Environment.Exit(0);
        }
    }

    private static Dictionary<string, Object> GeneratePassiveSKillInfo(PassiveSkill passiveSkill, TimelessJewel timelessJewel)
    {
        //AnsiConsole.MarkupLine($"PassiveSkill: {passiveSkill.Name}; TimelessJewel: {timelessJewel}");
        AlternateTreeManager alternateTreeManager = new AlternateTreeManager(passiveSkill, timelessJewel);

        bool isPassiveSkillReplaced = alternateTreeManager.IsPassiveSkillReplaced();
        var dic = new Dictionary<string, Object>();

        //AnsiConsole.MarkupLine($"[green]Is Passive Skill Replaced[/]: {isPassiveSkillReplaced};");
        dic.Add("replaced", isPassiveSkillReplaced.ToString());

        if (isPassiveSkillReplaced)
        {
            AlternatePassiveSkillInformation alternatePassiveSkillInformation = alternateTreeManager.ReplacePassiveSkill();

            //AnsiConsole.MarkupLine($"[green]Alternate Passive Skill[/]: [yellow]{alternatePassiveSkillInformation.AlternatePassiveSkill.Name}[/] ([yellow]{alternatePassiveSkillInformation.AlternatePassiveSkill.Identifier}[/]);");
            dic.Add("AlternatePassiveSkillName", alternatePassiveSkillInformation.AlternatePassiveSkill.Name);
            dic.Add("AlternatePassiveSkillIdentifier", alternatePassiveSkillInformation.AlternatePassiveSkill.Identifier);

            var stats = new List<object>();
            for (int i = 0; i < alternatePassiveSkillInformation.AlternatePassiveSkill.StatIndices.Count; i++)
            {
                var stat = new Dictionary<string, Object>();
                uint statIndex = alternatePassiveSkillInformation.AlternatePassiveSkill.StatIndices.ElementAt(i);
                uint statRoll = alternatePassiveSkillInformation.StatRolls.ElementAt(i).Value;

                //AnsiConsole.MarkupLine($"\t\tStat [yellow]{i}[/] | [yellow]{DataManager.GetStatTextByIndex(statIndex)}[/] (Identifier: [yellow]{DataManager.GetStatIdentifierByIndex(statIndex)}[/], Index: [yellow]{statIndex}[/]), Roll: [yellow]{statRoll}[/];");
                stat.Add("text", DataManager.GetStatTextByIndex(statIndex));
                stat.Add("identifier", DataManager.GetStatIdentifierByIndex(statIndex));
                stat.Add("index", statIndex);
                stat.Add("roll", statRoll);
                stats.Add(stat);
            }
            dic.Add("stats", stats);

            var adds = PrintAlternatePassiveAdditionInformations(alternatePassiveSkillInformation.AlternatePassiveAdditionInformations);
            dic.Add("additions", adds);
        }
        else
        {
            IReadOnlyCollection<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations = alternateTreeManager.AugmentPassiveSkill();

            var adds = PrintAlternatePassiveAdditionInformations(alternatePassiveAdditionInformations);
            dic.Add("additions", adds);
        }
        return dic;
    }

    private static PassiveSkill GetPassiveSkillFromInput(string arg = null)
    {
        TextPrompt<string> passiveSkillTextPrompt = new TextPrompt<string>("[green]Passive Skill[/]:")
            .Validate((string input) =>
            {
                PassiveSkill passiveSkill = DataManager.GetPassiveSkillByFuzzyValue(input);

                if (passiveSkill == null)
                    return ValidationResult.Error($"[red]Error[/]: Unable to find [yellow]passive skill[/] `{input}`.");

                if (!DataManager.IsPassiveSkillValidForAlteration(passiveSkill))
                    return ValidationResult.Error($"[red]Error[/]: The [yellow]passive skill[/] `{input}` is not valid for alteration.");

                return ValidationResult.Success();
            });

        string passiveSkillInput;

        if (arg != null)
        {
            passiveSkillInput = arg;
        }
        else
        {
            passiveSkillInput = AnsiConsole.Prompt(passiveSkillTextPrompt);
        }

        PassiveSkill passiveSkill = DataManager.GetPassiveSkillByFuzzyValue(passiveSkillInput);

        if (arg == null)
        {
            AnsiConsole.MarkupLine($"[green]Found Passive Skill[/]: [yellow]{passiveSkill.Name}[/] ([yellow]{passiveSkill.Identifier}[/])");
        }

        return passiveSkill;
    }

    private static TimelessJewel GetTimelessJewelFromInput(string arg1 = null, string arg2 = null, string arg3 = null)
    {
        Dictionary<uint, string> timelessJewelTypes = new Dictionary<uint, string>()
        {
            { 1, "Glorious Vanity" },
            { 2, "Lethal Pride" },
            { 3, "Brutal Restraint" },
            { 4, "Militant Faith" },
            { 5, "Elegant Hubris" }
        };

        Dictionary<uint, Dictionary<string, TimelessJewelConqueror>> timelessJewelConquerors = new Dictionary<uint, Dictionary<string, TimelessJewelConqueror>>()
        {
            {
                1, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Xibaqua", new TimelessJewelConqueror(1, 0, "Xibaqua") },
                    { "[springgreen3]Zerphi (Legacy)[/]", new TimelessJewelConqueror(2, 0, "[springgreen3]Zerphi (Legacy)[/]") },
                    { "Ahuana", new TimelessJewelConqueror(2, 1,"Ahuana") },
                    { "Doryani", new TimelessJewelConqueror(3, 0,"Doryani") }
                }
            },
            {
                2, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Kaom", new TimelessJewelConqueror(1, 0,"Kaom") },
                    { "Rakiata", new TimelessJewelConqueror(2, 0,"Rakiata") },
                    { "[springgreen3]Kiloava (Legacy)[/]", new TimelessJewelConqueror(3, 0,"[springgreen3]Kiloava (Legacy)[/]") },
                    { "Akoya", new TimelessJewelConqueror(3, 1,"Akoya") }
                }
            },
            {
                3, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Deshret (Legacy)[/]", new TimelessJewelConqueror(1, 0,"[springgreen3]Deshret (Legacy)[/]") },
                    { "Balbala", new TimelessJewelConqueror(1, 1,"Balbala") },
                    { "Asenath", new TimelessJewelConqueror(2, 0,"Asenath") },
                    { "Nasima", new TimelessJewelConqueror(3, 0,"Nasima") }
                }
            },
            {
                4, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Venarius (Legacy)[/]", new TimelessJewelConqueror(1, 0,"[springgreen3]Venarius (Legacy)[/]") },
                    { "Maxarius", new TimelessJewelConqueror(1, 1,"Maxarius") },
                    { "Dominus", new TimelessJewelConqueror(2, 0,"Dominus") },
                    { "Avarius", new TimelessJewelConqueror(3, 0,"Avarius") }
                }
            },
            {
                5, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Cadiro", new TimelessJewelConqueror(1, 0,"Cadiro") },
                    { "Victario", new TimelessJewelConqueror(2, 0,"Victario") },
                    { "[springgreen3]Chitus (Legacy)[/]", new TimelessJewelConqueror(3, 0,"[springgreen3]Chitus (Legacy)[/]") },
                    { "Caspiro", new TimelessJewelConqueror(3, 1,"Caspiro") }
                }
            }
        };

        Dictionary<uint, (uint minimumSeed, uint maximumSeed)> timelessJewelSeedRanges = new Dictionary<uint, (uint minimumSeed, uint maximumSeed)>()
        {
            { 1, (100, 8000) },
            { 2, (10000, 18000) },
            { 3, (500, 8000) },
            { 4, (2000, 10000) },
            { 5, (2000, 160000) }
        };

        SelectionPrompt<string> timelessJewelTypeSelectionPrompt = new SelectionPrompt<string>()
            .Title("[green]Timeless Jewel Type[/]:")
            .AddChoices(timelessJewelTypes.Values.ToArray());

        string timelessJewelTypeInput;

        if (arg1 != null)
        {
            timelessJewelTypeInput = arg1;
        }
        else
        {
            timelessJewelTypeInput = AnsiConsole.Prompt(timelessJewelTypeSelectionPrompt);

            //AnsiConsole.MarkupLine($"[green]Timeless Jewel Type[/]: {timelessJewelTypeInput}");
        }



        uint alternateTreeVersionIndex = timelessJewelTypes
            .First(q => (q.Value == timelessJewelTypeInput))
            .Key;

        AlternateTreeVersion alternateTreeVersion = DataManager.AlternateTreeVersions
            .First(q => (q.Index == alternateTreeVersionIndex));

        SelectionPrompt<string> timelessJewelConquerorSelectionPrompt = new SelectionPrompt<string>()
            .Title("[green] Timeless Jewel Conqueror[/]:")
            .AddChoices(timelessJewelConquerors[alternateTreeVersionIndex].Keys.ToArray());

        string timelessJewelConquerorInput;
        if (arg2 != null)
        {
            timelessJewelConquerorInput = arg2;
        }
        else
        {
            timelessJewelConquerorInput = AnsiConsole.Prompt(timelessJewelConquerorSelectionPrompt);
            //AnsiConsole.MarkupLine($"[green]Timeless Jewel Conqueror[/]: {timelessJewelConquerorInput}");
        }


        TimelessJewelConqueror timelessJewelConqueror = timelessJewelConquerors[alternateTreeVersionIndex]
            .First(q => (q.Key == timelessJewelConquerorInput))
            .Value;

        TextPrompt<uint> timelessJewelSeedTextPrompt = new TextPrompt<uint>($"[green]Timeless Jewel Seed ({timelessJewelSeedRanges[alternateTreeVersionIndex].minimumSeed} - {timelessJewelSeedRanges[alternateTreeVersionIndex].maximumSeed})[/]:")
            .Validate((uint input) =>
            {
                if ((input >= timelessJewelSeedRanges[alternateTreeVersionIndex].minimumSeed) &&
                    (input <= timelessJewelSeedRanges[alternateTreeVersionIndex].maximumSeed))
                {
                    return ValidationResult.Success();
                }

                return ValidationResult.Error($"[red]Error[/]: The [yellow]timeless jewel seed[/] must be between {timelessJewelSeedRanges[alternateTreeVersionIndex].minimumSeed} and {timelessJewelSeedRanges[alternateTreeVersionIndex].maximumSeed}.");
            });

        uint timelessJewelSeed;
        if (arg3 != null)
        {
            timelessJewelSeed = UInt32.Parse(arg3);
        }
        else
        {
            timelessJewelSeed = AnsiConsole.Prompt(timelessJewelSeedTextPrompt);
        }

        return new TimelessJewel(timelessJewelTypeInput, alternateTreeVersion, timelessJewelConqueror, timelessJewelSeed);
    }

    private static List<object> PrintAlternatePassiveAdditionInformations(IReadOnlyCollection<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations)
    {
        ArgumentNullException.ThrowIfNull(alternatePassiveAdditionInformations, nameof(alternatePassiveAdditionInformations));

        var l = new List<object>();
        foreach (AlternatePassiveAdditionInformation alternatePassiveAdditionInformation in alternatePassiveAdditionInformations)
        {
            var dic = new Dictionary<string, Object>();
            //AnsiConsole.MarkupLine($"\t[green]Addition[/]: [yellow]{alternatePassiveAdditionInformation.AlternatePassiveAddition.Identifier}[/];");
            var stats = new Dictionary<string, Object>();
            for (int i = 0; i < alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.Count; i++)
            {
                uint statIndex = alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.ElementAt(i);
                uint statRoll = alternatePassiveAdditionInformation.StatRolls.ElementAt(i).Value;

                //AnsiConsole.MarkupLine($"\t\tStat [yellow]{i}[/] | [yellow]{DataManager.GetStatTextByIndex(statIndex)}[/] (Identifier: [yellow]{DataManager.GetStatIdentifierByIndex(statIndex)}[/], Index: [yellow]{statIndex}[/]), Roll: [yellow]{statRoll}[/];");
                stats.Add("text", DataManager.GetStatTextByIndex(statIndex));
                stats.Add("identifier", DataManager.GetStatIdentifierByIndex(statIndex));
                stats.Add("index", statIndex);
                stats.Add("roll", statRoll);
            }
            dic.Add(alternatePassiveAdditionInformation.AlternatePassiveAddition.Identifier, stats);
            l.Add(dic);
        }
        return l;
    }

    private static void WaitForExit()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Press [yellow]any key[/] to exit.");

        try
        {
            Console.ReadKey();
        }
        catch { }

        Environment.Exit(0);
    }

    private static void PrintError(string error)
    {
        AnsiConsole.MarkupLine($"[red]Error[/]: {error}");
    }

    private static void ExitWithError(string error)
    {
        PrintError(error);
        WaitForExit();
    }

}
