using System.IO;
using Microsoft.Extensions.Logging;
using NDGen.Core;
using NDGen.Core.Models;
using Xunit;

namespace ND100.Definitions.Tests;

/// <summary>
/// The documentation data every tool reads from the definitions (the Ghidra plugin's PDF manual,
/// the documentation site, the Markdown generator): categories, manual references, the manual
/// list, the privilege wording, and examples. Each test fails on data that a tool would
/// otherwise drop without a word.
/// </summary>
public class DocumentationDataTests
{
    private static string CpuYaml()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "specs", "cpu.yaml");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Could not find specs/cpu.yaml above the test folder");
    }

    private static DefinitionLoader Load()
    {
        using var loggerFactory = LoggerFactory.Create(b => { });
        var loader = new DefinitionLoader(loggerFactory.CreateLogger<DefinitionLoader>(),
            new YamlLoader(loggerFactory.CreateLogger<YamlLoader>()));
        loader.Initialize(CpuYaml());
        return loader;
    }

    private static InstructionDefinition Find(DefinitionLoader loader, string name)
    {
        var all = loader.GetInstructions();
        foreach (var i in all)
            if (i.Name == name)
                return i;
        throw new Xunit.Sdk.XunitException($"{name} not loaded");
    }

    [Fact]
    public void Categories_come_from_categories_yaml_and_cover_every_instruction()
    {
        var loader = Load();
        Assert.Equal(30, loader.Categories.Count);
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < loader.Categories.Count; i++)
            names.Add(loader.Categories[i].Name);
        foreach (var ins in loader.GetInstructions())
            Assert.True(names.Contains(ins.Category), $"{ins.Name}: '{ins.Category}' not in categories.yaml");
        // The text the Markdown generator looks up by lower-case name.
        Assert.True(loader.CategoryDocs.ContainsKey("bit instructions"));
    }

    [Fact]
    public void Manual_references_and_the_manual_list_load()
    {
        var loader = Load();
        var sat = Find(loader, "SAT");
        Assert.Single(sat.ManualRefs);
        Assert.Equal("ND-06.029.1", sat.ManualRefs[0].Manual);
        Assert.Equal(85, sat.ManualRefs[0].Page);

        var manuals = loader.GetCpuDefinition().Metadata.Manuals;
        Assert.Equal(2, manuals.Count);
        Assert.Equal("ND-110 Instruction Set", manuals[1].Title);
    }

    [Fact]
    public void Privilege_wording_loads()
    {
        var levels = Load().GetCpuDefinition().Metadata.PrivilegeLevels;
        Assert.Equal(2, levels.Count);
        Assert.Equal("privileged", levels[1].Name);
        Assert.Contains("operating system", levels[1].Description);
    }

    [Fact]
    public void Every_file_with_examples_keeps_them_after_loading()
    {
        var loader = Load();
        var dir = Path.Combine(Path.GetDirectoryName(CpuYaml())!, "instructions");
        int checkedFiles = 0;
        foreach (var f in Directory.GetFiles(dir, "*.yaml"))
        {
            if (Path.GetFileName(f).StartsWith("_")) continue;
            var text = File.ReadAllText(f);
            if (!text.Contains("\n  examples:")) continue;
            var nameLine = text.Substring(text.IndexOf("name:", StringComparison.Ordinal) + 5);
            var name = nameLine.Substring(0, nameLine.IndexOf('\n')).Trim().Trim('"');
            Assert.NotEmpty(Find(loader, name).Examples);
            checkedFiles++;
        }
        Assert.True(checkedFiles > 30, $"only {checkedFiles} files with examples found");
    }
}
