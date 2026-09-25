using Sentinelis.Core.Models;
using Spectre.Console;

namespace Sentinelis.Cli.Formatters;

public static class ConsoleFormatter
{
    public static void Format(IEnumerable<AuditViolation> violations)
    {
        var violationList = violations.ToList();

        if (!violationList.Any())
        {
            AnsiConsole.MarkupLine("[green]✔ No security violations found.[/]");
            return;
        }

        AnsiConsole.MarkupLine("\n[bold red]Security Violations Detected[/]");

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

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[red]Found {violationList.Count} violation(s).[/]\n");
    }
}

