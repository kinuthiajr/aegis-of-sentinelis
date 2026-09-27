using System;
using System.Collections.Generic;
using System.IO;

namespace Sentinelis.Core.Utilities;

//<summary> 
// A directory walker to handle monorepo with multiple languages and ecosystems. 
// It upgrades the parser to perform a recursive directory search—but 
// must do it safely so we don't accidentally scan massive junk folders like node_modules or .git
// <Note> walker is completely blind to what a "language" is—and that is its superpower. 
// It is just a highly optimized, extremely fast file-finder. Directory.GetFiles(..., SearchOption.AllDirectories), 
// C# would waste time scanning millions of files inside node_modules, which would freeze the pipeline.
// <note>
// </summary>

public static class DirectoryWalker
{
    // A master list of heavy, generated, or virtual directories across all ecosystems
    private static readonly HashSet<string> _ignoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        // General / C#
        ".git", "bin", "obj", ".idea", ".vscode",
        // Node
        "node_modules", "dist", "build",
        // Python
        "venv", ".env", "__pycache__",
        // Rust
        "target"
    };

    public static IEnumerable<string> FindFiles(string rootPath, params string[] targetFileNames)
    {
        var foundFiles = new List<string>();
        var directoriesToSearch = new Queue<string>();
        directoriesToSearch.Enqueue(rootPath);

        while (directoriesToSearch.Count > 0)
        {
            var currentDir = directoriesToSearch.Dequeue();
            var dirName = Path.GetFileName(currentDir);

            // Skip heavy build directories across all languages
            if (_ignoredDirectories.Contains(dirName))
                continue;

            // Check if any of the target files exist in this folder
            foreach (var target in targetFileNames)
            {
                var possibleFile = Path.Combine(currentDir, target);
                if (File.Exists(possibleFile))
                {
                    foundFiles.Add(possibleFile);
                }
            }

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
}