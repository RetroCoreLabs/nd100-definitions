using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NDGen.Core;
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
        _out.WriteLine($"Category docs loaded (.md):  {loader.CategoryDocs.Count}");
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
