using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;

namespace Sentinelis.Core.Auditors;

public class InsecureRegistryAuditor : ISecurityAuditor
{
    public string Name => "InsecureRegistry";

    public Task<IEnumerable<AuditViolation>> AuditAsync(AuditContext context, CancellationToken ct = default)
    {
        var violations = context.Dependencies
            .Where(dep => dep.ResolvedUrl != null &&
                          dep.ResolvedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            .Select(dep => new AuditViolation(
                ModuleName: Name,
                PackageName: dep.Name,
                Version: dep.Version,
                Severity: "Medium",
                Description: $"Package '{dep.Name}' ({dep.Ecosystem}) resolves over an unencrypted registry URL: '{dep.ResolvedUrl}'"
            ));

        return Task.FromResult(violations);
    }
}