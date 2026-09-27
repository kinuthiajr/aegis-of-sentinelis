using System;
using System.IO;
using System.Linq;
using Xunit;
using Sentinelis.Core.Utilities;

namespace Sentinelis.Core.Tests;

// <Note> creates a temporary directory structure during test setup and delete 
// it during teardown using xUnit's IDisposable pattern
// <summary>
// DirectoryWalkerTests contains unit tests for the DirectoryWalker utility class.
// It verifies that the FindFiles method correctly identifies files while ignoring specified directories like node_modules.
// </summary>

public class DirectoryWalkerTests : IDisposable
{
    private readonly string _testRoot;

    public DirectoryWalkerTests()
    {
        // Setup: Create an isolated temporary directory for this test run
        _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testRoot);
    }

    public void Dispose()
    {
        // Teardown: Clean up the filesystem after the test finishes
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }

    [Fact]
    public void FindFiles_FindsNestedFiles_ButIgnoresNodeModules()
    {
        // 1. Arrange: Create a mock monorepo structure
        var clientDir = Path.Combine(_testRoot, "client");
        var nodeModulesDir = Path.Combine(_testRoot, "node_modules", "some-pkg");
        var pythonVenvDir = Path.Combine(_testRoot, "venv", "lib");

        Directory.CreateDirectory(clientDir);
        Directory.CreateDirectory(nodeModulesDir);
        Directory.CreateDirectory(pythonVenvDir);

        // Create target files
        File.WriteAllText(Path.Combine(_testRoot, "package-lock.json"), "{}");
        File.WriteAllText(Path.Combine(clientDir, "package-lock.json"), "{}");

        // Create files inside ignored directories (these SHOULD NOT be found)
        File.WriteAllText(Path.Combine(nodeModulesDir, "package-lock.json"), "{}");
        File.WriteAllText(Path.Combine(pythonVenvDir, "package-lock.json"), "{}");

        // 2. Act: Run the walker
        var foundFiles = DirectoryWalker.FindFiles(_testRoot, "package-lock.json").ToList();

        // 3. Assert
        Assert.Equal(2, foundFiles.Count);

        // Ensure it found the root and client files
        Assert.Contains(foundFiles, f => f.Equals(Path.Combine(_testRoot, "package-lock.json")));
        Assert.Contains(foundFiles, f => f.Equals(Path.Combine(clientDir, "package-lock.json")));

        // Ensure it strictly ignored the others
        Assert.DoesNotContain(foundFiles, f => f.Contains("node_modules"));
        Assert.DoesNotContain(foundFiles, f => f.Contains("venv"));
    }
}