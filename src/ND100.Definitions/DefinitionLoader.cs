using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using NDGen.Core.Models;
using NDGen.Core.Utils;
using Microsoft.Extensions.Logging;
using System.Text;

namespace NDGen.Core
{
    public class DefinitionLoader
    {
        private readonly ILogger<DefinitionLoader> _logger;
        private readonly YamlLoader _yamlLoader;
        private CpuDefinition _cpuDefinition = null!;
        private List<InstructionDefinition> _instructions;
        private bool _isInitialized;
        private Dictionary<string, string> _categoryDocs = new();

        private List<CategoryDefinition> _categories = new();

        /// <summary>Category text keyed by the category name in lower case (from specs/categories.yaml).</summary>
        public IReadOnlyDictionary<string, string> CategoryDocs => _categoryDocs;

        /// <summary>Every category in specs/categories.yaml, in the order of the manuals.</summary>
        public IReadOnlyList<CategoryDefinition> Categories => _categories;

        public DefinitionLoader(ILogger<DefinitionLoader> logger, YamlLoader yamlLoader)
        {
            _logger = logger;
            _yamlLoader = yamlLoader;
            _instructions = new List<InstructionDefinition>();
            _isInitialized = false;
        }

        public void Initialize(string cpuDefinitionPath)
        {
            if (_isInitialized)
                throw new InvalidOperationException("DefinitionLoader is already initialized");

            try
            {
                _logger.LogInformation("Initializing DefinitionLoader with CPU definition from {Path}", cpuDefinitionPath);
                LoadDefinitions(cpuDefinitionPath);
                // The categories: their names, sources and text
                var cpuDir = Path.GetDirectoryName(cpuDefinitionPath);
                LoadCategories(Path.Combine(cpuDir!, "categories.yaml"));
                _isInitialized = true;  // Set initialized before validation
                ValidateAll();
                GenerateVisualizations(); // Generate visualizations after validation
                _logger.LogInformation("DefinitionLoader initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load definitions");
                throw new DefinitionLoadException("Failed to load definitions", ex);
            }
        }

        private void LoadDefinitions(string cpuDefinitionPath)
        {
            try
            {
                _logger.LogInformation("Loading definitions from {Path}", cpuDefinitionPath);
                var cpuDir = Path.GetDirectoryName(cpuDefinitionPath);
                var instructionsPath = Path.Combine(cpuDir!, "instructions");

                var (cpuDef, instructions) = _yamlLoader.LoadDefinitions(cpuDefinitionPath, instructionsPath);
                _cpuDefinition = cpuDef;

                // Get the InstructionDefinitions directly from the YamlLoader
                _instructions = _yamlLoader.GetInstructions().ToList();

                _logger.LogInformation("Successfully loaded {Count} instructions from {Path}", _instructions.Count, instructionsPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading definitions from {Path}. Details: {Message}", cpuDefinitionPath, ex.Message);
                throw;
            }
        }

        private void LoadCategories(string path)
        {
            _categoryDocs.Clear();
            _categories = new List<CategoryDefinition>();
            if (!File.Exists(path))
                throw new DefinitionValidationException($"Missing {path}: every instruction category must be defined there");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var file = deserializer.Deserialize<CategoryFile>(File.ReadAllText(path));
            _categories = file?.Categories ?? new List<CategoryDefinition>();
            for (int i = 0; i < _categories.Count; i++)
            {
                var c = _categories[i];
                if (!string.IsNullOrEmpty(c.Description))
                    _categoryDocs[c.Name.ToLowerInvariant()] = c.Description;
            }
            _logger.LogInformation("Loaded {Count} categories from {Path}", _categories.Count, path);
        }

        /// <summary>
        /// Every instruction's category must be in categories.yaml, and every manual an instruction
        /// or a category cites must be listed in cpu.yaml metadata.manuals. A name that does not
        /// match would drop the instruction from every grouped list, or print a reference to nothing.
        /// </summary>
        private void ValidateCategoriesAndManuals()
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < _categories.Count; i++) names.Add(_categories[i].Name);
            var manuals = new HashSet<string>(StringComparer.Ordinal);
            var listed = _cpuDefinition.Metadata.Manuals;
            for (int i = 0; i < listed.Count; i++) manuals.Add(listed[i].Id);
            var errors = new List<string>();
            for (int i = 0; i < _instructions.Count; i++)
            {
                var ins = _instructions[i];
                if (!names.Contains(ins.Category))
                    errors.Add($"{ins.Name}: category '{ins.Category}' is not in categories.yaml");
                for (int r = 0; r < ins.ManualRefs.Count; r++)
                    if (!manuals.Contains(ins.ManualRefs[r].Manual))
                        errors.Add($"{ins.Name}: manual '{ins.ManualRefs[r].Manual}' is not in cpu.yaml manuals");
            }
            for (int i = 0; i < _categories.Count; i++)
                for (int r = 0; r < _categories[i].Sources.Count; r++)
                    if (!manuals.Contains(_categories[i].Sources[r].Manual))
                        errors.Add($"category {_categories[i].Name}: manual '{_categories[i].Sources[r].Manual}' is not in cpu.yaml manuals");
            if (errors.Count > 0)
                throw new DefinitionValidationException(string.Join(Environment.NewLine, errors));
        }

