import * as core from '@actions/core';
import { downloadEngine } from './downloader';
import { runEngine } from './runner';
import { reportViolations } from './reporter';

async function run(): Promise<void> {
    try {
        const version = core.getInput('version') || '1.0.0';
        const targetPath = core.getInput('target-path') || '.';
        const token = core.getInput('github-token') || '';
        const repository = core.getInput('engine-repository') || 'your-org/sentinelis';

        core.info(`Starting Sentinelis Action (v${version})...`);

        // 1. Download or retrieve cached Native AOT engine binary
        const binaryPath = await downloadEngine(version, repository);

        // 2. Run the C# engine binary against target path
        const violations = await runEngine(binaryPath, targetPath);

        // 3. Output results and post PR comments if violations exist
        await reportViolations(violations, token);
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        core.setFailed(`Sentinelis Action failed: ${message}`);
    }
}

run();