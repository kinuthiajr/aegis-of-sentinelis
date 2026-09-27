using System.Text.Json.Serialization;

namespace Sentinelis.Modules.Parsers.Models;

public class NpmLockfile
{
    [JsonPropertyName("packages")]
    public Dictionary<string, NpmPackageInfo>? Packages { get; set; }
}

public class NpmPackageInfo
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("resolved")]
    public string? Resolved { get; set; } // Added for structural URL checks
}

[JsonSerializable(typeof(NpmLockfile))]
internal partial class NpmLockfileJsonContext : JsonSerializerContext
{
}