
using Sentinelis.Modules.Lockfiles.Parsers;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;

namespace Sentinelis.Modules.Lockfiles.Auditors
{

    // <summary>
    // security-auditing component that checks an npm lockfile—specifically package-lock.json—for dependency-related security problems.
    // </summary>

    public class NpmLockfileAuditor : ISecurityAuditor
    {

        public string Name => "Lockfiles";

        private readonly NpmLockfileParser _npmParser = new();

        public async Task<IEnumerable<AuditViolation>> AuditAsync(AuditContext context, CancellationToken ct = default)
        {
            var violations = new List<AuditViolation>();
            var lockfilePath = Path.Combine(context.TargetPath, "package-lock.json");

            if (File.Exists(lockfilePath))
            {
                var npmViolations = await _npmParser.ParseAndAuditAsync(lockfilePath, ct);
                violations.AddRange(npmViolations);
            }

            return violations;
        }

    }
}