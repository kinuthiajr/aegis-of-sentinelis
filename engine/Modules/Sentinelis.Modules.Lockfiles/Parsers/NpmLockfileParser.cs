using System.Text.Json;
using Sentinelis.Core.Models;
using Sentinelis.Modules.Lockfiles.Dtos;

namespace Sentinelis.Modules.Lockfiles.Parsers;

// <summary>
// Parses and audits npm lockfiles (package-lock.json) for security violations.
// </summary>

public class NpmLockfileParser
{
    private static readonly string[] LifecycleHooks = ["preinstall", "install", "postinstall"];

    public async Task<List<AuditViolation>> ParseAndAuditAsync(string lockfilePath, CancellationToken ct = default)
    {
        var violations = new List<AuditViolation>();

        if (!File.Exists(lockfilePath))
            return violations;

        await using var stream = File.OpenRead(lockfilePath);

        // Native AOT Deserialization using Source Generator Context
        var lockfile = await JsonSerializer.DeserializeAsync(
            stream,
            NpmLockfileJsonContext.Default.NpmLockfileDtos,
            cancellationToken: ct);

        if (lockfile == null)
            return violations;

        // 1. Audit v2/v3 Lockfile format ("packages")
        if (lockfile.Packages != null)
        {
            foreach (var (pkgPath, pkg) in lockfile.Packages)
            {
                // Skip root workspace package ("")
                if (string.IsNullOrEmpty(pkgPath)) continue;

                var packageName = ExtractPackageName(pkgPath);
                var version = pkg.Version ?? "unknown";

                if (pkg.HasInstallScript || ContainsLifecycleScript(pkg.Scripts))
                {
                    violations.Add(new AuditViolation(
                        ModuleName: "Lockfiles",
                        PackageName: packageName,
                        Version: version,
                        Severity: "High",
                        Description: $"Package '{packageName}@{version}' contains an active lifecycle install script."
                    ));
                }
            }
        }
        // 2. Fallback to v1 Lockfile format ("dependencies")
        else if (lockfile.Dependencies != null)
        {
            foreach (var (pkgName, dep) in lockfile.Dependencies)
            {
                if (dep.HasInstallScript)
                {
                    violations.Add(new AuditViolation(
                        ModuleName: "Lockfiles",
                        PackageName: pkgName,
                        Version: dep.Version ?? "unknown",
                        Severity: "High",
                        Description: $"Package '{pkgName}@{dep.Version}' flags active install scripts."
                    ));
                }
            }
        }

        return violations;
    }

    private static string ExtractPackageName(string packagePath)
    {
        // Converts "node_modules/foo/node_modules/bar" -> "bar"
        var lastNodeModulesIndex = packagePath.LastIndexOf("node_modules/", StringComparison.Ordinal);
        return lastNodeModulesIndex != -1
            ? packagePath[(lastNodeModulesIndex + "node_modules/".Length)..]
            : packagePath;
    }

    private static bool ContainsLifecycleScript(Dictionary<string, string>? scripts)
    {
        if (scripts == null) return false;
        return scripts.Keys.Any(hook => LifecycleHooks.Contains(hook, StringComparer.OrdinalIgnoreCase));
    }
}