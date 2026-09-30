using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sentinelis.Core.Models;

// This class represents the structure of the trust manifest JSON file, 
// which defines trusted packages and their allowed rules.
// The TrustManifest is used by the auditors to determine if a package is trusted and 
// which rules are allowed for it.

public class TrustManifest
{
    [JsonPropertyName("trusted")]
    public Dictionary<string, TrustedPackage>? Trusted { get; set; } = new();

    // Helper method to keep auditor logic clean
    public bool IsAllowed(string packageName, string ruleName)
    {
        if (Trusted == null || !Trusted.TryGetValue(packageName, out var pkg))
        {
            return false;
        }

        return pkg.AllowedRules?.Contains(ruleName) == true;
    }
}

public class TrustedPackage
{
    [JsonPropertyName("allowedRules")]
    public List<string>? AllowedRules { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(TrustManifest))]
[JsonSerializable(typeof(TrustedPackage))]
public partial class TrustManifestJsonContext : JsonSerializerContext;