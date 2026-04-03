using System.Collections.Generic;
using NDGen.Core.Models;

namespace NDGen.Core.Utils
{
    public class FieldVisualization
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int StartBit { get; set; }
        public int EndBit { get; set; }
        public List<SubfieldVisualization> Subfields { get; set; } = new();
        public int CalculatedWidth { get; set; }
        public int NameWidth { get; set; }
        public int BitWidth { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public Dictionary<int, int> BitPositions { get; set; } = new();
        public InstructionOperand Operand { get; set; } = null!;

        private const int MinSpacing = 4; // Space between bit numbers
        private const int Padding = 2;    // Space around field names

        public FieldVisualization(InstructionOperand operand)
        {
            Operand = operand;
            Name = operand.Name;
            Type = operand.Type ?? string.Empty;
            var bits = operand.Bits.Split('-');
            StartBit = int.Parse(bits[0]);
            EndBit = bits.Length > 1 ? int.Parse(bits[1]) : int.Parse(bits[0]);
            Subfields = new List<SubfieldVisualization>();
            BitPositions = new Dictionary<int, int>();

            if (operand.Subfields != null)
            {
                foreach (var sf in operand.Subfields)
                {
                    Subfields.Add(new SubfieldVisualization(sf));
                }
            }

            // Calculate widths immediately
            CalculateWidths();
        }

        public void CalculateWidths()
        {
            // Calculate bit width based on the number of bits
            int bitCount = StartBit - EndBit + 1;
            BitWidth = bitCount * MinSpacing;

            // Calculate name width with padding
            NameWidth = Name.Length + (Padding * 2);

            // For addressing mode, ensure width accommodates subfields
            if (Type?.ToLower() == "addressing_mode" && Operand.Subfields != null)
            {
                // Calculate width needed for subfields (X I B)
                int subfieldWidth = Operand.Subfields.Sum(sf => sf.Name.Length) + 
                                  (Operand.Subfields.Count - 1) * 2; // Add space between subfields
                subfieldWidth += Padding * 2;
                NameWidth = Math.Max(NameWidth, subfieldWidth);
            }

            // Use the larger of bit width or name width
            CalculatedWidth = Math.Max(BitWidth, NameWidth);

            // Calculate bit positions
            int currentPos = 0;
            for (int bit = StartBit; bit >= EndBit; bit--)
            {
                BitPositions[bit] = currentPos;
                currentPos += MinSpacing;
            }
        }

        public bool OverlapsWith(FieldVisualization other)
        {
            return !(EndBit < other.StartBit || StartBit > other.EndBit);
        }
    }

    public class SubfieldVisualization
    {
        public string Name { get; set; } = string.Empty;
        public int BitPosition { get; set; }
        public int CalculatedPosition { get; set; }

        public SubfieldVisualization()
        {
        }

        public SubfieldVisualization(OperandSubfield subfield)
        {
            Name = subfield.Name;
            BitPosition = subfield.Position;
        }
    }

    public class FieldLayoutCalculator
    {
        private const int MinSpacing = 4; // Space between bit numbers
        private const int Padding = 2;    // Space around field names
        private const int BitNumberWidth = 2; // Width of each bit number
        private const int DividerWidth = 1;   // Width of the divider

        public List<FieldVisualization> CalculateLayouts(IEnumerable<InstructionOperand> operands)
        {
            var visualizations = new List<FieldVisualization>();
            foreach (var op in operands)
            {
                var vis = new FieldVisualization(op);
                visualizations.Add(vis);
            }
            
            // Sort by start bit to ensure proper ordering
            visualizations.Sort((a, b) => b.StartBit.CompareTo(a.StartBit));

            // Check for overlapping bit ranges
            for (int i = 0; i < visualizations.Count - 1; i++)
            {
                if (visualizations[i].OverlapsWith(visualizations[i + 1]))
                {
                    throw new InvalidOperationException($"Overlapping bit ranges detected between {visualizations[i].Name} and {visualizations[i + 1].Name}");
                }
            }

            // Calculate positions
            int currentPosition = 0;
            foreach (var vis in visualizations)
            {
                vis.StartPosition = currentPosition;
                vis.EndPosition = currentPosition + vis.CalculatedWidth - 1;
                currentPosition = vis.EndPosition + DividerWidth + 1;
            }

            return visualizations;
        }
    }
} 