# Sentinelis

**Pre-Install Dependency Admission Controller for CI/CD**

Sentinelis is a high-performance, Native AOT-compiled security gate designed to enforce dependency supply chain policies **strictly before** package manager execution (`npm install`, `pnpm install`, `yarn`). Operating as a deterministic admission controller, Sentinelis evaluates lockfile structure, registry metadata, and release age to render a binding **Allow** or **Block** decision—stopping zero-day account takeovers and malicious lifecycle scripts before any untrusted code touches your build environment.

---

### Get started

### GitHub Actions & CI/CD Integration

Sentinelis compiles to a native binary (Native AOT), allowing it to execute inside CI/CD environments without requiring the .NET 9.0 SDK on the runner. What it means, its superfast.

#### Workflow Setup (.github/workflows/sentinelis.yml)

1. Create a workflow file in your repository at **.github/workflows/sentinelis.yml**

```yml
name: Sentinelisis-Takeoff

on:
  push:
    branches: [main, master]
  pull_request:
    branches: [main, master]

permissions:
  contents: read
  security-events: write # Required for GitHub Security tab SARIF alerts

jobs:
  audit-all-services:
    name: Audit Microservices
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      # 1. Download Sentinelis Native Binary
      - name: Download Sentinelis Engine
        run: |
          curl -L -o sentinelis https://github.com/my-org/sentinelis/releases/latest/download/sentinelis-linux-x64
          chmod +x sentinelis
          sudo mv sentinelis /usr/local/bin/

      # 2. Single pass scan across all subdirectories
      - name: Run Sentinelis Engine Across All Services
        run: |
          sentinelis --path . --format sarif --mode enforce
        continue-on-error: true

      # 3. Report all findings to GitHub Security Tab
      - name: Upload SARIF to GitHub Code Security
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: "sentinelis.sarif"
```

**Change the **your-org\*\* to your repo

### One more thing

#### Developer Workflow & sentinelis-trust.lock

Sentinelis uses a version-controlled manifest (sentinelis-trust.lock) to maintain explicit rule bypasses for verified dependencies without turning off security checks globally.

```md
[1. Local Dry Run] ──► [2. Trust via CLI] ──► [3. Commit Manifest] ──► [4. Automatic CI Pass]
`sentinelis --path .` `sentinelis trust` `git push` `GitHub Actions`
```

#### Step-by-Step Developer Flow

1. Add a Package & Run a Local Dry Run
   When you install a new dependency that relies on native compilation or build hooks (e.g., esbuild):

```md
Bash
npm install esbuild
```

Before pushing to GitHub, run a local scan to verify rule compliance:

```md
Bash

# macOS / Linux

sentinelis --path . --format console

# Windows (PowerShell)

.\sentinelis.exe --path . --format console
```

2. Handle Flagged Violations
   If Sentinelis flags a lifecycle script (ScriptExecution) or recent release (AgeGate):

```md
[!] Found 1 security violation(s):

[High] ScriptExecution: Package 'esbuild' (npm) executes an automated lifecycle script.
```

Inspect the package source. If your team verifies that the script is safe, add an exception for that rule:

```md
Bash
sentinelis trust esbuild --rule ScriptExecution --path .
```

3. Inspect sentinelis-trust.lock
   Sentinelis updates or generates sentinelis-trust.lock in your project root:

```json
{
  "trusted": {
    "esbuild": {
      "allowedRules": ["ScriptExecution"]
    }
  }
}
```

4. Commit to Version Control
   Commit sentinelis-trust.lock alongside your package.json changes:

```md
Bash
git add package.json package-lock.json sentinelis-trust.lock
git commit -m "chore(security): allowlist esbuild lifecycle script in trust manifest"
git push origin feature-branch
```

5. Automatic CI Verification
   When GitHub Actions runs, Sentinelis reads sentinelis-trust.lock, bypasses ScriptExecution specifically for esbuild, and passes the PR check with exit code 0.

### Quick Reference CLI Commands

```md
# Run a security audit against target path

sentinelis --path /path/to/project --format console

# Run in non-blocking advisory mode

sentinelis --path . --mode audit

# Allowlist a rule for a verified package

sentinelis trust <package-name> --rule <RuleName> --path .

# Allowlist AgeGate for a newly published internal library

sentinelis trust @my-org/core --rule AgeGate --path .
```

### Engine Configuration (sentinelis.json)

2. To establish shared project defaults across all developers and CI runs, create a sentinelis.json file in your repository root during initial setup.

```json
{
  "mode": "enforce",
  "quarantineHours": 168,
  "rules": {
    "ScriptExecution": true,
    "InsecureRegistry": true,
    "AgeGate": true
  }
}
```

