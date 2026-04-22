# al-mcp-bridge

**Semantic AL refactoring MCP server.** Thin bridge between AI agents (Claude,
Copilot, anything speaking MCP) and the AL language server bundled with the
Microsoft AL VS Code extension.

The goal is narrow: give an AI the *semantic* primitives it needs to write,
review, and refactor AL code with the same correctness guarantees a human
developer gets from the editor — rename across a project, find references,
validate an edit against the real compiler, format the result.

---

## 1. Is MCP the right tool here?

Short answer: **yes, as the interface; LSP is the engine**. But it's worth
being explicit about what was considered.

| Option | Verdict |
|---|---|
| **External MCP server → LSP** (this repo) | Right choice. Standardized tool schema for AI, zero modification of the signed Microsoft extension, reuses the AL language server as-is. |
| Add C# tools inside the AL extension's own MCP surface (`almcp_*` pattern) | Technically superior (in-proc `SyntaxEditor`, `SemanticModel`) but requires patching or side-loading into a signed MS extension. High friction. Worth revisiting once the LSP-bridge surface is proven. |
| CLI invoked via Bash | Loses structured parameter schemas, tool discovery, and parallel tool calls. Fine for 2-3 primitives, breaks down at 10+. |
| Claude Code **sub-agent** or **skill** only | Works for static guidance ("how to structure a codeunit") but has no hook into the real compiler. Useful *alongside* this, not instead of it. |
| Reuse `bc-code-intelligence-mcp` | That's a **knowledge/persona** MCP (14 BC expert personas, embedded docs). Orthogonal. Pair with this, don't compete. |
| Reuse the AL extension's existing MCP tools (`al_symbolsearch`, `al_compile`, …) | Complementary. They cover build/search. The semantic rewrite loop (rename, references, edit+validate, format) is **not exposed**. That's the gap this repo fills. |

**When MCP would be the *wrong* answer:** if the full tool set is ≤3
primitives and each call returns plain text, a CLI + Bash is simpler. That's
not the case here — the tool surface is ~15 structured operations with typed
parameters and typed return payloads, which is exactly MCP's sweet spot.

---

## 2. What an AI actually needs to write / review / refactor AL

Reframing away from the VS Code quick-fix mental model. The AI's job looks
like three loops:

### Writing loop
1. **Discover** what exists → objects, symbols, event publishers, existing
   patterns in the project.
2. **Generate** candidate code.
3. **Validate** → parse + compile + semantic diagnostics *before* committing.
4. **Format** → normalize whitespace/indentation so later diffs stay clean.

### Review loop
1. **Resolve** the meaning of a symbol at a location — what is it, where's
   it defined, what's its signature, what's the XML doc.
2. **Trace** all references / callers.
3. **Enumerate** outstanding diagnostics on the file or project.

### Refactor loop
1. **Preview** an edit — apply in-memory, collect diagnostics, show diff. No
   disk write yet.
2. **Rename semantically** — one call, propagates across the project, not a
   text replace.
3. **Apply** — commit the edit if the preview was clean.
4. **Bulk** — run an existing registered code-fix across a scope
   (Document/Project) for mechanical migrations.

The primitive that matters most is **edit → diagnostics in the same round
trip**. Every other tool is in service of making that tight.

---

## 3. What the AL language server actually provides (verified)

Endpoints pulled from the decompiled extension at `../csharp-dump/decompiled/`.
All of these are already live — no extension modification required.

### Standard LSP (directly usable)

| Endpoint | Purpose in this toolkit |
|---|---|
| `textDocument/didOpen`, `didChange`, `didClose` | In-memory buffer the AI writes into before committing to disk |
| `textDocument/publishDiagnostics` (push notification) | **The validation feedback channel.** Core of the edit → validate loop |
| `textDocument/definition` | Resolve symbol → origin |
| `textDocument/references` | Find all usages |
| `textDocument/rename` | Semantic rename, returns a `WorkspaceEdit` |
| `textDocument/hover` | Type + XML doc at position |
| `textDocument/documentSymbol` | File outline (objects, triggers, procedures, fields) |
| `textDocument/formatting`, `rangeFormatting` | AL-aware formatter |
| `textDocument/codeAction` | List available fixes/refactorings at a range |
| `workspace/applyEdit` (server → client) | Response shape for rename / codeAction |

### AL-specific (`al/*` — also usable)

