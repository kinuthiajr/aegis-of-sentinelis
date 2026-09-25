using System.Text.Json.Serialization;


namespace Sentinelis.Modules.Lockfiles.Dtos
{

    // Pre-generated JSON serialization context for NpmLockfileDtos. 
    // This is used to avoid reflection-based serialization at runtime, improving performance.

    [JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(NpmLockfileDtos))]
    public partial class NpmLockfileJsonContext : JsonSerializerContext;
}