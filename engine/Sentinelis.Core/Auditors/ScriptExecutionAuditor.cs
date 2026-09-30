using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;

namespace Sentinelis.Core.Auditors;

public class ScriptExecutionAuditor : ISecurityAuditor
{
    public string Name => "ScriptExecution";

    public Task<IEnumerable<AuditViolation>> AuditAsync(AuditContext context, CancellationToken ct = default)
    {
        var violations = context.Dependencies
            .Where(dep => dep.HasInstallScript)
            .Select(dep => new AuditViolation(
                ModuleName: Name,
                PackageName: dep.Name,
                Version: dep.Version,
                Severity: "High",
                Description: $"Package '{dep.Name}' ({dep.Ecosystem}) executes an automated lifecycle script."
            ));

        return Task.FromResult(violations);
    }
}