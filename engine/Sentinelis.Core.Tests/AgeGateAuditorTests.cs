// using Moq;
// using Sentinelis.Core.Auditors;
// using Sentinelis.Core.Interfaces;
// using Sentinelis.Core.Models;

// namespace Sentinelis.Core.Tests;

// public class AgeGateAuditorTests
// {
//     private readonly Mock<IRegistryClient> _mockNpmClient;
//     private readonly Mock<IRegistryClientFactory> _mockFactory;

//     public AgeGateAuditorTests()
//     {
//         // 1. Setup the mocks
//         _mockNpmClient = new Mock<IRegistryClient>();
//         _mockFactory = new Mock<IRegistryClientFactory>();

//         // 2. Wire the factory to return our mock NPM client whenever "npm" is requested
//         _mockFactory.Setup(f => f.GetClient("npm")).Returns(_mockNpmClient.Object);
//     }

//     [Fact]
//     public async Task AuditAsync_WhenPackageIsYoungerThanQuarantine_ReturnsViolation()
//     {
//         // Arrange: Simulate a package published only 5 hours ago
//         var recentPublishDate = DateTime.UtcNow.AddHours(-5);
//         _mockNpmClient
//             .Setup(c => c.GetVersionPublishDateAsync("malicious-pkg", "1.0.0"))
//             .ReturnsAsync(recentPublishDate);

//         var auditor = new AgeGateAuditor(_mockFactory.Object);

//         var context = new AuditContext(
//             TargetPath: ".",
//             Dependencies: new List<DependencyInfo> { new("npm", "malicious-pkg", "1.0.0") },
//             QuarantineHours: 24 // Requires packages to be at least 24 hours old
//         );

//         // Act
//         var violations = (await auditor.AuditAsync(context)).ToList();

//         // Assert
//         Assert.Single(violations); // Should flag exactly 1 violation
//         Assert.Equal("AgeGate", violations[0].ModuleName);
//         Assert.Equal("npm:malicious-pkg", violations[0].PackageName);
//         Assert.Contains("hours ago", violations[0].Description);
//     }

//     [Fact]
//     public async Task AuditAsync_WhenPackageIsOlderThanQuarantine_ReturnsNoViolations()
//     {
//         // Arrange: Simulate a package published 48 hours ago
//         var safePublishDate = DateTime.UtcNow.AddHours(-48);
//         _mockNpmClient
//             .Setup(c => c.GetVersionPublishDateAsync("safe-pkg", "2.1.0"))
//             .ReturnsAsync(safePublishDate);

//         var auditor = new AgeGateAuditor(_mockFactory.Object);

//         var context = new AuditContext(
//             targetPath: targetPath,
//             dependencies: new List<DependencyInfo>(new("npm", "safe-pkg", "2.1.0") ),
//             outputFormat: format,
//             quarantineHours: config.QuarantineHours
//         );

//         // Act
//         var violations = (await auditor.AuditAsync(context)).ToList();

//         // Assert
//         Assert.Empty(violations); // Should pass with 0 violations
//     }
// }