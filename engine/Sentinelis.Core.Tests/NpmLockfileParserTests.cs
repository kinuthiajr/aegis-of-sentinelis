using System.IO;
using System.Linq;
using Sentinelis.Modules.Lockfiles.Parsers.Npm;
using Xunit;

namespace Sentinelis.Tests.Parsers;

public class NpmLockfileParserTests
{
  [Fact]
  public void Parse_ExtractsDependenciesAndLifecycleFlags_FromLockfile()
  {
    // Arrange
    var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(tempFolder);

    var sampleLockfileJson = """
        {
          "name": "test-project",
          "version": "1.0.0",
          "packages": {
            "": {},
            "node_modules/axios": {
              "version": "1.2.0",
              "resolved": "https://registry.npmjs.org/axios/-/axios-1.2.0.tgz"
            },
            "node_modules/sqlite3": {
              "version": "5.1.6",
              "resolved": "http://registry.npmjs.org/sqlite3/-/sqlite3-5.1.6.tgz",
              "hasInstallScript": true
            }
          }
        }
        """;

    File.WriteAllText(Path.Combine(tempFolder, "package-lock.json"), sampleLockfileJson);

    try
    {
      var parser = new NpmLockfileParser();

      // Act
      var dependencies = parser.Parse(tempFolder).ToList();

      // Assert
      Assert.Equal(2, dependencies.Count);

      var sqlite = dependencies.First(d => d.Name == "sqlite3");
      Assert.True(sqlite.HasInstallScript);
      Assert.StartsWith("http://", sqlite.ResolvedUrl);

      var axios = dependencies.First(d => d.Name == "axios");
      Assert.False(axios.HasInstallScript);
    }
    finally
    {
      Directory.Delete(tempFolder, true);
    }
  }
}