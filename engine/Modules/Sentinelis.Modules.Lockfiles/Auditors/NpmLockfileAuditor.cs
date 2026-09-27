using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;
using Sentinelis.Core.Utilities;
using Sentinelis.Modules.Lockfiles.Dtos;
using Sentinelis.Modules.Parsers.Models;

namespace Sentinelis.Modules.Lockfiles.Auditors;

public class NpmLockfileAuditor : ISecurityAuditor
{
    public string Name => "NpmLockfile";

    public Task<IEnumerable<AuditViolation>> AuditAsync(AuditContext context, CancellationToken ct = default)
    {
        var violations = new List<AuditViolation>();

        // Find all lockfiles recursively (ignoring node_modules)
        var lockfiles = DirectoryWalker.FindFiles(context.TargetPath, "package-lock.json");

        foreach (var lockfilePath in lockfiles)
        {
            try
            {
                using var stream = File.OpenRead(lockfilePath);
                var lockfile = JsonSerializer.Deserialize(stream, NpmLockfileJsonContext.Default.NpmLockfileDtos);

                if (lockfile?.Packages == null) continue;

                foreach (var (path, package) in lockfile.Packages)
                {
                    if (string.IsNullOrEmpty(path)) continue;

                    var packageName = path.StartsWith("node_modules/") ? path[13..] : path;

                    // Structural Check 1: Flag insecure HTTP registry URLs
                    if (package.Resolved != null && package.Resolved.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add(new AuditViolation(
                            ModuleName: "NpmLockfile",
                            PackageName: packageName,
                            Version: package.Version ?? "unknown",
                            Severity: "High",
                            Description: $"Package '{packageName}' resolves over insecure HTTP ({package.Resolved})."
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to audit lockfile '{lockfilePath}': {ex.Message}");
            }
        }

        return Task.FromResult<IEnumerable<AuditViolation>>(violations);
    }
}