        private void ValidateAll()
        {
            ValidateCpuDefinition();
            ValidateCategoriesAndManuals();
            ValidateInstructions();
            ValidateInstructionClasses();
            ValidateOperandTypes();
            ValidateRegisters();
        }

        private void ValidateCpuDefinition()
        {
            if (_cpuDefinition == null)
                throw new DefinitionValidationException("CPU definition is null", string.Empty, string.Empty, string.Empty, "CPU Definition Validation");

            if (_cpuDefinition.Metadata == null)
                throw new DefinitionValidationException("CPU metadata is null", string.Empty, string.Empty, string.Empty, "CPU Definition Validation");

            if (string.IsNullOrEmpty(_cpuDefinition.Metadata.CpuName))
                throw new DefinitionValidationException("CPU name is not specified", string.Empty, string.Empty, string.Empty, "CPU Definition Validation");

            if (_cpuDefinition.Metadata.WordSize <= 0)
                throw new DefinitionValidationException("Invalid word size", string.Empty, string.Empty, string.Empty, "CPU Definition Validation");
        }

        private void ValidateInstructions()
        {
            try
            {
                if (_instructions == null)
                    throw new DefinitionValidationException("Instructions list is null", string.Empty, string.Empty, string.Empty, "Instruction Validation");

                if (!_instructions.Any())
                    throw new DefinitionValidationException("No instructions found", string.Empty, string.Empty, string.Empty, "Instruction Validation");

                foreach (var instruction in _instructions)
                {
                    try
                    {
                        // Validate opcode operand
                        var opcodeOperand = instruction.Operands?.FirstOrDefault(o => o.Name == "opcode");
                        if (opcodeOperand != null)
                        {
                            if (opcodeOperand.Type != "opcode")
                            {
                                var context = $"Instruction format: {instruction.Format}, Opcode: {instruction.Opcode}, Mask: {instruction.Mask}";
                                var suggestedFix = "Add 'type: \"opcode\"' to the opcode operand in the instruction definition";
                                _logger.LogError("""
                                    [ERROR] Definition Validation Failed
                                    --------------------------------
                                    Error: Opcode operand must have type='opcode'

                                    Validation Stage:
                                    - Instruction Validation

                                    Context:
                                    - Instruction: {InstructionName}
                                    - Details: {Context}

                                    Suggested Fix:
                                    {SuggestedFix}
                                    """,
                                    instruction.Name,
                                    context,
                                    suggestedFix);
                                throw new DefinitionValidationException(
                                    "Opcode operand must have type='opcode'",
                                    instruction.Name,
                                    context,
                                    suggestedFix,
                                    "Instruction Validation"
                                );
                            }
                        }

                        // Validate value-type operands
                        foreach (var operand in instruction.Operands ?? Enumerable.Empty<InstructionOperand>())
                        {
                            if (operand.Type == "value")
                            {
                                if (string.IsNullOrEmpty(operand.Value))
                                {
                                    _logger.LogError("""
                                        [ERROR] Definition Validation Failed
                                        --------------------------------
                                        Error: Value-type operand '{OperandName}' must have a value specified

                                        Validation Stage:
                                        - Instruction Validation

                                        Context:
                                        - Instruction: {InstructionName}
                                        - Details: Operand bits: {Bits}, Description: {Description}

                                        Suggested Fix:
                                        Add a 'value' field to the operand definition
                                        """,
                                        operand.Name,
                                        instruction.Name,
                                        operand.Bits,
                                        operand.Description);
                                    throw new DefinitionValidationException(
                                        $"Value-type operand '{operand.Name}' must have a value specified",
                                        instruction.Name,
                                        $"Operand bits: {operand.Bits}, Description: {operand.Description}",
                                        "Add a 'value' field to the operand definition",
                                        "Instruction Validation"
                                    );
                                }
                            }
                        }
                    }
                    catch (DefinitionValidationException ex)
                    {
                        _logger.LogError(ex, "Validation failed for instruction {Name}: {Message}", instruction.Name, ex.Message);
                        throw; // Re-throw to stop validation immediately
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error validating instruction {Name}: {Message}", instruction.Name, ex.Message);
                        throw new DefinitionValidationException(
                            $"Unexpected error validating instruction {instruction.Name}: {ex.Message}",
                            instruction.Name,
                            $"Format: {instruction.Format}, Opcode: {instruction.Opcode}",
                            "Check the instruction definition for syntax errors",
                            "Instruction Validation"
                        );
                    }
                }
            }
            catch (DefinitionValidationException ex)
            {
                _logger.LogError(ex, "Instruction validation failed: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during instruction validation: {Message}", ex.Message);
                throw new DefinitionValidationException(
                    $"Unexpected error during instruction validation: {ex.Message}",
                    string.Empty,
                    string.Empty,
                    "Check the instruction definitions for errors",
                    "Instruction Validation"
                );
            }
        }

