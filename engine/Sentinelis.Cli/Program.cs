using engine.Clients;
using Sentinelis.Cli.Formatters;
using Sentinelis.Core.Auditors;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;
using Sentinelis.Modules.Lockfiles.Auditors;

namespace Sentinelis.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var targetPath = Directory.GetCurrentDirectory();
        var format = "console";

        // Lightweight CLI Argument Parsing
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--path" && i + 1 < args.Length)
            {
                targetPath = args[i + 1];
                i++;
            }
            else if (args[i] == "--format" && i + 1 < args.Length)
            {
                format = args[i + 1];
                i++;
            }
        }

        var context = new AuditContext(
            TargetPath: targetPath,
            Dependencies: new List<DependencyInfo>(), // Placeholder; actual dependency parsing logic would populate this
            OutputFormat: format,
            QuarantineHours: 24
        );

        // Shared Services and Auditors
        var registryFactory = new RegistryClientFactory();

        // Auditor Modules
        var auditors = new List<ISecurityAuditor>
        {
            new NpmLockfileAuditor(),
            new AgeGateAuditor(registryFactory)
        };

        var allViolations = new List<AuditViolation>();

        // Execution Loop to all the auditors
        try
        {
            foreach (var auditor in auditors)
            {
                var violations = await auditor.AuditAsync(context);
                allViolations.AddRange(violations);
            }

            // Output Routing
            if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                JsonFormatter.Format(allViolations);
                return 0; // JSON mode exits 0; TS wrapper evaluates the JSON payload to decide CI failure.
            }

            ConsoleFormatter.Format(allViolations);
            return allViolations.Count > 0 ? 1 : 0; // Console mode exits 1 to fail local terminals scripts directly.
        }
        catch (Exception ex)
        {
            if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"{{\"error\": \"{ex.Message}\"}}");
            }
            else
            {
                Console.WriteLine($"Fatal Error: {ex.Message}");
            }
            return -1;
        }
    }
}