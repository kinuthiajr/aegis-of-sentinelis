using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinelis.Core.Auditors;
using Sentinelis.Core.Models;
using Xunit;

namespace Sentinelis.Tests.Auditors;

public class InsecureRegistryAuditorTests
{
    [Fact]
    public async Task AuditAsync_FlagsInsecureHttpRegistries()
    {
        // Arrange
        var safeDep = new DependencyInfo("npm", "lodash", "4.17.21", ResolvedUrl: "https://registry.npmjs.org/lodash/-/lodash-4.17.21.tgz");
        var unsafeDep = new DependencyInfo("npm", "express", "4.17.1", ResolvedUrl: "http://registry.npmjs.org/express/-/express-4.17.1.tgz");

        var context = new AuditContext("/test", new List<DependencyInfo> { safeDep, unsafeDep });
        var auditor = new InsecureRegistryAuditor();

        // Act
        var violations = (await auditor.AuditAsync(context)).ToList();

        // Assert
        Assert.Single(violations);
        Assert.Equal("express", violations[0].PackageName);
        Assert.Equal("InsecureRegistry", violations[0].ModuleName);
    }

    [Fact]
    public async Task AuditAsync_ReturnsNoViolations_WhenAllRegistriesAreHttps()
    {
        // Arrange
        var safeDep = new DependencyInfo("npm", "react", "18.2.0", ResolvedUrl: "https://registry.npmjs.org/react/-/react-18.2.0.tgz");
        var context = new AuditContext("/test", new List<DependencyInfo> { safeDep });
        var auditor = new InsecureRegistryAuditor();

        // Act
        var violations = await auditor.AuditAsync(context);

        // Assert
        Assert.Empty(violations);
    }
}