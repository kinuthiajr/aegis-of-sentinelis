using System;
using System.IO;
using Xunit;
using Sentinelis.Cli.Commands;
using Sentinelis.Core.Loaders;

namespace Sentinelis.Core.Tests.Commands;

public class TrustCommandHandlerTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _manifestPath;

    public TrustCommandHandlerTests()
    {
        // Use isolated directories to prevent parallel test execution collisions
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _manifestPath = Path.Combine(_tempDirectory, "sentinelis-trust.lock");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void Execute_WhenFileDoesNotExist_CreatesManifestWithPackageAndRule()
    {
        // Act
        TrustCommandHandler.Execute("test-pkg", "ScriptExecution", _tempDirectory);

        // Assert
        Assert.True(File.Exists(_manifestPath), "Manifest file should be created.");

        var manifest = TrustManifestLoader.Load(_tempDirectory);
        Assert.True(manifest.IsAllowed("test-pkg", "ScriptExecution"), "Package and rule should be allowed.");
    }

    [Fact]
    public void Execute_WhenFileExists_AddsNewRuleToExistingPackage()
    {
        // Arrange - Create manifest with one rule
        TrustCommandHandler.Execute("test-pkg", "InsecureRegistry", _tempDirectory);

        // Act - Add a second rule to the same package
        TrustCommandHandler.Execute("test-pkg", "ScriptExecution", _tempDirectory);

        // Assert
        var manifest = TrustManifestLoader.Load(_tempDirectory);
        Assert.True(manifest.IsAllowed("test-pkg", "InsecureRegistry"), "Original rule should be preserved.");
        Assert.True(manifest.IsAllowed("test-pkg", "ScriptExecution"), "New rule should be added.");
    }

    [Fact]
    public void Execute_WhenFileExists_AddsNewPackagePreservingExisting()
    {
        // Arrange - Create manifest with one package
        TrustCommandHandler.Execute("existing-pkg", "AgeGate", _tempDirectory);

        // Act - Add an entirely new package
        TrustCommandHandler.Execute("new-pkg", "ScriptExecution", _tempDirectory);

        // Assert
        var manifest = TrustManifestLoader.Load(_tempDirectory);
        Assert.True(manifest.IsAllowed("existing-pkg", "AgeGate"), "Original package should be preserved.");
        Assert.True(manifest.IsAllowed("new-pkg", "ScriptExecution"), "New package should be added.");
    }
}