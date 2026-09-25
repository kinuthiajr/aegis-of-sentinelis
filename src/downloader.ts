import * as core from '@actions/core';
import * as tc from '@actions/tool-cache';
import * as os from 'node:os';
import * as path from 'path';
import * as fs from 'fs';

export async function downloadEngine(version: string, repository: string): Promise<string> {
    const platform = os.platform();
    const arch = os.arch();
    
    // Map Node OS/Arch to .NET Runtime Identifier (RID)
    let rid = '';
    if (platform === 'linux' && arch === 'x64') rid = 'linux-x64';
    else if (platform === 'darwin' && arch === 'x64') rid = 'osx-x64';
    else if (platform === 'darwin' && arch === 'arm64') rid = 'osx-arm64';
    else if (platform === 'win32' && arch === 'x64') rid = 'win-x64';
    else throw new Error(`Unsupported runner platform: ${platform}-${arch}`);

    const toolName = 'SentinelisEngine';
    const binaryName = platform === 'win32' ? 'Sentinelis.Cli.exe' : 'Sentinelis.Cli';
    const releaseAssetName = `${binaryName}-${rid}`;

    // 1. Check if the tool is already cached on the runner
    let cachedPath = tc.find(toolName, version, rid);
    
    if (cachedPath) {
        core.info(`Found cached Sentinelis engine at ${cachedPath}`);
        return path.join(cachedPath, binaryName);
    }

    // 2. Construct the GitHub Release download URL
    const url = `https://github.com/${repository}/releases/download/v${version}/${releaseAssetName}`;
    core.info(`Downloading Sentinelis engine from ${url}...`);

    try {
        // 3. Download the binary
        const downloadPath = await tc.downloadTool(url);
        
        // 4. Ensure the binary is executable on Unix-based systems
        if (platform !== 'win32') {
            fs.chmodSync(downloadPath, 0o755);
        }

        // 5. Cache the binary for future workflow steps
        cachedPath = await tc.cacheFile(
            downloadPath, 
            binaryName, 
            toolName, 
            version, 
            rid
        );
        
        return path.join(cachedPath, binaryName);
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        core.setFailed(`Failed to download engine: ${message}`);
        throw error;
    }
}