using System;
using System.Collections.Generic;
using System.Linq;
using Sentinelis.Core.Models;
using Spectre.Console;

public static class ConsoleFormatter
{
    private static bool IsGitHubActions =>
        string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase);

    public static void Format(IEnumerable<AuditViolation> violations, IAnsiConsole? console = null)
    {
        var ansiConsole = console ?? AnsiConsole.Console;
        var violationList = violations.ToList();

        if (!violationList.Any())
        {
            ansiConsole.MarkupLine("[green]✔ No security violations found.[/]");
            return;
        }

        // 1. Emit GitHub Action Annotations if running in CI
        if (IsGitHubActions)
        {
            EmitGitHubAnnotations(violationList);
        }

        // 2. Render Spectre.Console Table
        ansiConsole.MarkupLine("\n[bold red]Security Violations Detected[/]");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Module[/]")
            .AddColumn("[bold]Package[/]")
            .AddColumn("[bold]Severity[/]")
            .AddColumn("[bold]Description[/]");

        foreach (var v in violationList)
        {
            var severityColor = v.Severity switch
            {
                "Critical" or "High" => "red",
                "Medium" or "Warning" => "yellow",
                _ => "grey"
            };

            table.AddRow(
                v.ModuleName,
                $"{v.PackageName}@{v.Version}",
                $"[{severityColor}]{v.Severity}[/]",
                v.Description
            );
        }

        ansiConsole.Write(table);
        ansiConsole.MarkupLine($"[red]Found {violationList.Count} violation(s).[/]\n");
    }

    private static void EmitGitHubAnnotations(IEnumerable<AuditViolation> violations)
    {
        foreach (var v in violations)
        {
            string level = v.Severity switch
            {
                "Critical" or "High" => "error",
                "Medium" or "Warning" => "warning",
                _ => "notice"
            };

            string fileAttr = !string.IsNullOrEmpty(v.ModuleName)
                ? $"file={EscapeProperty(v.ModuleName)},"
                : string.Empty;

            string titleAttr = $"title={EscapeProperty($"[{v.Severity}] {v.PackageName}")}";
            string message = EscapeData($"{v.PackageName}@{v.Version}: {v.Description}");

            Console.WriteLine($"::{level} {fileAttr}{titleAttr}::{message}");
        }
    }

    private static string EscapeProperty(string value) =>
        value.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A").Replace(",", "%2C").Replace(":", "%3A");

    private static string EscapeData(string value) =>
        value.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A");
}