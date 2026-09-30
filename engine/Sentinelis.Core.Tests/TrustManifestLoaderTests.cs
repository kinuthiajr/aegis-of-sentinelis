using System;
using System.IO;
using Xunit;
using Sentinelis.Core.Loaders;
using Sentinelis.Core.Models;

namespace Sentinelis.Core.Tests.Loaders;

public class TrustManifestLoaderTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _manifestPath;

    public TrustManifestLoaderTests()
    {
        // Create an isolated temp directory for each test run to avoid race conditions
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _manifestPath = Path.Combine(_tempDirectory, "sentinelis-trust.lock");
    }

    public void Dispose()
    {
        // Cleanup temp files after the test completes
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ReturnsEmptyManifest()
    {
        // Act
        var manifest = TrustManifestLoader.Load(_tempDirectory);

        // Assert
        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Trusted); // Default instantiation state
        Assert.Empty(manifest.Trusted);
        Assert.False(manifest.IsAllowed("any-package", "any-rule"));
    }

    [Fact]
    public void Load_WhenFileIsValid_ParsesRulesCorrectly()
    {
        // Arrange
        var validJson = @"{
            ""trusted"": {
                ""malicious-hook"": {
                    ""allowedRules"": [""ScriptExecution""]
                },
                ""internal-pkg"": {
                    ""allowedRules"": [""InsecureRegistry"", ""AgeGate""]
                }
            }
        }";
        File.WriteAllText(_manifestPath, validJson);

        // Act
        var manifest = TrustManifestLoader.Load(_tempDirectory);

        // Assert
        Assert.NotNull(manifest.Trusted);

        // malicious-hook checks
        Assert.True(manifest.IsAllowed("malicious-hook", "ScriptExecution"));
        Assert.False(manifest.IsAllowed("malicious-hook", "InsecureRegistry")); // Not allowed for this rule

        // internal-pkg checks
        Assert.True(manifest.IsAllowed("internal-pkg", "AgeGate"));

        // Unknown package check
        Assert.False(manifest.IsAllowed("unknown-package", "ScriptExecution"));
    }

    [Fact]
    public void Load_WhenJsonIsMalformed_HandlesGracefullyWithoutThrowing()
    {
        // Arrange: Write corrupted JSON
        var malformedJson = "{ corrupted: json: structure [}";
        File.WriteAllText(_manifestPath, malformedJson);

        // Act
        var manifest = TrustManifestLoader.Load(_tempDirectory);

        // Assert
        Assert.NotNull(manifest);
        Assert.False(manifest.IsAllowed("any-package", "any-rule"));
    }

    [Fact]
    public void Load_WhenJsonIsEmptyObject_ParsesSafely()
    {
        // Arrange
        File.WriteAllText(_manifestPath, "{}");

        // Act
        var manifest = TrustManifestLoader.Load(_tempDirectory);

        // Assert
        Assert.NotNull(manifest);
        Assert.False(manifest.IsAllowed("any-package", "any-rule"));
    }
}