using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NDGen.Core.Models
{
    public sealed class MonCallSpec
    {
        [JsonPropertyName("mon_calls")]
        public List<MonCallDefinition> MonCalls { get; set; } = new();
    }

    public sealed class MonCallDefinition
    {
        [JsonPropertyName("octal")]
        public string Octal { get; set; } = "";

        [JsonPropertyName("decimal")]
        public int Decimal { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("nd100")]
        public bool Nd100 { get; set; }

        [JsonPropertyName("nd500")]
        public bool Nd500 { get; set; }

        /// <summary>
        /// Where the calling program continues, as word offsets from the MON instruction, when
        /// SINTRAN has more than one return for this call (error return at +1, normal "skip"
        /// return at +2; CopyPage also +3). Empty when the call always returns to +1.
        /// </summary>
        [JsonPropertyName("exits")]
        public List<MonCallExit> Exits { get; set; } = new();

        /// <summary>Where the exits were read: manual and example.</summary>
        [JsonPropertyName("exits_source")]
        public string ExitsSource { get; set; } = "";
    }

    /// <summary>One place a monitor call can return to.</summary>
    public sealed class MonCallExit
    {
        /// <summary>Word offset from the MON instruction.</summary>
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        /// <summary>When the call returns there.</summary>
        [JsonPropertyName("when")]
        public string When { get; set; } = "";
    }
}
