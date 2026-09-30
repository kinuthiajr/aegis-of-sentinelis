using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Sentinelis.Core.Loaders;
using Sentinelis.Core.Models;

namespace Sentinelis.Cli.Commands;

// class will load the existing manifest (or create a new one), add the allowed rule, and write it back.

public static class TrustCommandHandler
{
    public static void Execute(string packageName, string ruleName, string targetPath)
    {
        var manifestPath = Path.Combine(targetPath, "sentinelis-trust.lock");
        
        // 1. Load existing or get empty manifest
        var manifest = TrustManifestLoader.Load(targetPath);
        
        manifest.Trusted ??= new Dictionary<string, TrustedPackage>();

        // 2. Ensure package exists in dictionary
        if (!manifest.Trusted.TryGetValue(packageName, out var pkg))
        {
            pkg = new TrustedPackage { AllowedRules = new List<string>() };
            manifest.Trusted[packageName] = pkg;
        }

        // 3. Ensure rule is added
        pkg.AllowedRules ??= new List<string>();
        if (!pkg.AllowedRules.Contains(ruleName))
        {
            pkg.AllowedRules.Add(ruleName);
        }

        // 4. Serialize back to disk with pretty-printing (Native AOT compliant)
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = TrustManifestJsonContext.Default
        };

        var json = JsonSerializer.Serialize(manifest, options);
        File.WriteAllText(manifestPath, json);

        Console.WriteLine($"[SUCCESS] Added '{packageName}' -> '{ruleName}' to {manifestPath}");
    }
}