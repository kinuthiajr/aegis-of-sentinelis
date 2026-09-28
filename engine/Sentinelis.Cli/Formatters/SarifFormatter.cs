using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sentinelis.Core.Models;

namespace Sentinelis.Cli.Formatters;

public static class SarifFormatter
{
    public static string Format(IEnumerable<AuditViolation> violations)
    {
        var log = new SarifLog();
        var run = new SarifRun();

        var distinctRules = new Dictionary<string, SarifRule>();

        foreach (var v in violations)
        {
            var ruleId = string.IsNullOrEmpty(v.ModuleName) ? "SEC001" : v.ModuleName;

            // Ensure distinct rule registration
            if (!distinctRules.ContainsKey(ruleId))
            {
                distinctRules[ruleId] = new SarifRule
                {
                    Id = ruleId,
                    ShortDescription = new SarifMultiformatMessageString { Text = $"Sentinelis Rule: {ruleId}" }
                };
            }

            // Map severity to SARIF levels
            var level = v.Severity.ToLowerInvariant() switch
            {
                "high" or "critical" => "error",
                "medium" or "warning" => "warning",
                _ => "note"
            };

            var result = new SarifResult
            {
                RuleId = ruleId,
                Level = level,
                Message = new SarifMultiformatMessageString
                {
                    Text = $"[{v.PackageName}@{v.Version}] {v.Description}"
                },
                Locations = new List<SarifLocation>
                {
                    new()
                    {
                        PhysicalLocation = new()
                        {
                            ArtifactLocation = new()
                            {
                                // Points to the lockfile or relative path if available
                                Uri = "package-lock.json"
                            }
                        }
                    }
                }
            };

            run.Results.Add(result);
        }

        run.Tool.Driver.Rules = distinctRules.Values.ToList();
        log.Runs.Add(run);

        return JsonSerializer.Serialize(log, SarifJsonContext.Default.SarifLog);
    }
}