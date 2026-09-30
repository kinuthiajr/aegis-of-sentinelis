using System.Collections.Generic;

namespace Sentinelis.Core.Models;

public class AuditContext
{
    public string TargetPath { get; }
    public IReadOnlyList<DependencyInfo> Dependencies { get; }
    public string OutputFormat { get; }
    public int QuarantineHours { get; }

    public AuditContext(
        string targetPath,
        IReadOnlyList<DependencyInfo> dependencies,
        string outputFormat = "console",
        int quarantineHours = 168)
    {
        TargetPath = targetPath;
        Dependencies = dependencies;
        OutputFormat = outputFormat;
        QuarantineHours = quarantineHours;
    }
}