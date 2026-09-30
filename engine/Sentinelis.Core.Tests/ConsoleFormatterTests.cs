using System;
using System.Collections.Generic;
using System.IO;
using Sentinelis.Core.Models;
using Spectre.Console;
using Xunit;

[Collection("ConsoleTests")]
public class ConsoleFormatterTests
{
    [Fact]
    public void Format_WhenGitHubActionsSet_EmitsWorkflowCommands()
    {
        // Arrange
        var violations = new List<AuditViolation>
        {
           new AuditViolation(
        "package-lock.json",
        "esbuild",
        "0.18.0",
        "High",
        "Executes an automated lifecycle script.")
        };

        var originalOut = Console.Out;
        var originalEnv = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        var originalAnsiConsole = AnsiConsole.Console;

        using var stringWriter = new StringWriter();

        try
        {
            // Set environment and bind Spectre.Console to the test writer
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "true");
            Console.SetOut(stringWriter);

            var testConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(stringWriter)
            });

            // Act
            ConsoleFormatter.Format(violations, testConsole);

            // Assert
            string output = stringWriter.ToString();

            Assert.Contains("::error", output);
            Assert.Contains("file=package-lock.json", output);
            Assert.Contains("esbuild@0.18.0: Executes an automated lifecycle script.", output);
        }
        finally
        {
            // Restore environment, standard output, and Spectre console state
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", originalEnv);
            Console.SetOut(originalOut);
            AnsiConsole.Console = originalAnsiConsole;
        }
    }

    [Fact]
    public void Format_WhenGitHubActionsNotSet_DoesNotEmitWorkflowCommands()
    {
        // Arrange
        var violations = new List<AuditViolation>
        {
           new AuditViolation(
        "package-lock.json",
        "esbuild",
        "0.18.0",
        "High",
        "Executes an automated lifecycle script.")

        };

        var originalOut = Console.Out;
        var originalEnv = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        var originalAnsiConsole = AnsiConsole.Console;

        using var stringWriter = new StringWriter();

        try
        {
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "false");
            Console.SetOut(stringWriter);

            var testConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(stringWriter)
            });

            // Act
            ConsoleFormatter.Format(violations, testConsole);

            // Assert
            string output = stringWriter.ToString();

            Assert.DoesNotContain("::error", output);
            Assert.DoesNotContain("::warning", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", originalEnv);
            Console.SetOut(originalOut);
            AnsiConsole.Console = originalAnsiConsole;
        }
    }
}