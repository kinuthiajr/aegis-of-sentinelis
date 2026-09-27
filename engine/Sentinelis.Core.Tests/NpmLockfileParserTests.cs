using System;
using System.IO;
using System.Linq;
using Xunit;
using Sentinelis.Modules.Parsers;

namespace Sentinelis.Core.Tests;

public class NpmLockfileParserTests : IDisposable
{
    private readonly string _testRoot;
    private readonly NpmLockfileParser _parser;

    public NpmLockfileParserTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testRoot);
        _parser = new NpmLockfileParser();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }

    [Fact]
    public void Parse_ValidLockfile_ExtractsDependenciesAndIgnoresRoot()
    {
        // Arrange: Create a mock package-lock.json with standard formatting
        var lockfilePath = Path.Combine(_testRoot, "package-lock.json");
        var json = @"{
            ""packages"": {
                """": { 
                    ""version"": ""1.0.0"" 
                },
                ""node_modules/lodash"": { 
                    ""version"": ""4.17.21"",
                    ""resolved"": ""https://registry.npmjs.org/lodash/-/lodash-4.17.21.tgz""
                },
                ""node_modules/@types/node"": { 
                    ""version"": ""18.16.0""
                }
            }
        }";
        File.WriteAllText(lockfilePath, json);

        // Act
        var dependencies = _parser.Parse(_testRoot).ToList();

        // Assert
        Assert.Equal(2, dependencies.Count); // Should ignore the empty "" root package

        // Verify standard package
        var lodash = dependencies.Single(d => d.Name == "lodash");
        Assert.Equal("4.17.21", lodash.Version);
        Assert.Equal("npm", lodash.Ecosystem);

        // Verify scoped package
        var typesNode = dependencies.Single(d => d.Name == "@types/node");
        Assert.Equal("18.16.0", typesNode.Version);
    }

    [Fact]
    public void Parse_NoLockfileExists_ReturnsEmptyList()
    {
        // Arrange: _testRoot is empty

        // Act
        var dependencies = _parser.Parse(_testRoot).ToList();

        // Assert
        Assert.Empty(dependencies);
    }
    
    [Fact]
    public void Parse_MalformedJson_DoesNotCrash_ReturnsEmpty()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testRoot, "package-lock.json"), "{ invalid_json }");

        // Act
        var dependencies = _parser.Parse(_testRoot).ToList();

        // Assert
        Assert.Empty(dependencies); // The try-catch in the parser should safely handle this
    }
}