        private void ValidateInstructionClasses()
        {
            var validClasses = _cpuDefinition.Metadata.InstructionClasses.Select(c => c.Name).ToHashSet();
            var invalidInstructions = _instructions
                .Where(i => !string.IsNullOrEmpty(i.InstructionClass) && !validClasses.Contains(i.InstructionClass))
                .ToList();

            if (invalidInstructions.Any())
            {
                var invalidNames = string.Join(", ", invalidInstructions.Select(i => i.Name));
                throw new DefinitionValidationException(
                    $"Invalid instruction classes found in instructions: {invalidNames}",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Instruction Class Validation"
                );
            }
        }

        private void ValidateOperandTypes()
        {
            if (_cpuDefinition.Metadata.OperandTypes == null || !_cpuDefinition.Metadata.OperandTypes.Any())
            {
                throw new DefinitionValidationException(
                    "No operand types defined in CPU definition",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Operand Type Validation"
                );
            }

            foreach (var operandType in _cpuDefinition.Metadata.OperandTypes)
            {
                if (string.IsNullOrWhiteSpace(operandType.Name))
                {
                    throw new DefinitionValidationException(
                        "Operand type name cannot be empty",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "Operand Type Validation"
                    );
                }

                if (string.IsNullOrWhiteSpace(operandType.Description))
                {
                    throw new DefinitionValidationException(
                        $"Description missing for operand type '{operandType.Name}'",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "Operand Type Validation"
                    );
                }

                // Skip bit size validation for opcode type since it varies per instruction
                if (operandType.Name != "opcode" && operandType.BitSize <= 0)
                {
                    throw new DefinitionValidationException(
                        $"Invalid bit size for operand type '{operandType.Name}'",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "Operand Type Validation"
                    );
                }

                // Special validation for opcode type
                if (operandType.Name == "opcode")
                {
                    if (operandType.Type != "opcode")
                    {
                        throw new DefinitionValidationException(
                            "Operand type 'opcode' must have type='opcode'",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            "Operand Type Validation"
                        );
                    }
                }
            }
        }

