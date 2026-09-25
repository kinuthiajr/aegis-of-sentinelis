import * as core from '@actions/core';
import * as github from '@actions/github';
import { AuditViolation } from './types';

export async function reportViolations(violations: AuditViolation[], token: string): Promise<void> {
    if (violations.length === 0) {
        core.info('✅ No security violations found.');
        return;
    }

    core.error(`Found ${violations.length} security violation(s).`);

    // 1. Build the Markdown report
    let markdown = '## 🚨 Sentinelis Security Audit\n\n';
    markdown += 'The engine detected the following security violations in your dependencies:\n\n';
    markdown += '| Module | Package | Severity | Description |\n';
    markdown += '|--------|---------|----------|-------------|\n';

    for (const v of violations) {
        const severityIcon = (v.severity === 'Critical' || v.severity === 'High') ? '🔴' : '🟡';
        markdown += `| ${v.moduleName} | \`${v.packageName}@${v.version}\` | ${severityIcon} ${v.severity} | ${v.description} |\n`;
    }

    // 2. Post to the Pull Request
    const context = github.context;
    
    if (context.payload.pull_request) {
        try {
            const octokit = github.getOctokit(token);
            
            await octokit.rest.issues.createComment({
                owner: context.repo.owner,
                repo: context.repo.repo,
                issue_number: context.payload.pull_request.number,
                body: markdown
            });
            core.info('✅ Posted violation report to the Pull Request.');
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            core.warning(`Failed to post PR comment. Does the GITHUB_TOKEN have write permissions? Error: ${message}`);
        }
    } else {
        core.info('Not running in a Pull Request context. Skipping PR comment.');
    }

    // 3. Fail the GitHub Action workflow
    core.setFailed(`Sentinelis engine detected ${violations.length} security violations.`);
}