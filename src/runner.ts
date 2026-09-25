import * as exec from '@actions/exec';
import * as core from '@actions/core';
import { AuditViolation } from './types';

export async function runEngine(binaryPath: string, targetPath: string): Promise<AuditViolation[]> {
    let stdout = '';
    let stderr = '';

    const options: exec.ExecOptions = {
        listeners: {
            stdout: (data: Buffer) => { stdout += data.toString(); },
            stderr: (data: Buffer) => { stderr += data.toString(); }
        },
        ignoreReturnCode: true, // We parse JSON to determine success/failure, not just exit codes
        silent: true // Prevents raw JSON from spamming the Action logs directly
    };

    core.info(`Executing engine at ${binaryPath} targeting ${targetPath}...`);
    
    await exec.exec(binaryPath, ['--path', targetPath, '--format', 'json'], options);

    if (stderr) {
        core.debug(`Engine stderr: ${stderr}`);
    }

    try {
        const results = JSON.parse(stdout) as AuditViolation[] | { error: string };
        
        if (!Array.isArray(results) && 'error' in results) {
            throw new Error(`Engine reported an internal error: ${results.error}`);
        }
        
        return results;
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        throw new Error(`Failed to parse engine output.\nError: ${message}\nStdout: ${stdout}`);
    }
}