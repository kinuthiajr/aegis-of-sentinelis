using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using engine.Clients;
using Sentinelis.Cli.Formatters;
using Sentinelis.Core.Auditors;
using Sentinelis.Core.Interfaces;
using Sentinelis.Core.Models;
using Sentinelis.Modules.Lockfiles.Parsers.Npm;
using Sentinelis.Modules.Lockfiles.Parsers.Npm.Models;


namespace Sentinelis.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var targetPath = Directory.GetCurrentDirectory();
        var format = "console";

        // 1. Lightweight CLI Argument Parsing
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

        // 2. CONFIG LOAD PHASE: Read sentinelis.json from the target directory
        var configPath = Path.Combine(targetPath, "sentinelis.json");
        var config = new SentinelisConfig(); // Defaults to Enforce mode, 24h quarantine

        if (File.Exists(configPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(configPath);
                config = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.SentinelisConfig) ?? config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to parse sentinelis.json: {ex.Message}. Using defaults.");
            }
        }

        // PARSING PHASE: Extract dependencies using all registered parsers

        var parsers = new List<IDependencyParser>
        {
            new NpmLockfileParser()
        };


        var dependencies = new List<DependencyInfo>();
        foreach (var parser in parsers)
        {
            var parsed = parser.Parse(targetPath).ToList();
            // Console.WriteLine($"[INFO] {parser.GetType().Name} discovered {parsed.Count} dependencies.");
            dependencies.AddRange(parsed);
        }

        // Update AuditContext to use the dynamic QuarantineHours from the config
        var context = new AuditContext(
            targetPath: targetPath,
            dependencies: dependencies,
            outputFormat: format,
            quarantineHours: config.QuarantineHours
        // Note: If you added 'Config' directly to AuditContext, pass it here too: Config: config
        );

        // Shared Services
        var registryFactory = new RegistryClientFactory();

        // 3. AUDITOR REGISTRATION PHASE: Apply rule toggles from config
        var auditors = new List<ISecurityAuditor>();

        // Rule check for unencrypted http:// registry URLs
        if (!config.Rules.TryGetValue("InsecureRegistry", out var insecureRegistryEnabled) || insecureRegistryEnabled)
        {
            auditors.Add(new InsecureRegistryAuditor());
        }

        // Rule check for preinstall/install/postinstall scripts
        if (!config.Rules.TryGetValue("ScriptExecution", out var scriptExecutionEnabled) || scriptExecutionEnabled)
        {
            auditors.Add(new ScriptExecutionAuditor());
        }

        // 🔍 DEBUG: Print how many auditors are registered
        // Console.WriteLine($"[DEBUG] Registered auditors count: {auditors.Count}");

        // Only add AgeGate if it isn't explicitly disabled
        if (!config.Rules.TryGetValue("AgeGate", out var ageGateEnabled) || ageGateEnabled)
        {
            auditors.Add(new AgeGateAuditor(registryFactory));
        }

        var allViolations = new List<AuditViolation>();

        // Execution Loop to all the auditors
        try
        {
            foreach (var auditor in auditors)
            {
                var violations = (await auditor.AuditAsync(context)).ToList();
                allViolations.AddRange(violations);
            }

            // 4. FILTERING PHASE: Apply the allowlist to remove ignored packages
            var filteredViolations = allViolations.Where(v =>
            {
                var packageId = v.PackageName; // e.g., "npm:lodash"
                var exactVersionId = $"{v.PackageName}@{v.Version}"; // e.g., "npm:lodash@4.17.21"

                return !config.Allowlist.Contains(packageId) && !config.Allowlist.Contains(exactVersionId);
            }).ToList();

            // OUTPUT ROUTING PHASE (SARIF vs JSON vs Console)
            // Now that we have `filteredViolations`, we can format them.
            if (format.Equals("sarif", StringComparison.OrdinalIgnoreCase))
            {
                var sarifJson = SarifFormatter.Format(filteredViolations);

                // Write directly to file for GitHub upload step
                var outputPath = Path.Combine(targetPath, "sentinelis.sarif");
                await File.WriteAllTextAsync(outputPath, sarifJson);

                Console.WriteLine($"[SARIF] Report successfully written to {outputPath}");

                // Exit codes for SARIF
                if (filteredViolations.Count > 0 && config.Mode.Equals("enforce", StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }
                return 0;
            }

            // 5. Output Routing & Execution Modes (Enforce vs Audit)
            if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                JsonFormatter.Format(filteredViolations);
                // JSON mode always exits 0. The TS wrapper evaluates the JSON payload and the config.Mode to decide CI failure.
                return 0;
            }

            ConsoleFormatter.Format(filteredViolations);

            if (filteredViolations.Count > 0)
            {
                if (config.Mode.Equals("audit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\n[AUDIT MODE] Warning only. Pipeline will continue.");
                    return 0;
                }

                Console.WriteLine("\n[ENFORCE MODE] Pipeline stopped due to security violations.");
                return 1; // Fails local terminals scripts directly
            }

            return 0;
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