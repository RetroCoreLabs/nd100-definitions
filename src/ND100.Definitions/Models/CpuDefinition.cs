using System.Collections.Generic;
using System;
using System.Linq;
using YamlDotNet.Serialization;

namespace NDGen.Core.Models
{
    public class CpuDefinition
    {
        [YamlMember(Alias = "metadata")]
        public required CpuMetadata Metadata { get; init; }

        [YamlMember(Alias = "instructions")]
        public required List<Instruction> Instructions { get; init; }
    }

    public class CpuMetadata
    {
        [YamlMember(Alias = "cpu_name")]
        public required string CpuName { get; init; }

        [YamlMember(Alias = "word_size")]
        public required int WordSize { get; init; }

        [YamlMember(Alias = "endianness")]
        public required string Endianness { get; init; }

        [YamlMember(Alias = "paths")]
        public required Dictionary<string, string> Paths { get; init; }

        [YamlMember(Alias = "registers")]
        public required List<Register> Registers { get; init; }

        [YamlMember(Alias = "tokens")]
        public required Dictionary<string, TokenDefinition> Tokens { get; init; }

        [YamlMember(Alias = "instruction_classes")]
        public required List<InstructionClass> InstructionClasses { get; init; }

        [YamlMember(Alias = "operand_types")]
        public List<OperandType> OperandTypes { get; set; } = new();
    }

    public class Register
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }

        [YamlMember(Alias = "description")]
        public required string Description { get; init; }

        [YamlMember(Alias = "size")]
        public int? Size { get; init; }

        [YamlMember(Alias = "program_levels")]
        public int? ProgramLevels { get; init; }

        [YamlMember(Alias = "code")]
        public int? Code { get; init; }

        [YamlMember(Alias = "type")]
        public string? Type { get; init; }

        [YamlMember(Alias = "bits")]
        public List<RegisterBit>? Bits { get; init; }

        [YamlMember(Alias = "mnemonic_source")]
        public string? MnemonicSource { get; init; }

        [YamlMember(Alias = "mnemonic_target")]
        public string? MnemonicTarget { get; init; }

        [YamlMember(Alias = "value")]
        public int? Value { get; init; }
    }

    public class RegisterBit
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }

        [YamlMember(Alias = "size")]
        public required int Size { get; init; }

        [YamlMember(Alias = "description")]
        public required string Description { get; init; }

        [YamlMember(Alias = "position")]
        public required int Position { get; init; }
    }

    public class InstructionClass
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }

        [YamlMember(Alias = "mask")]
        public string? Mask { get; init; }

        [YamlMember(Alias = "description")]
        public required string Description { get; init; }

        [YamlMember(Alias = "format")]
        public required string Format { get; init; }

        [YamlMember(Alias = "operands")]
        public List<InstructionOperand>? Operands { get; init; }

        public string GeneratedBitFormat { get; set; } = "";
    }

    public class CpuInstructionOperand
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }

        [YamlMember(Alias = "bits")]
        public required string Bits { get; init; }

        [YamlMember(Alias = "description")]
        public required string Description { get; init; }


        [YamlMember(Alias = "detailed_description")]
        public string DetailedDescription { get; init; } = "";


        [YamlMember(Alias = "type")]
        public string? Type { get; init; }

        [YamlMember(Alias = "enum")]
        public List<EnumOption>? Enum { get; init; }

        /// <summary>
        /// Gets the bit range for this operand.
        /// Returns a tuple of (start, end) where start >= end.
        /// For single bits, start and end will be the same.
        /// </summary>
        public (int start, int end) GetBitRange()
        {
            if (string.IsNullOrEmpty(Bits))
                throw new InvalidOperationException("Bits field is empty");

            if (Bits.Contains("-"))
            {
                var parts = Bits.Split('-');
                if (parts.Length != 2)
                    throw new InvalidOperationException($"Invalid bit range format: {Bits}");

                if (!int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
                    throw new InvalidOperationException($"Invalid bit range numbers: {Bits}");

                if (start < end)
                    throw new InvalidOperationException($"Bit range start must be greater than or equal to end: {Bits}");

                return (start, end);
            }
            else
            {
                if (!int.TryParse(Bits, out int bit))
                    throw new InvalidOperationException($"Invalid bit number: {Bits}");

                return (bit, bit);
            }
        }
    }

    public class EnumOption
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }

        [YamlMember(Alias = "value")]
        public required string Value { get; init; }

        [YamlMember(Alias = "description")]
        public required string Description { get; init; }
    }

    public class OperandType
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }
        
        [YamlMember(Alias = "description")]
        public required string Description { get; init; }
        
        [YamlMember(Alias = "type")]
        public required string Type { get; init; }
        
        [YamlMember(Alias = "bit_size")]
        public int? BitSize { get; init; }
        
        [YamlMember(Alias = "signed")]
        public bool? Signed { get; init; }
        
        [YamlMember(Alias = "values")]
        public List<OperandValue>? Values { get; init; }
        
        [YamlMember(Alias = "subfields")]
        public List<OperandSubfield>? Subfields { get; init; }
        
        [YamlMember(Alias = "composite_effects")]
        public List<CompositeEffect>? CompositeEffects { get; init; }
    }

    public class OperandSubfield
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }
        
        [YamlMember(Alias = "position")]
        public required int Position { get; init; }
        
        [YamlMember(Alias = "bit_size")]
        public required int BitSize { get; init; }
        
        [YamlMember(Alias = "description")]
        public required string Description { get; init; }
        
        [YamlMember(Alias = "effect_when_set")]
        public string? EffectWhenSet { get; init; }
    }

    public class CompositeEffect
    {
        [YamlMember(Alias = "condition")]
        public required string Condition { get; init; }
        
        [YamlMember(Alias = "format_addition")]
        public required string FormatAddition { get; init; }
        
        [YamlMember(Alias = "position")]
        public required string Position { get; init; }
    }

    public class OperandValue
    {
        [YamlMember(Alias = "name")]
        public required string Name { get; init; }
        
        [YamlMember(Alias = "value")]
        public int? Value { get; init; }
        
        [YamlMember(Alias = "mode")]
        public string? Mode { get; init; }
        
        [YamlMember(Alias = "description")]
        public string? Description { get; init; }
        
        [YamlMember(Alias = "bit_pattern")]
        public string? BitPattern { get; init; }
        
        [YamlMember(Alias = "format")]
        public string? Format { get; init; }
        
        [YamlMember(Alias = "example")]
        public string? Example { get; init; }
        
        [YamlMember(Alias = "fields")]
        public List<Dictionary<string, object>>? Fields { get; init; }
        
        [YamlMember(Alias = "effective_address")]
        public string? EffectiveAddress { get; init; }
    }
}