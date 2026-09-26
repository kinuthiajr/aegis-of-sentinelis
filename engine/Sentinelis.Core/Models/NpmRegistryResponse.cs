using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Sentinelis.Cli.Models;

// defines the shape of the NPM registry data and the specific JSON required for Native AOT to parse it.

public class NpmRegistryResponse
{
    // The "time" dictionary in NPM registry contains publish dates for all versions
    [JsonPropertyName("time")]
    public Dictionary<string, string> Time { get; set; } = new();
}

// Native AOT requires this context to generate serialization code at compile-time
[JsonSerializable(typeof(NpmRegistryResponse))]
public partial class NpmRegistryJsonContext : JsonSerializerContext
{
}