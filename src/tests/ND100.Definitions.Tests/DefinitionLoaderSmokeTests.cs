using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NDGen.Core;
using NDGen.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ND100.Definitions.Tests;

public class DefinitionLoaderSmokeTests
{
    private readonly ITestOutputHelper _out;

    public DefinitionLoaderSmokeTests(ITestOutputHelper output) => _out = output;

    private static string FindCpuYamlPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "specs", "cpu.yaml");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        var env = Environment.GetEnvironmentVariable("ND100_SPECS_CPU_YAML");
        if (!string.IsNullOrEmpty(env) && File.Exists(env))
            return env;

        throw new FileNotFoundException(
            "Could not find specs/cpu.yaml (walk up from test bin) and ND100_SPECS_CPU_YAML is not set.");
    }

    private static int CountInstructionYamlFilesOnDisk(string specsRoot, string instructionsRelativeDir)
    {
        var instructionsDir = Path.Combine(specsRoot, instructionsRelativeDir);
        if (!Directory.Exists(instructionsDir))
            return 0;

        return Directory.GetFiles(instructionsDir, "*.yaml", SearchOption.AllDirectories)
            .Count(f =>
                !f.EndsWith("_template.yaml", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(Path.GetFileName(f), "cpu.yaml", StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<InstructionDefinition> LoadInstructions()
    {
        using var loggerFactory = LoggerFactory.Create(b => { });
        var loader = new DefinitionLoader(loggerFactory.CreateLogger<DefinitionLoader>(),
            new YamlLoader(loggerFactory.CreateLogger<YamlLoader>()));
        loader.Initialize(FindCpuYamlPath());
        return loader.GetInstructions().ToList();
    }

    private static InstructionDefinition Get(IReadOnlyList<InstructionDefinition> all, string name)
    {
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].Name == name)
                return all[i];
        }

        throw new Xunit.Sdk.XunitException("instruction not loaded: " + name);
    }

    /// <summary>
    /// Every instruction must say where execution continues afterwards (field "exits"),
    /// and every exit must be well formed (see specs/schema/instruction_schema.yaml).
    /// </summary>
    [Fact]
    public void Every_instruction_has_valid_exits()
    {
        var all = LoadInstructions();
        string[] whens = { "always", "ok", "error", "taken", "not_taken" };
        string[] targets = { "effective_address", "displacement", "result", "register",
            "stack_link", "stack_link_minus_1", "executed_instruction" };

        for (int i = 0; i < all.Count; i++)
        {
            var ins = all[i];
            Assert.True(ins.Exits.Count > 0, ins.Name + ": no exits");
            Assert.False(string.IsNullOrWhiteSpace(ins.ExitsSource), ins.Name + ": no exits_source");
            Assert.True(ins.InlineWords >= 0, ins.Name + ": inline_words < 0");
            for (int e = 0; e < ins.Exits.Count; e++)
            {
                var x = ins.Exits[e];
                Assert.True(x.Offset.HasValue != (x.Target != null), ins.Name + ": exit needs exactly one of offset/target");
                Assert.True(Array.IndexOf(whens, x.When) >= 0, ins.Name + ": bad when '" + x.When + "'");
                if (x.Target != null)
                    Assert.True(Array.IndexOf(targets, x.Target) >= 0, ins.Name + ": bad target '" + x.Target + "'");
                if (x.Offset.HasValue)
                    Assert.True(x.Offset.Value > ins.InlineWords, ins.Name + ": exit lands inside the inline words");
            }
        }
    }

    /// <summary>
    /// The skip returns read from nd100x (bcd.c / cpu_instr.c): error return and normal return.
    /// </summary>
    [Theory]
    [InlineData("ADDD", 0, 1, 2)]
    [InlineData("SUBD", 0, 1, 2)]
    [InlineData("COMD", 0, 1, 2)]
    [InlineData("SHDE", 0, 1, 2)]
    [InlineData("PACK", 0, 1, 2)]
    [InlineData("UPACK", 0, 1, 2)]
    [InlineData("MOVBF", 0, 1, 2)]
    [InlineData("ENTR", 1, 2, 3)]
    [InlineData("INIT", 5, 6, 7)]
    public void Error_and_ok_exits(string name, int inlineWords, int errorOffset, int okOffset)
    {
        var ins = Get(LoadInstructions(), name);
        Assert.Equal(inlineWords, ins.InlineWords);
        Assert.Equal(2, ins.Exits.Count);
        Assert.Equal("error", ins.Exits[0].When);
        Assert.Equal(errorOffset, ins.Exits[0].Offset);
        Assert.Equal("ok", ins.Exits[1].When);
        Assert.Equal(okOffset, ins.Exits[1].Offset);
    }

    [Fact]
    public void Initialize_loads_cpu_and_instructions()
    {
        var cpuPath = FindCpuYamlPath();
        using var loggerFactory = LoggerFactory.Create(b => { });
        var yamlLogger = loggerFactory.CreateLogger<YamlLoader>();
        var defLogger = loggerFactory.CreateLogger<DefinitionLoader>();

        var yaml = new YamlLoader(yamlLogger);
        var loader = new DefinitionLoader(defLogger, yaml);
        loader.Initialize(cpuPath);

        var cpu = loader.GetCpuDefinition();
        var meta = cpu.Metadata;
        var instructions = loader.GetInstructions().ToList();

        var specsRoot = Path.GetDirectoryName(cpuPath)!;
        var instructionsRel = meta.Paths.GetValueOrDefault("instructions", "instructions");
        var yamlFilesOnDisk = CountInstructionYamlFilesOnDisk(specsRoot, instructionsRel);

        _out.WriteLine("=== ND100.Definitions load statistics ===");
        _out.WriteLine($"cpu.yaml path: {cpuPath}");
        _out.WriteLine($"specs root:    {specsRoot}");
        _out.WriteLine($"CPU name:      {meta.CpuName}");
        _out.WriteLine($"word_size:     {meta.WordSize}");
        _out.WriteLine($"endianness:    {meta.Endianness}");
        _out.WriteLine($"Registers (metadata):        {meta.Registers.Count}");
        _out.WriteLine($"Operand types (metadata):    {meta.OperandTypes.Count}");
        _out.WriteLine($"Instruction classes (meta):  {meta.InstructionClasses.Count}");
        _out.WriteLine($"Tokens (metadata):           {meta.Tokens.Count}");
        _out.WriteLine($"Category texts loaded:  {loader.CategoryDocs.Count}");
        _out.WriteLine($"Instruction dir (relative):  {instructionsRel}");
        _out.WriteLine($"Instruction .yaml on disk:   {yamlFilesOnDisk}  (excl. _template.yaml, cpu.yaml)");
        _out.WriteLine($"Instructions loaded:         {instructions.Count}");
        _out.WriteLine("-- By category --");
        foreach (var g in instructions.GroupBy(i => i.Category).OrderBy(g => g.Key))
        {
            var label = string.IsNullOrWhiteSpace(g.Key) ? "(empty)" : g.Key;
            _out.WriteLine($"  {label,-40} {g.Count(),4}");
        }

        _out.WriteLine("-- By instruction_class --");
        foreach (var g in instructions.GroupBy(i => i.InstructionClass).OrderByDescending(g => g.Count()).ThenBy(g => g.Key))
        {
            var label = string.IsNullOrWhiteSpace(g.Key) ? "(empty in YAML)" : g.Key;
            _out.WriteLine($"  {label,-40} {g.Count(),4}");
        }

        Assert.NotNull(meta);
        Assert.NotEmpty(instructions);
        Assert.Equal(yamlFilesOnDisk, instructions.Count);
    }
}
