using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sentinelis.Core.Models;

public class SentinelisConfig
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "enforce"; // "enforce" or "audit"

    [JsonPropertyName("quarantineHours")]
    public int QuarantineHours { get; set; } = 24;

    [JsonPropertyName("rules")]
    public Dictionary<string, bool> Rules { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("allowlist")]
    public List<string> Allowlist { get; set; } = new();
}

[JsonSerializable(typeof(SentinelisConfig))]
public partial class ConfigJsonContext : JsonSerializerContext { }