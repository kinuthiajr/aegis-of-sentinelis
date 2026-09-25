namespace Sentinelis.Core.Models;

// Produces either JSON or console output for the audit results. 
// The context is passed to the auditor to determine what to check and how to report it.
public record AuditContext(
    string TargetPath,
    string OutputFormat = "console", // "console" or "json"
    int QuarantineHours = 24
);