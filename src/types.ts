// Typescript wrapper to understand the structure of the 
// JSON that is returned by the Sentinelis engine when it performs a security audit on a project.

export interface AuditViolation {
    moduleName: string;
    packageName: string;
    version: string;
    severity: string;
    description: string;
}