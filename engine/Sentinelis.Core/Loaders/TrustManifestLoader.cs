using System;
using System.IO;
using System.Text.Json;
using Sentinelis.Core.Models;

namespace Sentinelis.Core.Loaders;

// safely read and deserialize the manifest, falling back to an empty manifest if the file doesn't exist.


public static class TrustManifestLoader
{
    public static TrustManifest Load(string targetPath)
    {
        var manifestPath = Path.Combine(targetPath, "sentinelis-trust.lock");

        if (!File.Exists(manifestPath))
        {
            return new TrustManifest();
            // Return empty manifest, block nothing
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize(json, TrustManifestJsonContext.Default.TrustManifest);
            return manifest ?? new TrustManifest();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to parse sentinelis-trust.lock: {ex.Message}");
            return new TrustManifest();
        }
    }
}