        private void ValidateRegisters()
        {
            if (_cpuDefinition.Metadata.Registers == null || !_cpuDefinition.Metadata.Registers.Any())
            {
                throw new DefinitionValidationException(
                    "No registers defined in CPU definition",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Register Validation"
                );
            }

            foreach (var register in _cpuDefinition.Metadata.Registers)
            {
                if (string.IsNullOrWhiteSpace(register.Name))
                {
                    throw new DefinitionValidationException(
                        "Register name cannot be empty",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "Register Validation"
                    );
                }

                if (string.IsNullOrWhiteSpace(register.Description))
                {
                    throw new DefinitionValidationException(
                        $"Description missing for register '{register.Name}'",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "Register Validation"
                    );
                }
            }
        }


        /// <summary>
        /// After all details has been inherited, we can noe finally generated the visual
        /// </summary>
        private void GenerateVisualizations()
        {
            // Generate visualizations for instructions
            foreach (var instruction in _instructions)
            {
                instruction.GeneratedBitFormat = BitFormatVisualizer.GenerateVisualization(instruction.Operands); // # 2                                
            }
        }

        public virtual CpuDefinition GetCpuDefinition()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DefinitionLoader is not initialized");
            return _cpuDefinition;
        }

        public virtual IReadOnlyList<InstructionDefinition> GetInstructions()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DefinitionLoader is not initialized");
            return _instructions;
        }
    }

    public class DefinitionLoadException : Exception
    {
        public DefinitionLoadException(string message) : base(message) { }
        public DefinitionLoadException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DefinitionValidationException : Exception
    {
        public string? InstructionName { get; }
        public string? Context { get; }
        public string? SuggestedFix { get; }
        public string? ValidationStage { get; }

        public DefinitionValidationException(string message) : base(message)
        {
        }

        public DefinitionValidationException(string message, string instructionName) : base(message)
        {
            InstructionName = instructionName;
        }

        public DefinitionValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DefinitionValidationException(string message, string instructionName, string context, string suggestedFix) : base(message)
        {
            InstructionName = instructionName;
            Context = context;
            SuggestedFix = suggestedFix;
        }

        public DefinitionValidationException(string message, string instructionName, string context, string suggestedFix, string validationStage) : base(message)
        {
            InstructionName = instructionName;
            Context = context;
            SuggestedFix = suggestedFix;
            ValidationStage = validationStage;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[ERROR] Definition Validation Failed");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Error: {Message}");

            if (!string.IsNullOrEmpty(ValidationStage))
            {
                sb.AppendLine();
                sb.AppendLine("Validation Stage:");
                sb.AppendLine($"- {ValidationStage}");
            }

            if (!string.IsNullOrEmpty(InstructionName))
            {
                sb.AppendLine();
                sb.AppendLine("Context:");
                sb.AppendLine($"- Instruction: {InstructionName}");
                if (!string.IsNullOrEmpty(Context))
                {
                    sb.AppendLine($"- Details: {Context}");
                }
            }

            if (!string.IsNullOrEmpty(SuggestedFix))
            {
                sb.AppendLine();
                sb.AppendLine("Suggested Fix:");
                sb.AppendLine(SuggestedFix);
            }

            if (InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(InnerException.ToString());
            }

            return sb.ToString();
        }
    }
}