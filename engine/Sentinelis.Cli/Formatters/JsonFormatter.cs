using System.Text.Json;
using Sentinelis.Core.Models;

namespace Sentinelis.Cli.Formatters;

// <summary>
// Formats a list of AuditViolation objects as JSON and writes it to the console.
// </summary>

public static class JsonFormatter
{
    public static void Format(IEnumerable<AuditViolation> violations)
    {
        // Serialize using the Native AOT Source Generator Context
        var json = JsonSerializer.Serialize(
            violations.ToList(),
            AuditResultJsonContext.Default.ListAuditViolation);

        Console.WriteLine(json);
    }
}