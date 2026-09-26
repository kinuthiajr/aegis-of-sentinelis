using System;
using System.Collections.Concurrent;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;
// Assuming RegistryClientFactory is here

using engine.Clients;

namespace Sentinelis.Core.Auditors;

public class AgeGateAuditor : ISecurityAuditor
{
    public string Name => "AgeGate";
    private readonly IRegistryClientFactory _registryFactory;

    public AgeGateAuditor(IRegistryClientFactory registryFactory)
    {
        // <NB> Dependency Inversion Principle: your auditor will no longer care how the factory works, only that it fulfills the interface's contract.
        // This allows for easier testing and future extensibility.

        _registryFactory = registryFactory;
    }

    public async Task<IEnumerable<AuditViolation>> AuditAsync(AuditContext context, CancellationToken ct = default)
    {
        // ConcurrentBag is thread-safe for Parallel.ForEachAsync
        var violations = new ConcurrentBag<AuditViolation>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 15,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(context.Dependencies, parallelOptions, async (dep, token) =>
        {
            if (string.IsNullOrEmpty(dep.Version)) return;

            var client = _registryFactory.GetClient(dep.Ecosystem);
            if (client == null) return; // Unsupported ecosystem, skip

            var publishDate = await client.GetVersionPublishDateAsync(dep.Name, dep.Version);

            if (publishDate.HasValue)
            {
                var ageInHours = (DateTime.UtcNow - publishDate.Value.ToUniversalTime()).TotalHours;

                // Compare against the dynamic QuarantineHours AuditContext
                if (ageInHours < context.QuarantineHours)
                {
                    violations.Add(new AuditViolation(
                        ModuleName: this.Name,
                        PackageName: $"{dep.Ecosystem}:{dep.Name}", // Prefixing ecosystem for clarity
                        Version: dep.Version,
                        Severity: "Warning",
                        Description: $"Package was published {ageInHours:F1} hours ago. Must be older than {context.QuarantineHours} hours."
                    ));
                }
            }
        });

        return violations;
    }
}