| Field                    | Type    | Default     | Description                                                                                   |
| ------------------------ | ------- | ----------- | --------------------------------------------------------------------------------------------- |
| `mode`                   | String  | `"enforce"` | Operational mode (`"audit"` or `"enforce"`).                                                  |
| `quarantineHours`        | Integer | `168`       | Window (in hours) used by AgeGate to flag newly published package releases (`168h` = 7 days). |
| `rules.ScriptExecution`  | Boolean | `true`      | Flags packages with lifecycle install scripts (`postinstall`, `preinstall`).                  |
| `rules.InsecureRegistry` | Boolean | `true`      | Flags packages resolving over unencrypted `http://` registry URLs.                            |
| `rules.AgeGate`          | Boolean | `true`      | Flags package versions published within the quarantine window.                                |

### Operational Modes (audit vs. enforce)

- audit (Advisory Mode): Logs security violations as warnings in CI logs but always exits with code 0. Best for onboarding existing repos to inventory current supply-chain risks without blocking work.
- enforce (Strict Mode): Logs violations and exits with code 1, blocking pull requests from merging until exceptions are allowlisted.
  **CLI Override:** You can override the file configuration in terminal scripts or CI steps using flags:

## Why Pre-Install Admission Control?

### The Pipeline Ordering Problem

In CI pipelines that install, build, or otherwise materialize dependencies before running security analysis, malicious dependency code may execute before downstream scanners inspect the resulting environment. This is especially clear in ecosystems with install or build hooks, such as npm, Python, Rust, and some JVM and Ruby workflows. However, many SCA tools can scan manifests, lockfiles, or source files without installing packages, so the gap depends on pipeline ordering and scanner mode.

```
SENTINELIS PIPELINE (SAFE):
   Code Checkout ──► Sentinelis Gate ──► [ALLOW] ──► npm install --ignore-scripts ──► Build / Test
                          │
                      [BLOCK] ──► CI Fails (Zero code execution)
```

### The Sentinelis Guarantees

1. **Zero-Execution Architecture:** Sentinelis performs pure static analysis on raw manifest text. It never invokes package managers, executes JavaScript, or triggers lifecycle hooks.
2. **Deterministic Pre-Execution Barrier:** Enforces policy gates _before_ package manager execution occurs on developer machines or CI runners.
3. **Read-Only / Side-Effect-Free:** Sentinelis never automatically mutates lockfiles or executes repair commands (`npm audit fix`). It strictly outputs a binding policy decision (`allow` or `block`) with explicit violation details.

---

## How It Works

Sentinelis evaluates dependency trees in four deterministic, zero-execution phases before a single package is fetched or executed by your package manager.

### Phase 1: Zero-Execution Static Parsing

Without executing `node`, `npm`, or any package scripts, Sentinelis parses `package-lock.json` directly using a high-performance C# JSON parser. It builds an in-memory dependency graph containing every direct and transitive dependency, mapping:

- Package names and resolved semantic versions
- Download source URLs (`resolved`)
- Subresource Integrity hashes (`sha512` / `sha1` checksums)

### Phase 2: Registry Metadata Interrogation (`AgeGateAuditor`)

Sentinelis concurrently queries public or private registry metadata endpoints (e.g., `registry.npmjs.org`) to inspect official package publish history:

- **Timestamp Calculation:** $$\text{Release Age} = \text{Current Time} - \text{Publish Timestamp}$$
- **Quarantine Enforcement:** If $\text{Release Age} < \text{quarantineHours}$ (e.g., released 3 hours ago), Sentinelis flags the package as unproven, preventing zero-day supply chain attacks and compromised maintainer releases from reaching your build agents.

### Phase 3: Lockfile Integrity & Protocol Audit (`NpmLockfileAuditor`)

The policy engine evaluates the parsed graph for structural anomalies and policy breaches:

- **Protocol Verification:** Ensures all resolution endpoints enforce secure `https://` protocols (blocking unencrypted `http://` or raw git endpoints susceptible to MitM hijacking).
- **Integrity Validation:** Flags missing or malformed checksum hashes that could allow package tampering in transit.
- **Allowlist Exemption:** Checks package identifiers against configured allowlists in `sentinelis.json` to bypass false positives for trusted internal modules.

### Phase 4: Decision & Reporting

Sentinelis aggregates all rule findings and renders a binding admission result:

- **Enforce Mode:** Exits with code `1` if high-severity violations exist, halting the CI pipeline **before** `npm install` runs.
- **Audit Mode:** Emits findings as non-blocking warnings and exits with code `0`.
- **Report Generation:** Produces human-readable console logs, machine-parsable JSON payloads, or SARIF v2.1.0 output for native GitHub PR annotations.

---

### Downstream Sandboxing & Execution Isolation

Sentinelis serves as the **pre-install gatekeeper**. Because it evaluates pure text without downloading tarballs or executing code, it adds zero runtime risk.

If your organization requires deep source code inspection, static code analysis, or dynamic behavioral sandboxing of raw `.tgz` tarball contents:

1. **Sentinelis Gate First:** Verifies release age, registry origin, and lockfile integrity.
2. **Safe Fetch Phase:** Run `npm ci --ignore-scripts` inside an isolated container/sandbox _after_ Sentinelis grants admission.
3. **Payload Inspection:** Execute downstream static analysis or containerized behavioral sandboxing on the downloaded package files without risk of early `postinstall` script execution.

