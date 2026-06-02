# al-mcp-bridge

**Semantic AL refactoring MCP server.** A thin bridge between AI agents (Claude, Copilot, anything speaking MCP) and the AL language server bundled with the Microsoft AL VS Code extension.

The goal is narrow: give an AI the *semantic* primitives it needs to write, review, and refactor AL code with the same correctness guarantees a human developer gets from the editor — rename across a project, find references, validate an edit against the real compiler, format the result, run tests.

---

## Table of Contents

1. [Why this exists](#1-why-this-exists)
2. [Tech stack](#2-tech-stack)
3. [Prerequisites](#3-prerequisites)
4. [Getting started](#4-getting-started)
   - [Install and build](#install-and-build)
   - [Locate the AL language server](#locate-the-al-language-server)
   - [Install the AL language server for tests](#install-the-al-language-server-for-tests)
5. [Environment variables](#5-environment-variables)
6. [Analyzer configuration](#6-analyzer-configuration-from-vscodesettingsjson)
7. [Connecting to Claude Code](#7-connecting-to-claude-code)
   - [Option A: project-local](#option-a-project-local-recommended)
   - [Option B: user-wide](#option-b-user-wide)
   - [Option C: claude mcp add](#option-c-claude-mcp-add)
8. [MCP tool reference](#8-mcp-tool-reference)
9. [Architecture](#9-architecture)
   - [Directory structure](#directory-structure)
   - [Request lifecycle](#request-lifecycle)
   - [Diagnostics flow](#diagnostics-flow)
   - [Analyzer sibling loading](#analyzer-sibling-loading)
   - [Credential storage](#credential-storage)
10. [Available scripts](#10-available-scripts)
11. [Testing](#11-testing)
12. [Troubleshooting](#12-troubleshooting)
13. [Design notes](#13-design-notes)

---

## 1. Why this exists

The Microsoft AL VS Code extension ships an AL language server (`Microsoft.Dynamics.Nav.EditorServices.Host`) that speaks standard LSP plus a handful of AL-specific extensions. That language server already knows how to rename symbols, find references, apply code fixes, compile, and run tests — but its existing MCP surface (`al_symbolsearch`, `al_compile`, etc.) does not expose the semantic *edit loop* an AI agent needs:

- Apply a code change in memory and get fresh compiler diagnostics in the same round trip
- Preview an edit without touching disk
- Rename a symbol across the entire project (not a text replace)
- List and execute code-action fixes from loaded analyzer DLLs (ALCops, LinterCop, etc.)
- Run AL test codeunits against an on-premise Business Central instance from Linux

This bridge adds those missing primitives as a stdio MCP server that the AI connects to alongside whatever else it uses. It does not modify, patch, or replace the Microsoft extension — it simply spawns the LS binary the extension already installed and talks LSP to it.

---

## 2. Tech stack

| Layer | Technology |
|---|---|
| **Language** | TypeScript 5.6+, ES2022 modules |
| **Runtime** | Node.js 20+ |
| **MCP transport** | `@modelcontextprotocol/sdk` (stdio) |
| **LSP client** | `vscode-jsonrpc` + `vscode-languageserver-protocol` |
| **AL language server** | `ms-dynamics-smb.al` VS Code extension (any installed version; pinned to 17.0.2273547 for tests) |
| **Test runner** | Node built-in `--test` runner (no Jest, no Vitest) |
| **BC test runner** | `@microsoft/signalr` — connects to BC's `TestRunnerHub` SignalR endpoint |
| **Input validation** | `zod` |
| **Build output** | `dist/index.js` (ESM) |

---

## 3. Prerequisites

**Required for running the bridge:**

- **Node.js 20 or higher** (the bridge uses native `fetch`, `FormData`, `structuredClone`)
- **Microsoft AL VS Code extension** — install `ms-dynamics-smb.al` from the VS Code Marketplace; the bridge needs the language server binary that ships with it. VS Code itself does not need to be *running* — just installed.
- **An AL project** — a folder containing `app.json`. Dependencies (`*.app` files) must be in `.alpackages/`; populate them with *AL: Download symbols* in VS Code before using the bridge.

**Required only for e2e tests:**

- `unzip` (Linux/macOS): `sudo apt install unzip` or `brew install unzip`

**Required only for `al_run_tests` and `al_publish`:**

- A running Business Central on-premise development service tier reachable from this machine
- `BC_USER` and `BC_PASSWORD` environment variables, or a credentials file (see [Credential storage](#credential-storage))

---

## 4. Getting started

### Install and build

```bash
git clone <this-repo>
cd al-mcp-bridge
npm install
npm run build
```

The build compiles `src/**/*.ts` to `dist/` using `tsc`. The entry point is `dist/index.js`.

### Locate the AL language server

The bridge auto-detects the newest installed AL extension under `~/.vscode/extensions/ms-dynamics-smb.al-*/bin/`. You can override this by setting `AL_LS_PATH`.

To find the path manually:

```bash
# Linux / macOS
ls ~/.vscode/extensions | grep ms-dynamics-smb.al
# → ms-dynamics-smb.al-17.0.2273547  (pick the newest)

# The LS binary:
# Linux:   ~/.vscode/extensions/ms-dynamics-smb.al-<ver>/bin/linux/Microsoft.Dynamics.Nav.EditorServices.Host
# macOS:   ~/.vscode/extensions/ms-dynamics-smb.al-<ver>/bin/darwin/Microsoft.Dynamics.Nav.EditorServices.Host
# Windows: %USERPROFILE%\.vscode\extensions\ms-dynamics-smb.al-<ver>\bin\win32\Microsoft.Dynamics.Nav.EditorServices.Host.exe
```

If auto-detect resolves the wrong version, set the path explicitly:

```bash
export AL_LS_PATH="$HOME/.vscode/extensions/ms-dynamics-smb.al-17.0.2273547/bin/linux/Microsoft.Dynamics.Nav.EditorServices.Host"
```

### Install the AL language server for tests

The e2e test suite needs the AL LS binary. Run once before the first test run:

```bash
npm run install:al-ls
```

This script (in `scripts/install-al-ls.mjs`) resolves the LS in the following order:

1. `AL_LS_PATH` in env — already have a binary, just record it.
2. Local VS Code extension install at the pinned version (`17.0.2273547`) — free if you already develop AL on this machine.
3. Download the VSIX from the VS Code Marketplace, cache it under `tests/.al-ls/vsix-cache/`, and extract `bin/linux/` plus `bin/Analyzers/` to `tests/.al-ls/<version>/`.

To check the current status:

```bash
npm run install:al-ls:status
```

### Smoke test

Verify the LSP handshake, tool registration, and core AL tools work against a real project:

```bash
AL_WORKSPACE="/path/to/your/al/project" \
AL_PACKAGE_CACHE="/path/to/your/al/project/.alpackages" \
node scripts/smoke.mjs "/path/to/your/al/project/src/some.al"
```

Expected output: 13 tools listed, non-empty `al_list_objects`, an outline tree of your AL file.

---

## 5. Environment variables

| Variable | Required | Default | Purpose |
|---|---|---|---|
| `AL_LS_PATH` | No (autodetect) | Newest `ms-dynamics-smb.al-*` under `~/.vscode/extensions/` | Absolute path to `Microsoft.Dynamics.Nav.EditorServices.Host(.exe)` |
| `AL_WORKSPACE` | **Yes** | — | Absolute path to your AL project root (the folder containing `app.json`). Semicolon-separated for multiple projects (monorepo). If omitted, the bridge scans upward then downward from `cwd`. |
| `AL_PACKAGE_CACHE` | Recommended | `.alpackages/` in project root (if present) | Semicolon-separated list of `.alpackages` directories. Without this, BC dependency symbols won't resolve and most tools return empty results. |
| `AL_EXTRA_CODE_ANALYZERS` | No | — | Analyzer DLLs to load **on top of** `al.codeAnalyzers` from `.vscode/settings.json`, separated by `;` or `,`. Supports `${analyzerFolder}`, `${workspaceFolder}`, `${CodeCop}`, `${AppSourceCop}`, etc. placeholders. Always-active: when a workspace sets `"al.enableCodeAnalysis": false`, these still load and the master switch is force-enabled. |
| `AL_EXTRA_RULESETS` | No | — | `*.ruleset.json` paths to apply **in addition to** `al.ruleSetPath`, separated by `;` or `,`. The bridge merges multiple rulesets into a composite file under the OS temp directory. |
| `AL_DIAGNOSTICS_SETTLE_MS` | No | `750` | Milliseconds `al_apply_edit` waits for the next `publishDiagnostics` notification after sending a `didChange`. Increase to `2000`+ for slow machines or large projects. |
| `BC_USER` | No | — | Business Central username for `al_run_tests` and `al_publish`. Takes precedence over the credentials file. |
| `BC_PASSWORD` | No | — | Business Central password. Paired with `BC_USER`. |
| `BC_ALLOW_INVALID_CERT` | No | — | Set to `1` to skip TLS certificate validation for BC connections. Process-scoped. Prefer fixing the server certificate. |
| `AL_BRIDGE_DEBUG_DIAGS` | No | — | Set to any non-empty value to log every `publishDiagnostics` notification to stderr with URI, count, and rule codes. Useful for tracing missing diagnostics. |
| `AL_EXT_VERSION` | No | Value from `package.json` | Override the AL extension version for `install-al-ls.mjs`. |
| `XDG_CONFIG_HOME` | No | `~/.config` | Base directory for the credentials file (XDG convention). |

---

## 6. Analyzer configuration (from `.vscode/settings.json`)

The bridge reads `<workspace>/.vscode/settings.json` using the same logic VS Code uses. No extra config files or duplication needed.

| Setting | Type | Default | Purpose |
|---|---|---|---|
| `al.codeAnalyzers` | `string[]` | `[]` | Analyzer DLL paths. Supports VS Code placeholders: `${analyzerFolder}`, `${workspaceFolder}`, `${alWorkspaceFolder}`, `${AppSourceCop}`, `${CodeCop}`, `${PerTenantExtensionCop}`, `${UICop}` |
| `al.enableCodeAnalysis` | `boolean` | `true` | Master switch for the analyzer pipeline |
| `al.enableCodeActions` | `boolean` | `true` | Enables `textDocument/codeAction` responses |
| `al.backgroundCodeAnalysis` | `string` | `"File"` | `"File"`, `"Project"`, or `"None"`. Set to `"Project"` to surface third-party analyzer diagnostics (LinterCop, ALCops) across all files without opening each one. |
| `al.ruleSetPath` | `string` | — | Path to a `*.ruleset.json`, relative to the workspace root or absolute |
| `al.assemblyProbingPaths` | `string[]` | `[]` | Additional CLR assembly probe paths, merged with auto-detected analyzer directories |

### Analyzer sibling auto-loading

The AL LS's Roslyn analyzer host isolates each entry in `codeAnalyzers` inside its own `AssemblyLoadContext`. This means a DLL like `BusinessCentral.LinterCop.dll` cannot automatically find `Microsoft.Dynamics.Nav.Analyzers.Common.dll` even when it sits in the same folder. The bridge detects known dependencies and auto-prepends the required sibling DLLs:

| Analyzer pattern | Auto-added siblings |
|---|---|
| `businesscentral.lintercop.dll` | `Microsoft.Dynamics.Nav.Analyzers.Common.dll` |
| `alcops.*.dll` | `ALCops.Common.dll` |
| `microsoft.dynamics.nav.(codecop|appsourcecop|uicop|pertenantextensioncop).dll` | `Microsoft.Dynamics.Nav.Analyzers.Common.dll`, `Microsoft.Dynamics.Nav.AL.Common.dll` |
| `socitas.reviewercop.dll` | `Socitas.ReviewerCop.Common.dll`, `ALCops.Common.dll`, `ALCops.CompanyCop.dll` |

Without this, LinterCop and similar analyzers fail at probe time with `AD0001 … Could not load file or assembly …Common…` on `app.json` and no rules fire.

### Example `.vscode/settings.json`

```jsonc
{
  "al.codeAnalyzers": [
    "${CodeCop}",
    "${UICop}",
    "${AppSourceCop}",
    "${PerTenantExtensionCop}",
    "${analyzerFolder}BusinessCentral.LinterCop.dll",
    "${analyzerFolder}ALCops.LinterCop.dll",
    "${analyzerFolder}ALCops.ApplicationCop.dll"
  ],
  "al.enableCodeAnalysis": true,
  "al.enableCodeActions": true,
  "al.backgroundCodeAnalysis": "Project",
  "al.ruleSetPath": ".codeanalyzer/my.ruleset.json"
}
```

### Forcing house-wide analyzers via environment

When you want a linter active regardless of what each project's `.vscode/settings.json` says — typical for enforcing a team rule set — set `AL_EXTRA_CODE_ANALYZERS` on the MCP server itself:

```jsonc
// .mcp.json in your AL project (or merge into ~/.claude.json)
{
  "mcpServers": {
    "al": {
      "command": "node",
      "args": ["/path/to/al-mcp-bridge/dist/index.js"],
      "env": {
        "AL_WORKSPACE": "${workspaceFolder}",
        "AL_PACKAGE_CACHE": "${workspaceFolder}/.alpackages",
        "AL_EXTRA_CODE_ANALYZERS": "/team/analyzers/HouseRules.dll",
        "AL_EXTRA_RULESETS": "/team/rulesets/house.ruleset.json"
      }
    }
  }
}
```

---

## 7. Connecting to Claude Code

The bridge is a stdio MCP server. Any MCP-compatible client (Claude Code, Copilot, etc.) can use it.

### Option A: project-local (recommended)

Commit an `.mcp.json` at your AL project root (the folder that contains `app.json`):

```json
{
  "mcpServers": {
    "al": {
      "command": "node",
      "args": ["/absolute/path/to/al-mcp-bridge/dist/index.js"],
      "env": {
        "AL_WORKSPACE": "${workspaceFolder}",
        "AL_PACKAGE_CACHE": "${workspaceFolder}/.alpackages"
      }
    }
  }
}
```

Claude Code prompts once to trust the new server. After that, `al_*` tools appear in `/tools` and are callable from any prompt.

### Option B: user-wide

Add the `mcpServers.al` block to `~/.claude.json` (Windows: `%USERPROFILE%\.claude.json`). The server is available in every session, regardless of working directory.

### Option C: claude mcp add

```bash
claude mcp add al -- node /absolute/path/to/al-mcp-bridge/dist/index.js
# Then edit ~/.claude.json to add env vars under mcpServers.al.env
```

### Verifying the connection

Inside a Claude Code session:

```
/mcp
```

You should see `al` listed. Try:

```
/tools
```

You should see `al_document_outline`, `al_get_diagnostics`, `al_compile`, and the other tools listed.

A quick sanity check:

> "List all codeunits in this workspace."

The agent should call `al_list_objects` with `types: ["Codeunit"]` and return a structured list.

See [`examples/mcp.json`](examples/mcp.json) for a fully annotated config.

---

## 8. MCP tool reference

All tools are registered in `src/tools/register.ts`. Inputs are validated with Zod; outputs are JSON-serialized.

### Navigation and discovery

| Tool | LSP endpoint | Description |
|---|---|---|
| `al_document_outline` | `textDocument/documentSymbol` | Return the AL object/member structure of a file — objects, triggers, procedures, fields, and their line ranges. |
| `al_get_symbol_at` | `textDocument/hover` + `textDocument/definition` | Resolve the symbol at (file, line, character). Returns hover text (type + XML doc) plus definition location(s). Two LSP round trips combined into one MCP call. |
| `al_find_references` | `textDocument/references` | Find all references to the symbol at a position. Semantic (compiler-driven), not textual. Handles the AL LS quirk where references from the declaration site return empty — retries from a call site automatically. |
| `al_symbol_search` | `al/symbolSearch` | Search AL symbols across the project and dependencies. Pass `query='*'` with filters to enumerate. Member-level symbols (procedures, fields, triggers) are only returned when `memberKinds` or `objectName` is specified. |
| `al_list_objects` | `al/getApplicationObjects` | List AL application objects (tables, pages, codeunits, …) without reading files. |

### Editing

| Tool | LSP endpoint | Description |
|---|---|---|
| `al_apply_edit` | `textDocument/didChange` + `publishDiagnostics` | Replace the full text of an AL file and return fresh compiler diagnostics in the same call. Set `persist=false` to keep the change in the LS buffer only (preview mode — no disk write). |
| `al_get_diagnostics` | `publishDiagnostics` (push) + `textDocument/diagnostic` (pull) | Return compiler and analyzer diagnostics for a file. Merges LSP push and pull channels — required because the AL LS routes third-party analyzer findings (LinterCop, ALCops) through pull diagnostics only. Set `waitForFresh=true` to wait for the next publish after an edit. |
| `al_format` | `textDocument/formatting` or `textDocument/rangeFormatting` | Run the AL formatter on the full document or a range. Returns text edits; apply them with `al_apply_edit`. |
| `al_rename` | `textDocument/rename` | Semantic rename of the symbol at a position. Returns a per-file edit set for review. Does **not** apply to disk — pass the resulting text through `al_apply_edit` to commit. |

### Code actions

| Tool | LSP endpoint | Description |
|---|---|---|
| `al_list_code_actions` | `textDocument/codeAction` | List available code actions (quickfixes, refactorings) at a position or range. Works with loaded analyzer DLLs — ALCops and LinterCop fixes appear here when configured. Filter by `only` (e.g. `["quickfix"]`). |
| `al_run_code_action` | `al/runCodeAction` + `workspace/applyEdit` (reverse) | Execute a code action returned by `al_list_code_actions`. The AL LS uses a non-standard reverse-request protocol: the bridge pre-registers a waiter before sending the action, then captures the inbound `workspace/applyEdit`. Returns per-file before/after text plus fresh diagnostics. Set `persist=false` for preview mode. |

### Build and deploy

| Tool | Binary / endpoint | Description |
|---|---|---|
| `al_compile` | `alc` (ships with the AL extension) | Compile an AL project to a `.app` package. Returns exit code, parsed SARIF diagnostics (with file/line ranges), and the produced `.app` path. Uses the same analyzer, package cache, and ruleset config as the LSP. Linux-compatible. |
| `al_publish` | BC `/<instance>/dev/apps` HTTP endpoint | Upload a compiled `.app` to a Business Central on-premise dev service tier. Reads server/instance/tenant from `.vscode/launch.json`. Requires `BC_USER`/`BC_PASSWORD` or a credentials file. |
| `al_run_tests` | BC `/<instance>/dev/TestRunnerHub` SignalR | Run an AL test codeunit against a Business Central on-premise dev service tier. Reads connection info from `.vscode/launch.json`. Returns per-method pass/fail/skipped results. Serializes concurrent calls per hub to avoid BC's single-session restriction. |

---

## 9. Architecture

### Directory structure

```
al-mcp-bridge/
├── src/
│   ├── index.ts              # Entry point: spawn LS, register tools, start MCP
│   ├── config.ts             # Environment + settings.json ingestion, workspace discovery
│   ├── lsp/
│   │   ├── client.ts         # AlLspClient: child process lifecycle, JSON-RPC connection, document buffer
│   │   └── diagnostics.ts    # DiagnosticsCache: push/pull merge, waiters for async diagnostics
│   └── tools/
│       ├── register.ts       # Wires all tools to the McpServer instance
│       ├── diagnostics.ts    # al_get_diagnostics
│       ├── edit.ts           # al_apply_edit
│       ├── outline.ts        # al_document_outline
│       ├── symbol.ts         # al_get_symbol_at
│       ├── references.ts     # al_find_references
│       ├── rename.ts         # al_rename
│       ├── format.ts         # al_format
│       ├── search.ts         # al_symbol_search, al_list_objects
│       ├── codeActions.ts    # al_list_code_actions, al_run_code_action
│       ├── compile.ts        # al_compile (alc subprocess)
│       ├── publish.ts        # al_publish (BC HTTP)
│       └── runTests.ts       # al_run_tests (SignalR)
├── scripts/
│   ├── install-al-ls.mjs     # Download/locate AL LS for the test suite
│   ├── smoke.mjs             # Quick sanity check against a real AL project
│   ├── smoke-compile.mjs     # Smoke test: al_compile
│   ├── smoke-publish.mjs     # Smoke test: al_publish
│   ├── smoke-runtests.mjs    # Smoke test: al_run_tests
│   └── smoke-winklimax.mjs   # Smoke test: Winklimax project fixture
├── tests/
│   ├── helpers/
│   │   └── bridge.mjs        # startBridge(), waitFor(), fixturePath() — test utilities
│   ├── fixtures/
│   │   └── analyzers-sanity/ # Minimal AL project that intentionally trips analyzer rules
│   └── e2e/
│       └── diagnostics.analyzers.test.mjs  # E2e: MS cops + ALCops + LinterCop + ApplicationCop
├── examples/
│   └── mcp.json              # Annotated example MCP config
├── decompiled/               # Decompiled AL extension source for protocol reference
├── dist/                     # TypeScript build output (gitignored except .gitkeep)
├── package.json
└── tsconfig.json
```

### Request lifecycle

```
┌────────────────┐   MCP stdio    ┌──────────────────┐   LSP stdio   ┌───────────────────────┐
│  Claude / any  │ ◀───────────▶  │  al-mcp-bridge   │ ◀───────────▶ │  AL language server   │
│  MCP client    │  tool calls    │  (this repo)     │  JSON-RPC     │  (bundled w/ AL ext.) │
└────────────────┘                └──────────────────┘               └───────────────────────┘
                                          │
                                          └── alc binary (for al_compile)
                                          └── BC HTTP + SignalR (for al_publish, al_run_tests)
```

On startup, the bridge:

1. Reads `config.ts` — resolves `AL_LS_PATH`, discovers AL project folders (upward walk first, then downward scan), reads `.vscode/settings.json`, resolves analyzer paths and sibling DLLs, merges rulesets.
2. Spawns the AL language server binary as a child process with stdio pipes.
3. Connects a `vscode-jsonrpc` `MessageConnection` to the child's stdout/stdin.
4. Sends LSP `initialize` + `initialized`, then `al/setActiveWorkspace` for each discovered AL project folder.
5. Starts the `McpServer` and connects it to its own stdio transport.
6. LSP init runs in the background; each tool call awaits the `lspReady` promise before forwarding to the LS. The MCP server is immediately responsive to `listTools` while the LS warms up.
7. On `SIGINT`/`SIGTERM`, disposes the JSON-RPC connection and kills the child process.

Each tool call:
1. Awaits `lspReady`.
2. Calls `client.openDocument(file)` — idempotent; sends `textDocument/didOpen` only on first call per URI, caches the version number.
3. Forwards the LSP request and normalizes the response.
4. Returns a JSON-serialized result wrapped in `{ content: [{ type: "text", text: "..." }] }`.

### Diagnostics flow

The AL LS publishes diagnostics on two channels:

- **Push** (`textDocument/publishDiagnostics`): compiler-level and some analyzer diagnostics arrive here after `didOpen` / `didChange`. The bridge's `DiagnosticsCache` stores the latest batch per URI and notifies any awaiting callers.
- **Pull** (`textDocument/diagnostic`, LSP 3.17): third-party analyzers (ALCops, LinterCop) route their findings exclusively through pull. `al_get_diagnostics` queries both channels and merges the results, deduplicating on `(code, range, message)`.

`al_apply_edit` waits `AL_DIAGNOSTICS_SETTLE_MS` milliseconds for the next push notification before returning. This covers the AL LS's asynchronous re-analysis delay after a `didChange`. Increase this value on slow machines or large projects.

### Analyzer sibling loading

The AL LS's Roslyn host runs each `codeAnalyzers` entry in a separate `AssemblyLoadContext`. This means `BusinessCentral.LinterCop.dll` cannot load `Microsoft.Dynamics.Nav.Analyzers.Common.dll` even when both live in the same `Analyzers/` folder.

The fix: `config.ts:augmentWithAnalyzerSiblings()` scans the configured analyzer list against known sibling rules and prepends the required helper DLLs. The helpers load into the shared ALC first, so the main analyzer can resolve them at probe time.

Without this fix, every affected analyzer throws `System.IO.FileNotFoundException` in its `Initialize` override and the LS surfaces it as `AD0001` on `app.json`. The `DiagnosticAnalyzer` side fails; the `CodeFixProvider` still registers, so quickfixes appear in `al_list_code_actions` but no rules ever fire.

### Code action reverse-request protocol

The AL LS does not implement the standard LSP code action execution path. Instead:

1. `textDocument/codeAction` returns `ProtocolCodeAction[]` where each entry's `command.command` is `"al/runCodeAction"` and `command.arguments[0]` is the action payload.
2. To execute, the bridge sends `al/runCodeAction` with that payload.
3. The LS responds with an empty acknowledgement, then sends a *reverse* `workspace/applyEdit` request back to the bridge, keyed by the action's `identifier` in `label`.

The bridge handles this by registering a waiter in `AlLspClient.applyEditWaiters` *before* sending the `al/runCodeAction` request, then resolving the waiter when the inbound `workspace/applyEdit` arrives.

### Credential storage

For `al_run_tests` and `al_publish`, credentials are resolved in this order:

1. **Environment variables** (`BC_USER` + `BC_PASSWORD`) — preferred for ephemeral sessions and CI.
2. **Credentials file** at `$XDG_CONFIG_HOME/al-mcp-bridge/credentials.json` (default: `~/.config/al-mcp-bridge/credentials.json`).

The credentials file must be mode `0600` (owner read/write only). The bridge refuses to read it if any group or world bits are set.

File format:

```json
{
  "https://bc.example.com:7049|BC240": {
    "username": "admin",
    "password": "secret"
  },
  "bc.example.com|*": {
    "username": "admin",
    "password": "secret"
  }
}
```

The key is `<origin>|<serverInstance>` (or `<origin>|*` as a wildcard). The bridge tries multiple key variants (with/without scheme, with/without port, `|` and `_` separators) so you don't need to guess the exact format.

Passwords are never logged, never returned in MCP responses, and scrubbed from error strings by the `redact()` function before they leave the tool module.

---

## 10. Available scripts

| Script | Command | Description |
|---|---|---|
| **Build** | `npm run build` | Compile TypeScript to `dist/` |
| **Watch** | `npm run dev` | Compile in watch mode |
| **Type check** | `npm run typecheck` | Type-check without emitting output |
| **Start** | `npm start` | Run `dist/index.js` directly |
| **Install AL LS** | `npm run install:al-ls` | Download or locate the AL language server for the test suite |
| **AL LS status** | `npm run install:al-ls:status` | Show the current AL LS installation |
| **Run tests** | `npm test` | Build, install AL LS, then run e2e tests |
| **E2e only** | `npm run test:e2e` | Run e2e tests (skip build/install steps) |
| **Smoke** | `node scripts/smoke.mjs <al-file>` | Quick smoke test against a real AL file |
| **Smoke compile** | `node scripts/smoke-compile.mjs` | Smoke test: `al_compile` |
| **Smoke publish** | `node scripts/smoke-publish.mjs` | Smoke test: `al_publish` |
| **Smoke run tests** | `node scripts/smoke-runtests.mjs` | Smoke test: `al_run_tests` |

---

## 11. Testing

### Running the test suite

```bash
# Full run (build + install AL LS + e2e tests)
npm test

# Fast iteration (assumes already built and AL LS installed)
npm run test:e2e
```

### What the tests cover

The e2e suite (`tests/e2e/diagnostics.analyzers.test.mjs`) verifies that every MS ALCops analyzer bundled with the AL extension surfaces diagnostics correctly through the bridge:

| Test | Rule | Severity | Source |
|---|---|---|---|
| CodeCop unused local variable | `AA0137` | warning | `Microsoft.Dynamics.Nav.CodeCop.dll` |
| AppSourceCop missing ApplicationArea | `AS0062` | error | `Microsoft.Dynamics.Nav.AppSourceCop.dll` |
| UICop missing UsageCategory | `AW0006` | info | `Microsoft.Dynamics.Nav.UICop.dll` |
| PerTenantExtensionCop missing ApplicationArea | `PTE0008` | error | `Microsoft.Dynamics.Nav.PerTenantExtensionCop.dll` |
| ALCops.LinterCop cognitive complexity | `LC0090` | warning | `ALCops.LinterCop.dll` |
| ApplicationCop permission set caption | `AC0009` | warning | `ALCops.ApplicationCop.dll` |
| AL compiler object name too long | `AL0305` | error | AL compiler |
| Socitas.ReviewerCop (optional) | `CC0009` | warning | `Socitas.ReviewerCop.dll` (gated on `AL_REVIEWERCOP_DLL`) |

### Fixture project

The test fixture at `tests/fixtures/analyzers-sanity/` is a minimal AL project that intentionally violates the rules above. Its `.vscode/settings.json` loads all MS cops plus `ALCops.LinterCop.dll` and `ALCops.ApplicationCop.dll`.

### Test helper: `startBridge`

`tests/helpers/bridge.mjs` exports `startBridge(opts)` which spawns the built MCP bridge as a child process, connects an `@modelcontextprotocol/sdk` client over stdio, and returns a wrapper with `callTool(name, args)` and `close()`. Tests use `waitFor(probe, { timeoutMs, intervalMs, label })` to poll for asynchronous analyzer diagnostics.

### Running a single optional test

The `Socitas.ReviewerCop` test is skipped by default. To enable it:

```bash
AL_REVIEWERCOP_DLL="/path/to/Socitas.ReviewerCop.dll" npm run test:e2e
```

---

## 12. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Bridge exits immediately: `Could not locate the AL language server` | AL extension not installed or `AL_LS_PATH` points to a non-existent path | Install `ms-dynamics-smb.al` from the VS Code Marketplace, or set `AL_LS_PATH` to the absolute binary path |
| `al_list_objects` returns `null` or empty | Workspace didn't load — usually wrong `AL_WORKSPACE` or no `app.json` at that path | Point `AL_WORKSPACE` at the folder containing `app.json` |
| `al_document_outline` returns `{outline: []}` on a non-empty file | `.alpackages` missing or wrong; LS couldn't resolve project dependencies | Run *AL: Download symbols* in VS Code on the same project; verify `*.app` files are in `.alpackages/`; set `AL_PACKAGE_CACHE` to that folder |
| Bridge hangs on startup, no stderr | AL LS binary path is wrong and the spawn fails silently | Check `AL_LS_PATH`; run `ls "$AL_LS_PATH"` to confirm the binary exists |
| `al_get_diagnostics` shows `AD0001 … Could not load file or assembly …Common…` on `app.json`, no LinterCop/ALCops diagnostics appear | Roslyn's per-entry `AssemblyLoadContext` can't find the analyzer's sibling DLL | The bridge auto-patches known analyzers (LinterCop, CodeCop, ALCops, ReviewerCop). If the `AD0001` persists for an unknown analyzer, add the missing sibling DLL to `al.codeAnalyzers` explicitly |
| LinterCop rules missing on files you haven't opened | `al.backgroundCodeAnalysis` defaults to `"File"` | Set `"al.backgroundCodeAnalysis": "Project"` in `.vscode/settings.json` |
| ALCops quickfixes don't appear in `al_list_code_actions` | Analyzer not loaded, or the specific rule has no `CodeFixProvider` | Verify the DLL path in `al.codeAnalyzers`; try a rule you know ships a CodeFix (e.g. `AA0137`) |
| `al_run_tests` / `al_publish` fail with auth error | Wrong credentials or credential file permissions | Set `BC_USER`/`BC_PASSWORD` env vars, or ensure `~/.config/al-mcp-bridge/credentials.json` is mode `0600` and contains the right key |
| `al_run_tests` fails with `An unexpected error occurred invoking 'Initialize'` | Parallel call hit BC's single-session lock — or the company name in `launch.json` doesn't match | The bridge serializes calls per hub; if you see this, another process holds the hub. For company issues, pass `company: ""` explicitly. |
| `al_compile` fails: `AL compiler not found` | `alc` binary not beside the LS binary | The bridge derives the `alc` path from `AL_LS_PATH`. Ensure `AL_LS_PATH` points to the EditorServices host that ships in the same folder as `alc`. |
| Claude doesn't see the tools | MCP server crashed on launch | Run `/mcp` in Claude Code to check server status and logs. Run `node scripts/smoke.mjs <file>` standalone to isolate the issue. |
| Diagnostics settle too fast, `al_apply_edit` returns empty | `AL_DIAGNOSTICS_SETTLE_MS` too low for your machine | Increase to `2000` or more: set `AL_DIAGNOSTICS_SETTLE_MS=2000` in the MCP server env |

---

## 13. Design notes

### Why MCP and not a CLI?

The tool surface is ~13 structured operations with typed parameters and typed return payloads. MCP's tool schema enables AI parameter validation, tool discovery, and parallel calls. A CLI + Bash approach works for 2-3 primitives that return plain text; it breaks down at this scale.

### Why TypeScript and not C# or Python?

- `@modelcontextprotocol/sdk` and `vscode-jsonrpc` / `vscode-languageserver-protocol` are the most mature MCP + LSP client libraries outside the AL extension itself.
- ESM Node.js packaging (`npx`-style) has the least install friction.
- The same language is used by `bc-code-intelligence-mcp`, making contributions easier.

C# would offer in-proc access to `Microsoft.Dynamics.Nav.CodeAnalysis` primitives (extract method, inline variable, syntax trees), which LSP does not expose. That's out of scope for the LSP bridge but worth revisiting for a future in-proc companion tool.

### What the bridge does NOT do

- Reimplement the AL parser, formatter, or symbol resolver — everything semantic goes through the real LS.
- Provide a web UI or dashboard.
- Replace the AL extension's own MCP tools — it adds the semantic-edit surface that's missing from them.
- Modify or patch the Microsoft AL extension in any way.
- Support AL SaaS (Business Central Online) for `al_run_tests` and `al_publish` — only on-premise with `UserPassword` auth is implemented.

### Planned tools

- `al_list_event_publishers` — via `al/getEventPublishersRequest`
- `al_check_symbols` — via `al/checkSymbols`, post-edit validation
