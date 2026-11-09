using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;
using NDGen.Core.Models;
using NDGen.Core.Exceptions;
using NDGen.Core.Utils;
using YamlDotNet.Core;
using System.Text.Json;

namespace NDGen.Core
{
    public class YamlLoader
    {
        private readonly ILogger<YamlLoader> _logger;
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;
        private CpuDefinition _cpuDefinition = null!;
        private readonly List<InstructionDefinition> _instructions = new();

        public YamlLoader(ILogger<YamlLoader> logger)
        {
            _logger = logger;
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new PrivilegeLevelConverter())
                .Build();
            _serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithTypeConverter(new PrivilegeLevelConverter())
                .Build();
        }

        private void LogYamlError(string filePath, string yamlContent, Exception ex)
        {
            if (ex is YamlException yamlEx)
            {
                var lineNumber = yamlEx.Start.Line;
                var columnNumber = yamlEx.Start.Column;
                var lineContent = GetLineContent(yamlContent, lineNumber);
                var context = GetErrorContext(yamlContent, lineNumber);
                
                _logger.LogError("""
                    YAML Error in {FilePath}
                    Line {LineNumber}, Column {ColumnNumber}: {ErrorMessage}
                    
                    Problematic Line:
                    {LineContent}
                    
                    Context:
                    {Context}
                    """, 
                    filePath,
                    lineNumber,
                    columnNumber,
                    yamlEx.Message,
                    lineContent,
                    context);
            }
            else
            {
                _logger.LogError("""
                    Error loading {FilePath}
                    {ErrorMessage}
                    """,
                    filePath,
                    ex.Message);
            }
        }

        private string GetLineContent(string content, int lineNumber)
        {
            var lines = content.Split('\n');
            if (lineNumber <= 0 || lineNumber > lines.Length)
                return "Line not found";
            
            var line = lines[lineNumber - 1];
            return $"{lineNumber}: {line}";
        }

        private string GetErrorContext(string content, int lineNumber, int contextLines = 3)
        {
            var lines = content.Split('\n');
            var startLine = Math.Max(1, lineNumber - contextLines);
            var endLine = Math.Min(lines.Length, lineNumber + contextLines);
            
            var context = new StringBuilder();
            for (int i = startLine; i <= endLine; i++)
            {
                var prefix = i == lineNumber ? ">>> " : "    ";
                context.AppendLine($"{prefix}{i}: {lines[i - 1]}");
            }
            return context.ToString();
        }

