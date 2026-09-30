using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sentinelis.Modules.Lockfiles.Parsers.Npm.Models;

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
    public string? Resolved { get; set; }

    [JsonPropertyName("hasInstallScript")]
    public bool? HasInstallScript { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(NpmLockfile))]
[JsonSerializable(typeof(NpmPackageInfo))]
public partial class NpmLockfileJsonContext : JsonSerializerContext;