| Endpoint | Purpose |
|---|---|
| `al/symbolSearch` | Full-text + filter search across project + dependencies. See `SymbolSearchService.SearchAsync` (decompiled/workspaces/…LanguageModelTools.SymbolSearch/SymbolSearchService.cs:419) |
| `al/getApplicationObjects` / `al/getApplicationObject` | Enumerate / fetch object metadata without reading files |
| `al/getEventPublishersRequest` | Discover event hooks — replaces guessing in generated subscriber code |
| `al/runCodeAction` | Execute a specific code action by identifier (returns `WorkspaceEdit`). Handler: `RunCodeActionCommandHandler` (decompiled/es_proto.cs:17426) |
| `al/gotodefinition` | AL-specific definition (handles objects, symbol packages) |
| `al/checkSymbols` | Verify referenced symbols still resolve after an edit |
| `al/mcp/listTools`, `al/mcp/invokeTool`, `al/mcp/listResources`, … | The LS's own MCP bridge (see es_proto.cs:13247+). Lets us reach `al_symbolsearch` and friends *through* LSP instead of a second transport. |

### Things LSP does **not** expose (out of scope for this bridge)

- Raw parse-only validation without touching a workspace buffer — would need
  an in-proc C# tool on `Microsoft.Dynamics.Nav.CodeAnalysis.Parser`.
- Extract method, inline variable — not exposed as code actions or LSP
  endpoints. Would need C# provider.
- `SyntaxEditor` transactional tree edits — same, C# only.

Workaround for parse-only validation: `didOpen` on a scratch URI, read back
diagnostics, `didClose`. Good enough for first iteration.

---

## 4. MCP tool roster

Names follow the existing `al_*` convention so they sit naturally next to
`al_symbolsearch`, `al_compile`, etc.

### Shipped (callable today)
- `al_list_objects` — AL objects via `al/getApplicationObjects`
- `al_symbol_search` — forwards to `al/symbolSearch` (filter by kind, scope, access, …)
- `al_get_symbol_at` — combined hover + definition at (file, line, char)
- `al_find_references` — `textDocument/references`
- `al_document_outline` — `textDocument/documentSymbol`
- `al_apply_edit` — full-text replace + fresh diagnostics in the same call
  (set `persist=false` for preview mode, in-memory only)
- `al_get_diagnostics` — current cached diagnostics, optional `waitForFresh`
- `al_format` — full-document or range formatting
- `al_rename` — semantic rename, returns a `WorkspaceEdit` for review. Does
  **not** auto-apply — feed the resulting text through `al_apply_edit`.
- `al_list_code_actions` — `textDocument/codeAction` over a range; filterable
  by `only` (e.g. `['quickfix']`). Works with loaded analyzer DLLs (ALCops,
  LinterCop) configured via `al.codeAnalyzers` in `.vscode/settings.json`.
- `al_run_code_action` — executes one, applies the resulting `WorkspaceEdit`,
  returns per-file before/after + fresh diagnostics.

### Planned
- `al_list_event_publishers` — via `al/getEventPublishersRequest`
- `al_check_symbols` — via `al/checkSymbols`, post-edit validation
- `al_test_run(snippet)` — wrap `BusinessCentral.AL.Runner` to execute
  generated code without a BC Service Tier
- `al_extract_method`, `al_inline` — need C# in-proc primitives (out of scope
  for the LSP bridge)

---

## 5. Architecture

```
┌────────────────┐    MCP stdio      ┌─────────────────┐    LSP stdio    ┌──────────────────┐
│  Claude / any  │ ◀───────────────▶ │  al-mcp-bridge  │ ◀─────────────▶ │  AL language     │
│  MCP client    │   tool calls      │  (this repo)    │   JSON-RPC      │  server (bundled │
└────────────────┘                   └─────────────────┘                 │  with AL ext.)   │
                                              │                          └──────────────────┘
                                              └── optional: `alc.exe` for compile/package
```

Single process. Stateless MCP surface; per-workspace LSP session managed
internally. On startup:

1. Locate AL extension at
   `%USERPROFILE%/.vscode/extensions/ms-dynamics-smb.al-*/bin/` (or
   respect `AL_LS_PATH` env var).
2. Spawn the AL language server binary.
3. Send `initialize` with the workspace root.
4. Open files on-demand when a tool call references them (lazy `didOpen`).
5. Cache `publishDiagnostics` per-URI keyed on version so tools can
   return "diagnostics after my edit" deterministically.

---

## 6. Language choice: TypeScript