---

## Threat Model & Security Coverage

Sentinelis provides an execution-prevention layer targeting the initial stage of supply chain compromises:

| Threat Vector                  | Attack Mechanism                                                                                              | Sentinelis Defense                                                                                                                                                      |
| :----------------------------- | :------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Malicious Lifecycle Hooks**  | `postinstall` or `preinstall` scripts exfiltrating CI environment variables (`process.env`).                  | **Zero-Execution Gate:** Evaluates lockfiles as raw static text before `npm install` runs, preventing hooks from executing.                                             |
| **Zero-Day Account Takeovers** | Compromised maintainer credentials used to publish malware-laden patch releases (e.g., `event-stream`).       | **Age-Gating Quarantine:** Enforces a configurable cooling-off window (e.g., releases must be $\ge 24\text{ hours}$ old) to allow community/registry security response. |
| **Typosquatting & Lookalikes** | Accidental typos (`lodsh` vs `lodash`) introducing freshly published copycat packages.                        | **Age-Gating & Strict Allowlists:** New typosquat releases trigger age-gate quarantines and fail configured package allowlists.                                         |
| **Insecure Registry Targets**  | Lockfiles altered to resolve dependencies over unencrypted `http://` endpoints susceptible to MitM hijacking. | **Protocol Integrity Check:** Rejects any dependency entry configured with non-HTTPS resolution targets.                                                                |

---

## Key Features

- **Standalone CI binary:** Distributed as platform-specific .NET Native AOT executables for Linux, macOS, and Windows. Requires no Node.js, Python, or .NET SDK on the runner.
- **Lockfile discovery:** Scans supported lockfiles in single-project repositories and polyglot monorepos, with configurable include and exclude paths.
- **Minimum release age:** Blocks or flags dependency versions younger than a configurable maturity window, with support for explicit, expiring overrides.
- **Pre-install lockfile policy:** Audits package sources, registry hosts, URL schemes, dependency types, and integrity metadata without invoking package managers or package code.
- **SARIF reporting:** Emits SARIF 2.1.0 for GitHub Code Scanning, pull-request annotations, and compatible CI security platforms.
- **Policy as code:** Uses version-controlled sentinelis.json policies with enforce, audit, allowlist, severity, and override controls.

---

## Scope Boundaries & Ecosystem Position

Sentinelis is designed to complement scanners and threat intelligence platforms.

```
┌─────────────────────────┐      ┌───────────────────────────┐      ┌─────────────────────────┐
│  Lockfile / Manifest    │ ───► │  Sentinelis Admission     │ ───► │ Package Manager Fetch   │
│  Changes (PR / Push)    │      │  Gate (Policy Decision)   │      │ (npm ci / pnpm install) │
└─────────────────────────┘      └─────────────┬─────────────┘      └─────────────────────────┘
                                               │
                                     Queries Evidence
                                               │
                                               ▼
                                  ┌─────────────────────────┐
                                  │ Evidence Providers      │
                                  │ (NPM Registry / OSV)    │
                                  └─────────────────────────┘
```

- **What Sentinelis Does:** Acts as an execution-prevention layer and admission gate. It evaluates lockfile structure, registry origins, release age, and external evidence to make an immediate `allow` or `block` decision.
- **What Sentinelis Leaves to Downstream Steps:** Payload analysis, binary decompilation, or deep behavioral sandboxing of package contents. These inspections should happen as a separate, isolated phase _after_ pre-install admission passes.
- **Relationship to Scanners (Snyk, Dependabot, OSV):** Sentinelis treats vulnerability engines as **Evidence Providers**. It ingests evidence from these systems to enforce local repository policies before any package code runs.

---

## Developer Setup & Integration

### 1. GitHub Actions Workflow (Recommended)

Integrate Sentinelis into your repository to enforce pre-install admission checks on every Pull Request.

Create `.github/workflows/sentinelis.yml`:

```yaml
name: Sentinelis Admission Control

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

permissions:
  contents: read
  security-events: write # Required for posting findings to the Security tab

jobs:
  admission-gate:
    name: Pre-Install Security Gate
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      # 1. Run Sentinelis Gate BEFORE npm install
      - name: Run Sentinelis Engine
        uses: your-org/sentinelis-action@v1 # Replace with your GitHub handle/action repo
        with:
          path: "."
          format: "sarif"
        continue-on-error: true # Allows SARIF upload step to run even if gate blocks

      # 2. Upload Findings to GitHub Code Security
      - name: Upload SARIF to GitHub Code Security
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: "sentinelis.sarif"

      # 3. Proceed to install dependencies ONLY after gate evaluation
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: "20"

      - name: Safe Install
        run: npm ci --ignore-scripts
```

### 2. Local CLI Usage

Developers can download the standalone binary or compile locally to verify lockfiles prior to committing changes.

```bash
# Standard interactive terminal output
./sentinelis --path ./my-project --format console

# Standardized machine-readable SARIF output
./sentinelis --path ./my-project --format sarif

# Raw JSON output format
./sentinelis --path ./my-project --format json
```
