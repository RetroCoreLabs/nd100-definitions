using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NDGen.Core.Models
{
    public sealed class IODeviceSpec
    {
        [JsonPropertyName("device_types")]
        public Dictionary<string, IODeviceType> DeviceTypes { get; set; } = new();

        [JsonPropertyName("device_instances")]
        public List<IODeviceInstance> DeviceInstances { get; set; } = new();
    }

    public sealed class IODeviceType
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("register_count")]
        public int RegisterCount { get; set; }

        [JsonPropertyName("registers")]
        public List<IORegisterDefinition> Registers { get; set; } = new();
    }

    public sealed class IORegisterDefinition
    {
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("label_suffix")]
        public string LabelSuffix { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("bit_fields")]
        public List<IOBitField>? BitFields { get; set; }
    }

    public sealed class IOBitField
    {
        [JsonPropertyName("bits")]
        public string Bits { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    public sealed class IODeviceInstance
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("prefix")]
        public string Prefix { get; set; } = "";

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "";

        [JsonPropertyName("base_address_hex")]
        public string BaseAddressHex { get; set; } = "";

        [JsonPropertyName("octal_range")]
        public string OctalRange { get; set; } = "";

        [JsonPropertyName("irq_level")]
        public string IrqLevel { get; set; } = "";

        [JsonPropertyName("ident_octal")]
        public string IdentOctal { get; set; } = "";
    }
}