| Criterion | TS | Python | C# | Go |
|---|---|---|---|---|
| MCP SDK maturity | ★★★ (`@modelcontextprotocol/sdk`) | ★★★ | ★★ | ★ |
| LSP client libs | ★★★ (`vscode-jsonrpc`, `vscode-languageserver-protocol` — same ones the AL ext. uses) | ★★ | ★★★ (StreamJsonRpc) | ★ |
| Subprocess / stdio | ★★★ | ★★★ | ★★★ | ★★★ |
| Packaging / install friction | ★★★ (npx) | ★★ | ★★ (dotnet tool) | ★★★ |
| Matches neighbour project conventions | ★★★ (bc-code-intelligence-mcp is TS) | | | |

TypeScript wins on every axis except "access to in-proc AL primitives" —
which LSP already externalizes for us.

---

## 7. Non-goals

- **No reimplementation** of the AL parser, formatter, or symbol resolver.
  Everything semantic goes through the real LS.
- **No web UI**, no dashboards. This is a plumbing layer.
- **No replacement** for the AL extension's own MCP tools. This adds the
  missing semantic-edit surface next to them.
- **No direct modification** of the Microsoft AL extension. We run
  the LS in a separate process we control.

---

## 8. Milestones

1. **M1 — spawn & handshake.** ✅ Locate AL LS, `initialize`,
   `al/setActiveWorkspace`. `al_document_outline` as smoke test.
2. **M2 — edit loop.** ✅ `al_apply_edit` + `al_get_diagnostics`. Preview
   mode via `persist=false`.
3. **M3 — semantic query.** ✅ `al_find_references`, `al_get_symbol_at`,
   `al_rename`, `al_format`.
