using System.Collections.Generic;

namespace NDGen.Core.Models
{
    public class InstructionSet
    {
        public InstructionSet()
        {
            Metadata = new Metadata
            {
                CpuName = "Default",
                WordSize = 16
            };
            Instructions = new List<InstructionDefinition>();
        }

        public Metadata Metadata { get; set; }
        public List<InstructionDefinition> Instructions { get; set; }
    }
} 