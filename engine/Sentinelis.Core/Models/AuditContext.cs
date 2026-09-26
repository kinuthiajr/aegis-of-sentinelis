// The context passed to the auditor
public record AuditContext(
    string TargetPath,
    IReadOnlyList<DependencyInfo> Dependencies, //The agnostic bridge
    string OutputFormat = "console",
    int QuarantineHours = 24
);

// Represents a parsed dependency from ANY ecosystem
public record DependencyInfo(
    string Ecosystem, // e.g., "npm", "pypi", "nuget"
    string Name,
    string Version
);