4. **M4 — discovery.** ✅ `al_symbol_search`, `al_list_objects`.
5. **M5 — code actions.** ✅ `al_list_code_actions`, `al_run_code_action`
   (handles AL's reverse-`workspace/applyEdit` protocol). Analyzer loading
   from `.vscode/settings.json` (`al.codeAnalyzers`, ALCops, LinterCop).
6. **M5.1 — remaining.** `al_list_event_publishers`, `al_check_symbols`.
7. **M6 — optional runner.** Wrap `BusinessCentral.AL.Runner` for
   `al_test_run` behavioural validation.

---

## 9. Quick start

### Prerequisites
- **Node 20+**
- **Microsoft AL VS Code extension installed** — the bridge spawns the AL
  language server binary shipped with it. Install from the Marketplace
  (`ms-dynamics-smb.al`) if you haven't; you don't need VS Code to be
  running.
- **An AL project** with an `app.json` at its root and a populated
  `.alpackages/` folder (run *AL: Download symbols* in VS Code once to
  populate it, or copy from another project).

### Install & build
```bash
git clone <this-repo>
cd al-mcp-bridge
npm install
npm run build
```

### Locate the AL language server
The binary lives inside the installed extension:

```bash
# Windows (bash/git-bash)
ls ~/AppData/Roaming/Code/User/globalStorage/  # not this one — actual path:
ls "$USERPROFILE/.vscode/extensions" | grep ms-dynamics-smb.al
# → ms-dynamics-smb.al-17.0.2273547   (pick the newest)
# Full path:
# C:/Users/<you>/.vscode/extensions/ms-dynamics-smb.al-<ver>/bin/win32/Microsoft.Dynamics.Nav.EditorServices.Host.exe

# Linux / macOS
ls ~/.vscode/extensions | grep ms-dynamics-smb.al
# binary under bin/linux/ or bin/darwin/
```

If you skip `AL_LS_PATH`, the bridge auto-detects the newest install under
`~/.vscode/extensions/ms-dynamics-smb.al-*/bin/`.

### Environment variables

| Var | Required | Purpose |
|---|---|---|
| `AL_LS_PATH` | no (autodetect) | Absolute path to `Microsoft.Dynamics.Nav.EditorServices.Host(.exe)` |
| `AL_WORKSPACE` | **yes** | Absolute path to your AL project root (the folder containing `app.json`) |
| `AL_PACKAGE_CACHE` | **yes** | Absolute path to `.alpackages` (semicolon-separated for multiple) — without this, BC symbols won't resolve and most tools return empty |
| `AL_EXTRA_CODE_ANALYZERS` | no | Semicolon-separated list of analyzer DLLs to load **in addition to** `al.codeAnalyzers` from `.vscode/settings.json`. Supports the same `${analyzerFolder}` / `${workspaceFolder}` / `${CodeCop}` / etc. placeholders |
| `AL_EXTRA_RULESETS` | no | Semicolon-separated list of `*.ruleset.json` paths to apply **in addition to** `al.ruleSetPath`. If more than one ruleset is in play (settings + extras), the bridge synthesizes a composite that `includedRuleSets`-chains them and points the LS at it |
| `AL_DIAGNOSTICS_SETTLE_MS` | no (default `750`) | How long `al_apply_edit` waits for the next `publishDiagnostics` before returning |

### Analyzer configuration (from `.vscode/settings.json`)

The bridge reads analyzer configuration from `<AL_WORKSPACE>/.vscode/settings.json` — the same file VS Code reads. No env vars, no duplication. Supported keys:

| Key | Purpose |
|---|---|
| `al.codeAnalyzers` | Array of analyzer DLLs. Supports the same placeholders VS Code does: `${analyzerFolder}`, `${AppSourceCop}`, `${CodeCop}`, `${PerTenantExtensionCop}`, `${UICop}`, `${workspaceFolder}`, `${alWorkspaceFolder}` |
| `al.enableCodeAnalysis` | Master switch. Default `true` |
| `al.enableCodeActions` | Enable `textDocument/codeAction`. Default `true` |
| `al.backgroundCodeAnalysis` | `"File"`, `"Project"`, or `"Off"`. Default `"File"`. Set to `"Project"` to run LinterCop-style analyzers across the whole project and surface their diagnostics without opening every file |
| `al.ruleSetPath` | Path to a `*.ruleset.json` (relative to workspace root or absolute). Filters and tunes severities |
| `al.assemblyProbingPaths` | Extra directories the CLR scans for analyzer-referenced DLLs. Merged with auto-detected analyzer directories |

The bridge automatically adds every analyzer's parent directory to `assemblyProbingPaths` so sibling helper DLLs resolve. If LinterCop is enabled, `Microsoft.Dynamics.Nav.Analyzers.Common.dll` (the helper it depends on) is auto-included in the analyzer list — without this, Roslyn's per-entry `AssemblyLoadContext` isolation causes LinterCop to fail at probe time with `AD0001` and no rules fire.

**Forcing extra analyzers / rulesets (env var overlay).** When you want a linter enabled *regardless* of what VS Code has in `.vscode/settings.json` — typical case: enforcing a house linter with quickfixes on every AL project — set `AL_EXTRA_CODE_ANALYZERS` and/or `AL_EXTRA_RULESETS` on the MCP server itself. These stack on top of the workspace settings; they don't replace them. Example:

```jsonc
// .mcp.json
{
  "mcpServers": {
    "al": {
      "command": "node",
      "args": ["C:/git/al-mcp-bridge/dist/index.js"],
      "env": {
        "AL_WORKSPACE": "${workspaceFolder}",
        "AL_PACKAGE_CACHE": "${workspaceFolder}/.alpackages",
        "AL_EXTRA_CODE_ANALYZERS": "${analyzerFolder}BusinessCentral.LinterCop.dll;C:/team/analyzers/HouseRules.dll",
        "AL_EXTRA_RULESETS": "C:/team/rulesets/house.ruleset.json;${workspaceFolder}/.codeanalyzer/project.ruleset.json"
      }
    }
  }
}
```

Entries are semicolon-separated, support the same placeholders as `.vscode/settings.json`, and missing paths log a warning to stderr and are skipped. When multiple rulesets are active, the bridge writes a composite ruleset under the OS temp dir that chains each source via `includedRuleSets` and passes that path to the LS — a single `ruleSetPath` is all the LSP accepts.

### Smoke test

End-to-end check against a real AL file (verifies LSP handshake, tool
registration, and `al/getApplicationObjects` + `textDocument/documentSymbol`
responses):

```bash
AL_WORKSPACE="C:/path/to/your/al/project" \
AL_PACKAGE_CACHE="C:/path/to/your/al/project/.alpackages" \
node scripts/smoke.mjs "C:/path/to/your/al/project/src/some.al"
```

Expected: 9 tools listed, non-empty `al_list_objects`, outline tree with
your object(s) and their members.

---

## 10. Claude Code integration

The bridge is a stdio MCP server. Claude Code picks it up via any of the
standard MCP registration paths.

### Option A: project-local (recommended for a team)

Commit an `.mcp.json` at your **AL project** root (the folder containing
`app.json` — *not* this repo):

```json
{
  "mcpServers": {
    "al": {
      "command": "node",
      "args": ["C:/git/al-mcp-bridge/dist/index.js"],
      "env": {
        "AL_WORKSPACE": "${workspaceFolder}",
        "AL_PACKAGE_CACHE": "${workspaceFolder}/.alpackages"
      }
    }
  }
}
```

Claude Code will prompt on first launch to trust the new MCP server. After
that, the `al_*` tools appear in `/tools` and are callable from any prompt.

### Option B: user-wide

Add the same `mcpServers.al` block to `~/.claude.json` (Windows:
`%USERPROFILE%/.claude.json`). The server is then available in every
session, regardless of working directory. Useful if you hop between
multiple AL projects — just set `AL_WORKSPACE` via the shell before
launching Claude Code, or keep it unset and let each project override via
its own `.mcp.json`.

### Option C: `claude mcp add`

One-shot registration from the CLI:

```bash
claude mcp add al -- node C:/git/al-mcp-bridge/dist/index.js
# then set env vars in ~/.claude.json under mcpServers.al.env
```

### Verifying it's live

Inside a Claude Code session, in the working directory of your AL project:

```
/mcp
```

You should see `al` listed with 11 tools. Try:

> "List all codeunits in this workspace and show me the outline of
> src/pageextension/CustomerListExt.PageExt.al"

The agent should call `al_list_objects` (filtered) and
`al_document_outline` and return structured results.

### Enabling ALCops / LinterCop quickfixes

Analyzer configuration lives in `<workspace>/.vscode/settings.json`, so the same file that drives your VS Code editor experience drives the bridge:

```jsonc
{
  "al.codeAnalyzers": [
    "${CodeCop}",
    "${UICop}",
    "${analyzerFolder}BusinessCentral.LinterCop.dll",
    "${analyzerFolder}ALCops.dll"
  ],
  "al.enableCodeAnalysis": true,
  "al.enableCodeActions": true,
  "al.backgroundCodeAnalysis": "Project",
  "al.ruleSetPath": ".codeanalyzer/SOCITAS.ruleset.json"
}
```

Restart Claude Code. Ask the agent to run `al_list_code_actions` over a range that trips an analyzer rule; the returned list should include entries from those DLLs. Feed the `identifier` into `al_run_code_action` to execute the fix.

`al_get_diagnostics` surfaces every active rule — compiler (`AL*`), CodeCop (`AA*`), LinterCop (`LC*`), ALCops (`PC*`), AppSourceCop (`AS*`), etc. Set `backgroundCodeAnalysis` to `"Project"` when you want project-wide diagnostics without opening each file.

### Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `al_list_objects` returns `null` or empty | Workspace didn't load — usually missing/wrong `AL_WORKSPACE` or no `app.json` at that path | Point `AL_WORKSPACE` at the folder containing `app.json` |
| `al_document_outline` returns `{outline: []}` on a non-empty file | `.alpackages` missing or wrong; LS couldn't bind project references | Run *AL: Download symbols* in VS Code on the same project, verify `.alpackages/*.app` files exist, point `AL_PACKAGE_CACHE` at that folder |
| Bridge hangs on startup, no stderr | AL LS binary path wrong | Check `AL_LS_PATH` or unset it to use autodetect |
| ALCops rules don't appear in `al_list_code_actions` | Analyzer DLL path wrong in `al.codeAnalyzers`, or analyzer has diagnostics but no `CodeFixProvider` for the specific rule | Verify the DLL path in `.vscode/settings.json`; try a rule you know ships a CodeFix |
| `al_get_diagnostics` shows `AD0001 … Could not load file or assembly Microsoft.Dynamics.Nav.Analyzers.Common` on `app.json`, no LinterCop/ALCops diagnostics appear | Roslyn's per-entry `AssemblyLoadContext` couldn't resolve a sibling helper DLL at analyzer probe time | The bridge now auto-includes `Microsoft.Dynamics.Nav.Analyzers.Common.dll` whenever LinterCop is in `al.codeAnalyzers`. If you still see this on a different analyzer, add the missing sibling DLL to `al.codeAnalyzers` explicitly so it co-loads in the shared ALC |
| LinterCop rules missing on files you haven't opened | `al.backgroundCodeAnalysis` set to `"File"` (default) | Set `"al.backgroundCodeAnalysis": "Project"` in `.vscode/settings.json` |
| Claude doesn't see the tools | MCP server crashed on launch | Check Claude Code's MCP logs (`/mcp` shows status); run the smoke test standalone to isolate |

See [`examples/mcp.json`](examples/mcp.json) for a complete annotated config.
