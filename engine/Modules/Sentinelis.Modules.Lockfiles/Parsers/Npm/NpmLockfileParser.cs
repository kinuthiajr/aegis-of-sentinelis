using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;
using Sentinelis.Modules.Lockfiles.Parsers.Npm.Models;

namespace Sentinelis.Modules.Lockfiles.Parsers.Npm;

public class NpmLockfileParser : IDependencyParser
{
    public IEnumerable<DependencyInfo> Parse(string targetPath)
    {
        var lockfilePath = Path.Combine(targetPath, "package-lock.json");
        if (!File.Exists(lockfilePath))
        {
            yield break;
        }

        var json = File.ReadAllText(lockfilePath);
        var lockfile = JsonSerializer.Deserialize(json, NpmLockfileJsonContext.Default.NpmLockfile);

        if (lockfile?.Packages == null)
        {
            yield break;
        }

        foreach (var (key, pkg) in lockfile.Packages)
        {
            // Skip root package entry ("")
            if (string.IsNullOrEmpty(key)) continue;

            // Normalize package name (strip "node_modules/")
            var name = key.StartsWith("node_modules/") ? key["node_modules/".Length..] : key;

            yield return new DependencyInfo(
                Ecosystem: "npm",
                Name: name,
                Version: pkg.Version ?? "0.0.0",
                ResolvedUrl: pkg.Resolved,
                HasInstallScript: pkg.HasInstallScript ?? false
            );
        }
    }
}