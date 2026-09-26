using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinelis.Core.Interfaces;

namespace engine.Clients
{
    // Factory class to create instances of IRegistryClient based on the ecosystem
    //<summary>
    // This factory can be extended to support multiple ecosystems like NuGet, Cargo, etc.
    // Returns the correct HTTP client for the given ecosystem, which can then be used to fetch package metadata and check for security vulnerabilities in dependencies.
    //</summary>

    public class RegistryClientFactory
    {
        private readonly NpmRegistryClient _npmClient = new();

        public IRegistryClient? GetClient(string ecosystem)
        {
            return ecosystem.ToLower() switch
            {
                "npm" => _npmClient,
                // Add more ecosystems here as needed
                _ => throw new NotSupportedException($"Ecosystem '{ecosystem}' is not supported.")
            };
        }
    }
}