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

### Discovery
- `al_list_objects(filter?)` — list AL objects via `al/getApplicationObjects`
- `al_get_object(type, id|name)` — full metadata via `al/getApplicationObject`
- `al_list_event_publishers(objectFilter?)` — via `al/getEventPublishersRequest`
- `al_symbol_search(query, filters)` — forwards to `al/symbolSearch`

### Semantic query
- `al_get_symbol_at(file, offset)` — combines `hover` + `definition` +
  optional reference count. Saves the AI from re-reading whole files.
- `al_find_references(file, offset)` — `textDocument/references`
- `al_document_outline(file)` — `textDocument/documentSymbol`

### Edit loop (core value)
- `al_preview_edit(file, edits[])` — `didChange` → await diagnostics →
  return `{ diagnostics, newText }` without persisting
- `al_apply_edit(file, edits[])` — preview + commit to disk + re-diagnose
- `al_get_diagnostics(file | project)` — current cached diagnostics
- `al_format(file, range?)` — returns formatted text

### Refactor
- `al_rename(file, offset, newName)` — `textDocument/rename`, returns
  `WorkspaceEdit` for review. Does **not** auto-apply.
- `al_list_code_actions(file, range)` — `textDocument/codeAction`, filter
  out the ones an AI shouldn't trigger blindly (e.g. pragma-suppress)
- `al_run_code_action(file, range, identifier)` — `al/runCodeAction`
- `al_check_symbols(file)` — `al/checkSymbols`

### Later / out of MVP
- `al_extract_method`, `al_inline` — need C# in-proc primitives
- `al_test_run(snippet)` — stands up a transient runner (see AL.Runner for
  inspiration) to actually execute generated code without a BC Service Tier

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

1. **M1 — spawn & handshake.** Locate AL LS, `initialize`, accept a workspace
   path from config. Ship `al_document_outline` as smoke test.
2. **M2 — edit loop.** `al_apply_edit` + `al_preview_edit` +
   `al_get_diagnostics`. This is the unlock for every other AI-authored
   change.
3. **M3 — semantic query.** `al_find_references`, `al_get_symbol_at`,
   `al_rename`.
4. **M4 — discovery.** `al_symbol_search`, `al_list_objects`,
   `al_list_event_publishers`.
5. **M5 — code actions.** `al_list_code_actions`, `al_run_code_action`,
   `al_format`.
6. **M6 — optional runner.** Wrap `BusinessCentral.AL.Runner` for
   `al_test_run` behavioural validation.

---

## 9. Local dev

```bash
npm install
npm run build
AL_LS_PATH="C:/Users/you/.vscode/extensions/ms-dynamics-smb.al-<ver>/bin/Microsoft.Dynamics.Nav.EditorServices.Host.exe" \
AL_WORKSPACE="C:/path/to/your/al/project" \
node dist/index.js
```

Claude Code / Claude Desktop registration example in `examples/mcp.json`.
