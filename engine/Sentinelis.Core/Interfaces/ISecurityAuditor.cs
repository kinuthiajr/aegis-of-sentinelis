
using Sentinelis.Core.Models;

namespace Sentinelis.Core.Interfaces
{
    public interface ISecurityAuditor
    {
        string Name { get; }
        // Return a list of security violation. If empty, the check passed
        Task<IEnumerable<AuditViolation>> AuditAsync(AuditContext context, CancellationToken ct = default);
    }
}