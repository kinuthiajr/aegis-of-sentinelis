namespace Sentinelis.Core.Models;

// Lists security violations found during an audit. If empty, the check passed
public record AuditViolation(
    string ModuleName,  // e.g., "AgeGate"
    string PackageName, // e.g., "pkg-installed"
    string Version, // e.g., "1.0.0"
    string Severity, // "Critical", "Warning"
    string Description // "Package was published 2 hours ago."
);