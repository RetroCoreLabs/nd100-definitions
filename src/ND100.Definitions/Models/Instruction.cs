using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NDGen.Core.Models
{
    /// <summary>
    /// Represents the privilege level required to execute an instruction
    /// </summary>
    public enum PrivilegeLevel
    {
        /// <summary>
        /// Regular instructions that can be executed in user mode
        /// </summary>
        User,

        /// <summary>
        /// Instructions that require elevated privileges to execute
        /// </summary>
        Privileged
    }

    /// <summary>
    /// Represents an instruction operand with its properties and validation
    /// </summary>
    public class InstructionOperand
    {
        /// <summary>
        /// Gets or sets the operand name. Must start with a letter and contain only letters, numbers, and underscores.
        /// </summary>
        [Required]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]*$")]
        public required string Name { get; init; }

        /// <summary>
        /// Gets or sets the operand type (e.g., uint5, addressing_mode, displacement, enum)
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// Gets or sets the description of the operand
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bit range in format start-end (e.g., "15-0")
        /// </summary>
        [Required]
        [RegularExpression(@"^[0-9]+-[0-9]+$")]
        public required string Bits { get; init; }

        /// <summary>
        /// Gets or sets the enum options for operands of type 'enum'
        /// </summary>
        public List<EnumOption>? Enum { get; init; }

        /// <summary>
        /// Gets or sets the optional fixed value for the operand
        /// </summary>
        public string? Value { get; init; }

        /// <summary>
        /// Gets or sets the detailed operand type information
        /// </summary>
        [JsonIgnore]
        public OperandType? OperandTypeDetails { get; set; }
        
        /// <summary>
        /// Gets or sets the subfields of the operand
        /// </summary>
        [JsonIgnore]
        public List<OperandSubfield>? Subfields { get; set; }
        
        /// <summary>
        /// Gets or sets the composite effects of the operand
        /// </summary>
        [JsonIgnore]
        public List<CompositeEffect>? CompositeEffects { get; set; }
        
        /// <summary>
        /// Gets or sets the possible values of the operand
        /// </summary>
        [JsonIgnore]
        public List<OperandValue>? Values { get; set; }
        
        /// <summary>
        /// Gets or sets whether this operand is an addressing mode type
        /// </summary>
        [JsonIgnore]
        public bool IsAddressingMode { get; set; } = false;
    }

    /// <summary>
    /// Represents the effects of an instruction on CPU flags
    /// </summary>
    public class FlagEffects
    {
        public FlagEffects()
        {
            C = "unaffected";
            O = "unaffected";
            Q = "unaffected";
            Z = "unaffected";
        }

        /// <summary>
        /// Gets or sets the effect on the Carry flag
        /// </summary>
        public string C { get; set; }

        /// <summary>
        /// Gets or sets the effect on the Overflow flag
        /// </summary>
        public string O { get; set; }

        /// <summary>
        /// Gets or sets the effect on the Dynamic Overflow flag
        /// </summary>
        public string Q { get; set; }

        /// <summary>
        /// Gets or sets the effect on the Error flag
        /// </summary>
        public string Z { get; set; }
    }

    /// <summary>
    /// A place in a manual (YAML <c>manual_refs</c> entry of an instruction, <c>sources</c> entry
    /// of a category). Page is the page number printed in the manual.
    /// </summary>
    public class ManualRef
    {
        /// <summary>The manual id, for example ND-06.029.1 (see cpu.yaml <c>metadata.manuals</c>).</summary>
        public string Manual { get; set; } = string.Empty;

        /// <summary>The printed page number, when one page is meant.</summary>
        public int? Page { get; set; }

        /// <summary>A page range such as "137-146", for a group of entries.</summary>
        public string? Pages { get; set; }

        /// <summary>A section number and title, such as "3.2.1.2 Store Instructions".</summary>
        public string? Section { get; set; }
    }

    /// <summary>
    /// Represents an example of instruction usage
    /// </summary>
    public class Example
    {
        /// <summary>
        /// Gets or sets the description of what the example demonstrates
        /// </summary>
        [Required]
        public required string Description { get; init; }

        /// <summary>
        /// Gets or sets the example assembly code
        /// </summary>
        [Required]
        public required string Code { get; init; }
    }

    /// <summary>
    /// One place execution can continue after an instruction (YAML <c>exits</c> entry).
    /// Exactly one of <see cref="Offset"/> and <see cref="Target"/> is set.
    /// </summary>
    public class InstructionExit
    {
        /// <summary>
        /// Gets or sets the fixed exit, in words from the instruction's own address
        /// (1 = the next word, 2 = skip one word). Null when <see cref="Target"/> is used.
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the kind of non-fixed exit: effective_address, displacement, result,
        /// register, stack_link, stack_link_minus_1 or executed_instruction.
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// Gets or sets the register P is loaded from when <see cref="Target"/> is "register".
        /// </summary>
        public string? Register { get; set; }

        /// <summary>
        /// Gets or sets when this exit is used: always, ok, error, taken or not_taken.
        /// </summary>
        public string When { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets, in plain words, what makes this exit happen.
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Gets or sets extra facts about this exit.
        /// </summary>
        public string? Note { get; set; }
    }

    /// <summary>
    /// Represents a complete instruction definition with validation
    /// </summary>
    public class InstructionDefinition
    {
        public InstructionDefinition()
        {
            Operands = new List<InstructionOperand>();
            Flags = new FlagEffects();
            Examples = new List<Example>();
            FlagsAffected = new List<string>();
        }

        /// <summary>
        /// Gets or sets the instruction name in uppercase (e.g., 'ADD', 'SUB', 'AND')
        /// </summary>
        [Required]
        [RegularExpression(@"^[A-Z][A-Z0-9_]*$")]
        public required string Name { get; init; }

        /// <summary>
        /// Gets or sets the basic description of what the instruction does
        /// </summary>
        [Required]
        public required string Description { get; init; }

        /// <summary>
        /// Gets or sets the detailed description with technical information and usage notes
        /// </summary>
        public string? DetailedDescription { get; init; }

        /// <summary>
        /// Gets or sets more text after the detailed description (YAML <c>extended_description</c>).
        /// </summary>
        public string? ExtendedDescription { get; init; }

        /// <summary>
        /// Gets or sets what must be set up before the instruction is used (YAML <c>before_use</c>).
        /// </summary>
        public string? BeforeUse { get; init; }

        /// <summary>
        /// Gets or sets where the manuals describe the instruction (YAML <c>manual_refs</c>).
        /// The manual ids are listed in cpu.yaml <c>metadata.manuals</c>.
        /// </summary>
        public List<ManualRef> ManualRefs { get; init; } = new List<ManualRef>();

        /// <summary>
        /// Gets or sets the instruction opcode in octal format
        /// </summary>
        [Required]
        [RegularExpression(@"^0[0-7]{6}$")]
        public required string Opcode { get; init; }

        /// <summary>
        /// Gets or sets the bit mask for opcode (may be omitted if instruction_class is defined)
        /// </summary>
        [RegularExpression(@"^[01_]+$")]
        public string? Mask { get; init; }

        /// <summary>
        /// Gets or sets the assembly format of the instruction
        /// </summary>
        [Required]
        public required string Format { get; init; }

        /// <summary>
        /// Gets or sets the category of the instruction
        /// </summary>
        [Required]
        public required string Category { get; init; }

        /// <summary>
        /// Gets or sets the instruction class (must be a valid class defined in cpu.yaml)
        /// </summary>
        [Required]
        public required string InstructionClass { get; init; }

        /// <summary>
        /// Gets or sets the privilege level required to execute the instruction
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required PrivilegeLevel Privilege { get; init; }

        /// <summary>
        /// Gets or sets whether this instruction is an alias for another instruction
        /// Used by dissasembler to detect that it might 
        /// </summary>
        public bool IsAlias { get; init; }

        /// <summary>
        /// Gets or sets the instruction operands
        /// </summary>
        public List<InstructionOperand>? Operands { get; init; }

        /// <summary>
        /// Gets or sets the flags affected by this instruction
        /// </summary>
        [Required]
        public List<string> FlagsAffected { get; set; }

        /// <summary>
        /// Gets or sets the detailed flag effects
        /// </summary>
        public FlagEffects Flags { get; set; }

        /// <summary>
        /// Gets or sets the pseudocode showing the instruction's behavior
        /// </summary>
        public string? Pseudocode { get; init; }

        /// <summary>
        /// Gets or sets the examples showing how to use the instruction
        /// </summary>
        public List<Example> Examples { get; set; }

        /// <summary>
        /// Gets or sets the number of data words after the instruction word that the
        /// instruction reads (for example the stack demand after ENTR). 0 when absent.
        /// </summary>
        public int InlineWords { get; set; }

        /// <summary>
        /// Gets or sets every place execution can continue after this instruction
        /// (YAML field <c>exits</c>). Traps, page faults and interrupts are not listed.
        /// </summary>
        public List<InstructionExit> Exits { get; set; } = new List<InstructionExit>();

        /// <summary>
        /// Gets or sets where the exits were read from (nd100x file, line, function and commit).
        /// </summary>
        public string? ExitsSource { get; set; }

        /// <summary>
        /// Gets or sets the auto-generated bit format visualization
        /// </summary>
        public string? GeneratedBitFormat { get; set; }

        /// <summary>
        /// Gets the instruction's opcode as a parsed 16-bit unsigned integer value
        /// </summary>
        [JsonIgnore]
        public ushort OpcodeValue {
            get {
                if (string.IsNullOrEmpty(Opcode))
                    return 0;
                
                string cleanedOpcode = Opcode.Replace("_", "").Replace(" ", "");
                try {
                    return Convert.ToUInt16(cleanedOpcode, 8);
                }
                catch (Exception ex) {
                    throw new FormatException(
                        $"Failed to parse opcode '{Opcode}' for instruction '{Name}'. " +
                        $"Cleaned value was '{cleanedOpcode}'. Make sure it's a valid octal number. {ex.Message}", ex);
                }
            }
        }
            
        /// <summary>
        /// Gets the instruction's opcode value formatted as a 6-character octal string
        /// </summary>
        [JsonIgnore]
        public string OpcodeValueFormatted {
            get {
                try {
                    if (string.IsNullOrEmpty(Opcode))
                        return "000000";
                    return Convert.ToString(OpcodeValue, 8).PadLeft(6, '0');
                }
                catch (Exception ex) {
                    throw new FormatException(
                        $"Failed to format opcode '{Opcode}' for instruction '{Name}'. {ex.Message}", ex);
                }
            }
        }
    }

    /// <summary>
    /// Represents a simplified instruction model for display purposes
    /// </summary>
    public class Instruction
    {
        public Instruction()
        {
            Operands = new List<string>();
            FlagsAffected = new List<string>();
        }

        /// <summary>
        /// Gets or sets the instruction name
        /// </summary>
        [Required]
        public required string Name { get; init; }

        /// <summary>
        /// Gets or sets the instruction description
        /// </summary>
        [Required]
        public required string Description { get; init; }

        /// <summary>
        /// Gets or sets the detailed description
        /// </summary>
        public string? DetailedDescription { get; init; }

        /// <summary>
        /// Gets or sets the instruction opcode
        /// </summary>
        public string? Opcode { get; init; }

        /// <summary>
        /// Gets or sets the instruction mask
        /// </summary>
        public string? Mask { get; init; }

        /// <summary>
        /// Gets or sets the instruction format
        /// </summary>
        [Required]
        public required string Format { get; init; }

        /// <summary>
        /// Gets or sets the instruction category
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// Gets or sets the instruction class
        /// </summary>
        public string? InstructionClass { get; init; }

        /// <summary>
        /// Gets or sets the privilege level
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PrivilegeLevel Privilege { get; init; }

        /// <summary>
        /// Gets or sets the operand names
        /// </summary>
        public List<string> Operands { get; init; }

        /// <summary>
        /// Gets or sets the generated bit format
        /// </summary>
        public string? GeneratedBitFormat { get; init; }

        /// <summary>
        /// Gets or sets the affected flags
        /// </summary>
        public List<string> FlagsAffected { get; set; }
    }
}