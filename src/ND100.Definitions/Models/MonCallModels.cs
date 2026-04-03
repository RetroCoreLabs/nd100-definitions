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
    }
}
