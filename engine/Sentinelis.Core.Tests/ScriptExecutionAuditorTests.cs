// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Sentinelis.Core.Auditors;
// using Sentinelis.Core.Models;
// using Xunit;

// namespace Sentinelis.Tests.Auditors;

// public class ScriptExecutionAuditorTests
// {
//     [Fact]
//     public async Task AuditAsync_FlagsDependenciesWithInstallScripts()
//     {
//         // Arrange
//         var safeDep = new DependencyInfo("npm", "chalk", "5.0.0", HasInstallScript: false);
//         var maliciousDep = new DependencyInfo("npm", "core-js", "3.6.5", HasInstallScript: true);

//         var context = new AuditContext("/test", new List<DependencyInfo> { safeDep, maliciousDep });
//         var auditor = new ScriptExecutionAuditor();

//         // Act
//         var violations = (await auditor.AuditAsync(context)).ToList();

//         // Assert
//         Assert.Single(violations);
//         Assert.Equal("core-js", violations[0].PackageName);
//         Assert.Equal("High", violations[0].Severity);
//     }
// }