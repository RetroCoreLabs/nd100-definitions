using System;
using System.Collections.Generic;
using System.Linq;
using NDGen.Core.Models;

namespace NDGen.Core.Utils
{
    /// <summary>
    /// Helper class for working with enhanced addressing mode definitions
    /// </summary>
    public static class AddressingModeHelper
    {
        /// <summary>
        /// Gets the format string for an addressing mode based on its bit pattern
        /// </summary>
        public static string GetAddressingModeFormat(OperandType addressingModeType, string bitPattern)
        {
            if (addressingModeType.Type != "addressing_mode")
                throw new ArgumentException("Operand type is not an addressing_mode", nameof(addressingModeType));

            // Find the matching addressing mode by bit pattern
            var mode = addressingModeType.Values?.FirstOrDefault(v => v.BitPattern == bitPattern);
            return mode?.Format ?? "<disp>";
        }

        /// <summary>
        /// Extracts the subfield values (X, I, B) from an addressing mode value
        /// </summary>
        public static Dictionary<string, int> ExtractSubfieldValues(int addressingModeValue)
        {
            // Extract X, I, B from the 3-bit value
            return new Dictionary<string, int>
            {
                { "X", (addressingModeValue >> 2) & 0x1 },
                { "I", (addressingModeValue >> 1) & 0x1 },
                { "B", addressingModeValue & 0x1 }
            };
        }

        /// <summary>
        /// Gets the format string for an addressing mode by applying composite effects to the base format
        /// </summary>
        public static string GetFormatWithCompositeEffects(OperandType addressingModeType, int addressingModeValue)
        {
            var subfieldValues = ExtractSubfieldValues(addressingModeValue);
            var formatParts = new List<string> { "<disp>" }; // Base format
            
            if (addressingModeType.CompositeEffects == null)
                return string.Join(" ", formatParts);

            // Apply each composite effect based on conditions
            foreach (var effect in addressingModeType.CompositeEffects)
            {
                // Parse condition like "X=1"
                string[] conditionParts = effect.Condition.Split('=');
                if (conditionParts.Length != 2)
                    continue;

                string fieldName = conditionParts[0].Trim();
                if (!int.TryParse(conditionParts[1], out int requiredValue))
                    continue;

                // Check if condition is met
                if (subfieldValues.TryGetValue(fieldName, out int actualValue) && actualValue == requiredValue)
                {
                    // Add format part based on position
                    if (effect.Position == "prefix")
                        formatParts.Insert(0, effect.FormatAddition);
                    else
                        formatParts.Add(effect.FormatAddition);
                }
            }

            return string.Join(" ", formatParts);
        }

        /// <summary>
        /// Gets the effective address calculation for an addressing mode
        /// </summary>
        public static string GetEffectiveAddressCalculation(OperandType addressingModeType, int addressingModeValue)
        {
            if (addressingModeType.Type != "addressing_mode")
                throw new ArgumentException("Operand type is not an addressing_mode", nameof(addressingModeType));

            // Find the value that matches the addressing mode value
            var mode = addressingModeType.Values?.FirstOrDefault(v => 
                v.Value == addressingModeValue);
            
            return mode?.EffectiveAddress ?? "unknown";
        }
    }
}