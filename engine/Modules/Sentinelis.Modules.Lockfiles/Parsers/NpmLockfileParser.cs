using System.Text.Json;
using Sentinelis.Core.Interfaces;
using Sentinelis.Modules.Lockfiles.Dtos;


namespace Sentinelis.Modules.Parsers;

// <summary>
// NpmLockfileParser is responsible for parsing npm lockfiles (package-lock.json) to extract dependency information.
// It implements the IDependencyParser interface, allowing it to be used in a polymorphic way with other dependency parsers.
// The parser is designed to be efficient, skipping over large directories like node_modules and .git
// </summary>

public class NpmLockfileParser : IDependencyParser
{
    // A list of folders the engine should completely ignore to save time and memory
    private static readonly HashSet<string> _ignoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", ".git", "bin", "obj", "dist", "build", ".idea", ".vscode"
    };

    public IEnumerable<DependencyInfo> Parse(string targetPath)
    {
        var allDependencies = new List<DependencyInfo>();
        var lockfiles = FindFiles(targetPath, "package-lock.json");

        // Parse every lockfile found in the repository
        foreach (var lockfilePath in lockfiles)
        {
            allDependencies.AddRange(ParseSingleFile(lockfilePath));
        }

        return allDependencies;
    }

    // 1. Smart Directory Search (Skips node_modules!)
    // <Note> Speed: By using a Queue and checking against _ignoredDirectories, it explicitly skips node_modules
    private IEnumerable<string> FindFiles(string rootPath, string targetFileName)
    {
        var foundFiles = new List<string>();
        var directoriesToSearch = new Queue<string>();
        directoriesToSearch.Enqueue(rootPath);

        while (directoriesToSearch.Count > 0)
        {
            var currentDir = directoriesToSearch.Dequeue();
            var dirName = Path.GetFileName(currentDir);

            // Skip heavy build directories and node_modules
            if (_ignoredDirectories.Contains(dirName))
                continue;

            // Check if the lockfile exists in this specific folder
            var possibleFile = Path.Combine(currentDir, targetFileName);
            if (File.Exists(possibleFile))
            {
                foundFiles.Add(possibleFile);
            }

            // Queue up all subdirectories to check them next
            try
            {
                foreach (var subDir in Directory.GetDirectories(currentDir))
                {
                    directoriesToSearch.Enqueue(subDir);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore folders we don't have permission to read
            }
        }

        return foundFiles;
    }

    // 2. The JSON Parsing logic
    private IEnumerable<DependencyInfo> ParseSingleFile(string lockfilePath)
    {
        try
        {
            using var stream = File.OpenRead(lockfilePath);
            var lockfile = JsonSerializer.Deserialize(stream, NpmLockfileJsonContext.Default.NpmLockfileDtos);

            if (lockfile?.Packages == null) return Enumerable.Empty<DependencyInfo>();

            var dependencies = new List<DependencyInfo>();

            foreach (var package in lockfile.Packages)
            {
                var path = package.Key;
                if (string.IsNullOrEmpty(path)) continue;

                var name = path.StartsWith("node_modules/") ? path.Substring(13) : path;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(package.Value.Version))
                {
                    dependencies.Add(new DependencyInfo(
                        Name: name,
                        Version: package.Value.Version,
                        Ecosystem: "npm"
                    ));
                }
            }

            return dependencies;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to parse {lockfilePath}: {ex.Message}");
            return Enumerable.Empty<DependencyInfo>();
        }
    }
}