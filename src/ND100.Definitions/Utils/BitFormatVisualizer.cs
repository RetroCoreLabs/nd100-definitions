using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDGen.Core.Models;

namespace NDGen.Core.Utils
{
    public class FieldInfo
    {
        public required string Name { get; set; }
        public int Width { get; set; }
        public required InstructionOperand Operand { get; set; }
    }

    public static class BitFormatVisualizer
    {
        private const char TopLeft = '┌';
        private const char TopRight = '┐';
        private const char BottomLeft = '└';
        private const char BottomRight = '┘';
        private const char Horizontal = '─';
        private const char Vertical = '│';
        private const char LeftT = '├';
        private const char RightT = '┤';
        private const char Cross = '┼';
        private const char TopT = '┬';
        private const char BottomT = '┴';
        private const int Padding = 1; // Space before and after content
        private const int BitSpacing = 2; // Space between bits
        private const int Indent = 3; // Number of spaces for indentation

        public static string GenerateVisualization(IEnumerable<InstructionOperand>? operands)
        {
            // If no operands provided, create a default opcode operand spanning all bits
            if (operands == null || !operands.Any())
            {
                operands = new[]
                {
                    new InstructionOperand
                    {
                        Name = "opcode",
                        Type = "opcode",
                        Bits = "15-0",
                        Description = "The opcode determines what type of operation occurs"
                    }
                };
            }

            // Calculate layouts for all fields
            var calculator = new FieldLayoutCalculator();
            var visualizations = calculator.CalculateLayouts(operands);
            
            // Sort visualizations by start bit to ensure correct order
            visualizations = visualizations.OrderByDescending(v => v.StartBit).ToList();

            // Calculate total width needed
            var totalWidth = visualizations.Sum(v => v.CalculatedWidth) + visualizations.Count + 1;

            // Create the visualization
            var visualization = new StringBuilder();
            
            // Add bit numbers line
            var bitNumbersLine = new char[totalWidth];
            Array.Fill(bitNumbersLine, ' ');
            bitNumbersLine[0] = Vertical;
            bitNumbersLine[^1] = Vertical;

            // Add bit numbers
            var currentPosition = 1;
            foreach (var vis in visualizations)
            {
                if (currentPosition > 1)
                {
                    bitNumbersLine[currentPosition - 1] = Vertical;
                }

                // Calculate bit positions for this field
                var startBit = vis.StartBit;
                var endBit = vis.EndBit;
                var fieldWidth = vis.CalculatedWidth;
                
                // Calculate positions for all bits in the range
                var bitsInField = startBit - endBit + 1;
                var spacingPerBit = fieldWidth / Math.Max(1, bitsInField);
                
                for (int bit = startBit, pos = 0; bit >= endBit; bit--, pos++)
                {
                    var bitStr = bit.ToString();
                    var bitPos = currentPosition + (pos * spacingPerBit) + ((spacingPerBit - bitStr.Length) / 2);
                    
                    // Ensure we don't write past the field boundary
                    if (bitPos + bitStr.Length <= currentPosition + fieldWidth)
                    {
                        for (int i = 0; i < bitStr.Length; i++)
                        {
                            bitNumbersLine[bitPos + i] = bitStr[i];
                        }
                    }
                }

                currentPosition += fieldWidth;
            }

            visualization.Append(new string(' ', Indent));
            visualization.AppendLine(new string(bitNumbersLine));

            // Add subfield labels if any field has them
            if (visualizations.Any(v => v.Subfields != null && v.Subfields.Any()))
            {
                var subfieldLine = new char[totalWidth];
                Array.Fill(subfieldLine, ' ');
                subfieldLine[0] = Vertical;
                subfieldLine[^1] = Vertical;

                currentPosition = 1;
                foreach (var vis in visualizations)
                {
                    if (currentPosition > 1)
                    {
                        subfieldLine[currentPosition - 1] = Vertical;
                    }

                    if (vis.Subfields != null && vis.Subfields.Any())
                    {
                        // Calculate bit positions for this field
                        var startBit = vis.StartBit;
                        var endBit = vis.EndBit;
                        var fieldWidth = vis.CalculatedWidth;
                        
                        // Calculate positions for all bits in the range
                        var bitsInField = startBit - endBit + 1;
                        var spacingPerBit = fieldWidth / Math.Max(1, bitsInField);
                        
                        // Place each subfield under its corresponding bit
                        foreach (var subfield in vis.Subfields)
                        {
                            var bitPos = currentPosition + ((startBit - subfield.BitPosition) * spacingPerBit) + 
                                       ((spacingPerBit - 1) / 2); // Center the subfield under the bit
                            
                            if (bitPos < subfieldLine.Length)
                            {
                                subfieldLine[bitPos] = subfield.Name[0]; // Use first character of subfield name
                            }
                        }
                    }
                    currentPosition += vis.CalculatedWidth;
                }

                visualization.Append(new string(' ', Indent));
                visualization.AppendLine(new string(subfieldLine));
            }

            // Add middle border
            var middleBorder = new char[totalWidth];
            Array.Fill(middleBorder, Horizontal);
            middleBorder[0] = LeftT;
            middleBorder[^1] = RightT;

            currentPosition = 1;
            foreach (var vis in visualizations)
            {
                if (currentPosition > 1)
                {
                    middleBorder[currentPosition - 1] = Cross;
                }
                currentPosition += vis.CalculatedWidth;
            }

            visualization.Append(new string(' ', Indent));
            visualization.AppendLine(new string(middleBorder));

            // Add field names
            var fieldLine = new char[totalWidth];
            Array.Fill(fieldLine, ' ');
            fieldLine[0] = Vertical;
            fieldLine[^1] = Vertical;

            currentPosition = 1;
            foreach (var vis in visualizations)
            {
                if (currentPosition > 1)
                {
                    fieldLine[currentPosition - 1] = Vertical;
                }

                var nameStart = currentPosition + (vis.CalculatedWidth - vis.Name.Length) / 2;
                for (int i = 0; i < vis.Name.Length && nameStart + i < fieldLine.Length - 1; i++)
                {
                    fieldLine[nameStart + i] = vis.Name[i];
                }

                currentPosition += vis.CalculatedWidth;
            }

            visualization.Append(new string(' ', Indent));
            visualization.AppendLine(new string(fieldLine));

            // Add bottom border
            var bottomBorder = new char[totalWidth];
            Array.Fill(bottomBorder, Horizontal);
            bottomBorder[0] = BottomLeft;
            bottomBorder[^1] = BottomRight;

            currentPosition = 1;
            foreach (var vis in visualizations)
            {
                if (currentPosition > 1)
                {
                    bottomBorder[currentPosition - 1] = BottomT;
                }
                currentPosition += vis.CalculatedWidth;
            }

            visualization.Append(new string(' ', Indent));
            visualization.AppendLine(new string(bottomBorder));

            return visualization.ToString();
        }
        
    }
}