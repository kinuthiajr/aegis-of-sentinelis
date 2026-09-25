using System.Text.Json.Serialization;
using Sentinelis.Core.Models;

namespace Sentinelis.Cli.Formatters;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(AuditViolation))]
[JsonSerializable(typeof(IEnumerable<AuditViolation>))]
[JsonSerializable(typeof(List<AuditViolation>))]
public partial class AuditResultJsonContext : JsonSerializerContext
{ }