        public (CpuDefinition CpuDefinition, List<Instruction> Instructions) LoadDefinitions(string cpuDefinitionPath, string instructionsPath)
        {
            try
            {
                _logger.LogInformation("""
                    ============================================
                    STARTING YAML LOADING PROCESS
                    ============================================
                    CPU Definition Path: {CpuPath}
                    Instructions Path: {InstructionsPath}
                    """, cpuDefinitionPath, instructionsPath);
                
                // Load CPU definition
                var cpuYaml = File.ReadAllText(cpuDefinitionPath);
                
                try
                {
                    _cpuDefinition = _deserializer.Deserialize<CpuDefinition>(cpuYaml);
                    _logger.LogInformation("Successfully loaded CPU definition from {Path}", cpuDefinitionPath);

                    // Validate required properties
                    if (_cpuDefinition.Metadata == null)
                        throw new DefinitionValidationException("CPU metadata is null");

                    if (_cpuDefinition.Metadata.Registers == null)
                        throw new DefinitionValidationException("CPU registers are null");

                    foreach (var register in _cpuDefinition.Metadata.Registers)
                    {
                        if (register.Bits != null)
                        {
                            foreach (var bit in register.Bits)
                            {
                                if (string.IsNullOrEmpty(bit.Name))
                                    throw new DefinitionValidationException($"Register bit name is required in register {register.Name}");
                                if (bit.Size <= 0)
                                    throw new DefinitionValidationException($"Register bit size must be positive in register {register.Name}, bit {bit.Name}");
                                if (string.IsNullOrEmpty(bit.Description))
                                    throw new DefinitionValidationException($"Register bit description is required in register {register.Name}, bit {bit.Name}");
                                if (bit.Position < 0)
                                    throw new DefinitionValidationException($"Register bit position must be non-negative in register {register.Name}, bit {bit.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogYamlError(cpuDefinitionPath, cpuYaml, ex);
                    throw new DefinitionLoadException($"Failed to deserialize CPU definition: {ex.Message}", ex);
                }

                // Load instruction files
                var cpuDir = Path.GetDirectoryName(cpuDefinitionPath);
                var resolvedInstructionsPath = Path.Combine(cpuDir!, _cpuDefinition.Metadata.Paths["instructions"]);

                if (!Directory.Exists(resolvedInstructionsPath))
                {
                    throw new DirectoryNotFoundException($"Instructions directory not found: {resolvedInstructionsPath}");
                }

                var instructionFiles = Directory.GetFiles(resolvedInstructionsPath, "*.yaml", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith("_template.yaml") && !f.EndsWith("cpu.yaml"));
                
                _logger.LogInformation("Found {Count} instruction files", instructionFiles.Count());

                foreach (var file in instructionFiles)
                {
                    try
                    {
                        var yamlContent = File.ReadAllText(file);

                        try
                        {
                            // First deserialize to a dynamic object to get the instruction property
                            var dynamicObj = _deserializer.Deserialize<Dictionary<string, object>>(yamlContent);
                            if (dynamicObj == null || !dynamicObj.ContainsKey("instruction"))
                            {
                                throw new DefinitionValidationException($"Invalid instruction format in file: {file}");
                            }
                            
                            // Then deserialize the instruction property
                            var instruction = _deserializer.Deserialize<InstructionDefinition>(_serializer.Serialize(dynamicObj["instruction"]));
                            
                            if (string.IsNullOrEmpty(instruction.Name))
                            {
                                throw new DefinitionValidationException($"Instruction name is missing in file: {file}");
                            }
                            
                            _logger.LogInformation("Successfully loaded instruction: {Name}", instruction.Name);
                            
                            // Add automatic opcode operand if mask is "1111_1111_1111_1111" and no operands defined
                            if (instruction.Mask == "1111_1111_1111_1111" && (instruction.Operands == null || !instruction.Operands.Any()))
                            {
                                var newOperands = new List<InstructionOperand>
                                {
                                    new InstructionOperand
                                    {
                                        Name = "opcode",
                                        Type = "opcode",
                                        Bits = "15-0",
                                        Description = "The opcode determines what type of operation occurs"
                                    }
                                };
                                
                                // Create a new instruction with the updated operands
                                var updatedInstruction = new InstructionDefinition
                                {
                                    Name = instruction.Name,
                                    Description = instruction.Description,
                                    DetailedDescription = instruction.DetailedDescription,
                                    Opcode = instruction.Opcode,                                    
                                    Mask = instruction.Mask,
                                    Format = instruction.Format,
                                    Category = instruction.Category,
                                    InstructionClass = instruction.InstructionClass,
                                    Privilege = instruction.Privilege,
                                    Operands = newOperands,
                                    FlagsAffected = instruction.FlagsAffected,
                                    // Keep where execution continues afterwards (exits).
                                    InlineWords = instruction.InlineWords,
                                    Exits = instruction.Exits,
                                    ExitsSource = instruction.ExitsSource
                                };
                                
                                instruction = updatedInstruction;
                            }
                            
                            _instructions.Add(instruction);
                        }
                        catch (Exception ex)
                        {
                            LogYamlError(file, yamlContent, ex);
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load instruction file: {File}", file);
                        throw;
                    }
                }

                // Process operand subfields inheritance for instructions
                ProcessOperandSubfieldInheritance();

                // Generate bit formats
                _logger.LogInformation("Generating bit formats for instruction classes");
                foreach (var instructionClass in _cpuDefinition.Metadata.InstructionClasses)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(instructionClass.Description))
                            throw new DefinitionValidationException($"Instruction class '{instructionClass.Name}' description cannot be empty");

                        if (string.IsNullOrEmpty(instructionClass.Format))
                            throw new DefinitionValidationException($"Instruction class '{instructionClass.Name}' format cannot be empty");

                        if (instructionClass.Operands == null || !instructionClass.Operands.Any())
                            throw new DefinitionValidationException($"Instruction class '{instructionClass.Name}' must have at least one operand");

                        // Apply operand type inheritance to all instruction class operands
                        foreach (var operand in instructionClass.Operands)
                        {
                            EnrichOperandWithTypeDetails(operand);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing instruction class: {Name}", instructionClass.Name);
                        throw;
                    }
                }

                // Generate bit formats for instructions
                _logger.LogInformation("Generating bit formats for instructions");
                foreach (var instruction in _instructions)
                {
                    try
                    {
                        var instructionClass = instruction.InstructionClass != null
                            ? _cpuDefinition.Metadata.InstructionClasses.FirstOrDefault(c => c.Name == instruction.InstructionClass)
                            : null;

                        var operandsToUse = instruction.Operands?.Any() == true
                            ? instruction.Operands
                            : instructionClass?.Operands ?? new List<InstructionOperand>();

                        // Apply operand type inheritance to all instruction operands
                        foreach (var operand in operandsToUse)
                        {
                            EnrichOperandWithTypeDetails(operand);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing instruction: {Name}", instruction.Name);
                        throw;
                    }
                }

                // Convert InstructionDefinitions to Instructions
                _logger.LogInformation("Converting instruction definitions to instructions");
                var instructions = _instructions.Select(def =>
                {
                    try
                    {
                        var instructionClass = def.InstructionClass != null
                            ? _cpuDefinition.Metadata.InstructionClasses.FirstOrDefault(c => c.Name == def.InstructionClass)
                            : null;

                        var operandsToUse = def.Operands?.Any() == true
                            ? def.Operands
                            : instructionClass?.Operands ?? new List<InstructionOperand>();

                        var instruction = new Instruction
                        {
                            Name = def.Name ?? throw new DefinitionValidationException("Instruction name is required"),
                            Description = def.Description ?? throw new DefinitionValidationException("Instruction description is required"),
                            DetailedDescription = def.DetailedDescription,
                            Opcode = def.Opcode,
                            Mask = def.Mask ?? instructionClass?.Mask,
                            Format = def.Format ?? throw new DefinitionValidationException("Instruction format is required"),
                            Category = def.Category,
                            InstructionClass = def.InstructionClass,
                            Privilege = def.Privilege,
                            Operands = operandsToUse.Select(o => o.Name).ToList(),
                            GeneratedBitFormat = def.GeneratedBitFormat ?? string.Empty,
                            FlagsAffected = def.FlagsAffected ?? new List<string>()
                        };

                        return instruction;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error converting instruction definition: {Name}", def.Name);
                        throw;
                    }
                }).ToList();

                _logger.LogInformation("Successfully completed YAML loading process");
                return (_cpuDefinition, instructions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load and validate definitions");
                throw;
            }
        }

        /// <summary>
        /// Processes inheritance of subfields and composite effects from operand types to instructions
        /// </summary>
        private void ProcessOperandSubfieldInheritance()
        {
            //_logger.LogInformation("Processing operand subfield inheritance");

            // Build a dictionary of operand types for quick lookups
            var operandTypes = _cpuDefinition.Metadata.OperandTypes
                .ToDictionary(ot => ot.Name.ToLower(), ot => ot);

            // Process inheritance for instruction classes first
            foreach (var instructionClass in _cpuDefinition.Metadata.InstructionClasses)
            {
                if (instructionClass.Operands == null) 
                    continue;

                foreach (var operand in instructionClass.Operands)
                {
                    ApplyOperandTypeInheritance(operand, operandTypes);
                }
            }

            // Then process inheritance for individual instructions
            for (int i = 0; i < _instructions.Count; i++)
            {
                var instruction = _instructions[i];
                if (instruction.Operands == null)
                    continue;

                // First, inherit operands from instruction class if needed
                if (!string.IsNullOrEmpty(instruction.InstructionClass) && (instruction.Operands == null || !instruction.Operands.Any()))
                {
                    var instructionClass = _cpuDefinition.Metadata.InstructionClasses
                        .FirstOrDefault(ic => ic.Name == instruction.InstructionClass);
                    
                    if (instructionClass?.Operands != null)
                    {
                        // Create a new instruction with the updated operands
                        var updatedInstruction = new InstructionDefinition
                        {
                            Name = instruction.Name,
                            Description = instruction.Description,
                            DetailedDescription = instruction.DetailedDescription,
                            Opcode = instruction.Opcode,
                            Mask = instruction.Mask,
                            Format = instruction.Format,
                            Category = instruction.Category,
                            InstructionClass = instruction.InstructionClass,
                            Privilege = instruction.Privilege,
                            Operands = new List<InstructionOperand>(instructionClass.Operands),
                            FlagsAffected = instruction.FlagsAffected,
                            Flags = instruction.Flags,
                            Pseudocode = instruction.Pseudocode,
                            Examples = instruction.Examples,                            
                            GeneratedBitFormat = instruction.GeneratedBitFormat,
                            // Keep where execution continues afterwards (exits).
                            InlineWords = instruction.InlineWords,
                            Exits = instruction.Exits,
                            ExitsSource = instruction.ExitsSource
                        };
                        
                        // Replace the instruction in the list
                        _instructions[i] = updatedInstruction;
                        instruction = updatedInstruction;
                    }
                }

                // Apply type inheritance to each operand
                if (instruction.Operands != null)
                {
                    foreach (var operand in instruction.Operands)
                    {
                        ApplyOperandTypeInheritance(operand, operandTypes);
                    }
                }
            }

            //_logger.LogInformation("Completed operand subfield inheritance processing");
        }

        /// <summary>
        /// Applies operand type inheritance to an individual operand
        /// </summary>
        private void ApplyOperandTypeInheritance(InstructionOperand operand, Dictionary<string, OperandType> operandTypes)
        {
            if (string.IsNullOrEmpty(operand.Type))
                return;

            // Try to find the referenced operand type
            if (operandTypes.TryGetValue(operand.Type.ToLower(), out var operandType))
            {
                // Store the operand type reference in the operand for easy access
                operand.OperandTypeDetails = operandType;

                // If it's an addressing mode, ensure we have subfield information
                if (operandType.Type == "addressing_mode")
                {
                    // Set the enhanced operand properties from the operand type
                    operand.Subfields = operandType.Subfields;
                    operand.CompositeEffects = operandType.CompositeEffects;
                    operand.Values = operandType.Values;
                    
                    // Also set the IsAddressingMode flag to ensure the visualizer recognizes it
                    operand.IsAddressingMode = true;
                }
            }
        }

        /// <summary>
        /// Enriches an operand with details from its operand type
        /// </summary>
        private void EnrichOperandWithTypeDetails(InstructionOperand operand)
        {
            if (string.IsNullOrEmpty(operand.Type))
                return;

            // Look up the operand type
            var operandType = _cpuDefinition.Metadata.OperandTypes
                .FirstOrDefault(ot => ot.Name.ToLower() == operand.Type.ToLower());

            if (operandType != null)
            {
                // Ensure all relevant details are transferred to the operand
                if (string.IsNullOrEmpty(operand.Description))
                {
                    operand.Description = operandType.Description;
                }

                operand.OperandTypeDetails = operandType;

                // For addressing modes, ensure subfields and composite effects are transferred
                if (operandType.Type == "addressing_mode")
                {
                    if (operand.Subfields == null)
                    {
                        operand.Subfields = operandType.Subfields;
                    }

                    if (operand.CompositeEffects == null)
                    {
                        operand.CompositeEffects = operandType.CompositeEffects;
                    }

                    if (operand.Values == null)
                    {
                        operand.Values = operandType.Values;
                    }
                    
                    // Ensure the IsAddressingMode flag is set
                    operand.IsAddressingMode = true;
                }
                
                // For addressing_modes operand type (plural form)
                else if (operandType.Name.ToLower() == "addressing_modes" && operandType.Type == "addressing_mode")
                {
                    // This is the case in the CPU YAML where type="addressing_modes" 
                    if (operand.Subfields == null)
                    {
                        operand.Subfields = operandType.Subfields;
                    }

                    if (operand.CompositeEffects == null)
                    {
                        operand.CompositeEffects = operandType.CompositeEffects;
                    }

                    if (operand.Values == null)
                    {
                        operand.Values = operandType.Values;
                    }
                    
                    operand.IsAddressingMode = true;
                }
            }
        }

        private class TypeInspector : ITypeInspector
        {
            private readonly ITypeInspector _innerTypeInspector;

            public TypeInspector(ITypeInspector innerTypeInspector)
            {
                _innerTypeInspector = innerTypeInspector;
            }

            public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
            {
                return _innerTypeInspector.GetProperties(type, container);
            }

            // Fix nullability warning by matching the interface signature
            public IPropertyDescriptor GetProperty(Type type, object? container, string name, bool ignoreUnmatched)
            {
                return _innerTypeInspector.GetProperty(type, container, name, ignoreUnmatched)!;
            }
        }

        private class PrivilegeLevelConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return type == typeof(PrivilegeLevel);
            }

            public object? ReadYaml(IParser parser, Type type)
            {
                var value = parser.Consume<YamlDotNet.Core.Events.Scalar>().Value;
                return value.ToLower() switch
                {
                    "user" => PrivilegeLevel.User,
                    "privileged" => PrivilegeLevel.Privileged,
                    _ => throw new YamlException($"Invalid privilege level: {value}. Must be 'user' or 'privileged'.")
                };
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                if (value is PrivilegeLevel privilegeLevel)
                {
                    var stringValue = privilegeLevel switch
                    {
                        PrivilegeLevel.User => "user",
                        PrivilegeLevel.Privileged => "privileged",
                        _ => throw new YamlException($"Invalid privilege level: {value}")
                    };
                    emitter.Emit(new YamlDotNet.Core.Events.Scalar(stringValue));
                }
            }
        }

        public CpuDefinition GetCpuDefinition() => _cpuDefinition;
        public IReadOnlyList<InstructionDefinition> GetInstructions() => _instructions;
    }
}