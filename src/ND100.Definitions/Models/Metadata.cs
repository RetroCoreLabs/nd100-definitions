using System.Collections.Generic;

namespace NDGen.Core.Models
{
    public class Flag
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
    }

    public class Metadata
    {
        public Metadata()
        {
            Registers = new List<Register>();
            Flags = new List<Flag>();
        }

        public required string CpuName { get; init; }
        public int WordSize { get; init; }
        public List<Register> Registers { get; init; }
        public List<Flag> Flags { get; init; }
    }
} 