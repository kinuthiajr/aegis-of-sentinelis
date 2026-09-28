using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sentinelis.Cli.Formatters;

public class SarifLog
{
    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "2.1.0";

    [JsonPropertyName("runs")]
    public List<SarifRun> Runs { get; set; } = new();
}

public class SarifRun
{
    [JsonPropertyName("tool")]
    public SarifTool Tool { get; set; } = new();

    [JsonPropertyName("results")]
    public List<SarifResult> Results { get; set; } = new();
}

public class SarifTool
{
    [JsonPropertyName("driver")]
    public SarifDriver Driver { get; set; } = new();
}

public class SarifDriver
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Sentinelis";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("rules")]
    public List<SarifRule> Rules { get; set; } = new();
}

public class SarifRule
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("shortDescription")]
    public SarifMultiformatMessageString ShortDescription { get; set; } = new();
}

public class SarifResult
{
    [JsonPropertyName("ruleId")]
    public string RuleId { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; set; } = "error"; // "error", "warning", or "note"

    [JsonPropertyName("message")]
    public SarifMultiformatMessageString Message { get; set; } = new();

    [JsonPropertyName("locations")]
    public List<SarifLocation> Locations { get; set; } = new();
}

public class SarifMultiformatMessageString
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class SarifLocation
{
    [JsonPropertyName("physicalLocation")]
    public SarifPhysicalLocation PhysicalLocation { get; set; } = new();
}

public class SarifPhysicalLocation
{
    [JsonPropertyName("artifactLocation")]
    public SarifArtifactLocation ArtifactLocation { get; set; } = new();
}

public class SarifArtifactLocation
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

// Native AOT Source Generator Context
[JsonSerializable(typeof(SarifLog))]
public partial class SarifJsonContext : JsonSerializerContext
{
}