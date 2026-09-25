using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sentinelis.Modules.Lockfiles.Dtos
{
    public class NpmLockfileDtos
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("lockfileVersion")]
        public int LockfileVersion { get; set; }

        // v2/v3 format: "packages": { "node_modules/foo": { ... } }
        [JsonPropertyName("packages")]
        public Dictionary<string, NpmPackageDto>? Packages { get; set; }

        // v1 fallback format: "dependencies": { "foo": { ... } }
        [JsonPropertyName("dependencies")]
        public Dictionary<string, NpmDependencyDto>? Dependencies { get; set; }
    }

    public class NpmPackageDto
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("hasInstallScript")]
        public bool HasInstallScript { get; set; }

        [JsonPropertyName("scripts")]
        public Dictionary<string, string>? Scripts { get; set; }
    }

    public class NpmDependencyDto
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("hasInstallScript")]
        public bool HasInstallScript { get; set; }
    }
}