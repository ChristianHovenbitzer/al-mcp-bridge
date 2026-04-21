# Missing / Wanted MCP Tools

Notes from a real code-review session (AL carrier integration, ~5 codeunits,
~900 LOC) on what the bridge surfaces well vs. where I had to leave the MCP
and go back to raw `Read`/`Grep`. Ordered by how often I reached for the
missing capability.

---

## 0. Disk-changed-on-LSP resync (**new blocker — found during edit pass**)

**Use case.** Edit a file with a non-bridge tool (native `Edit`/IDE/raw
file write) and call `al_get_diagnostics` with `waitForFresh: true`.
Result: stale `[]` every time, because the LSP's open-buffer version
never received a `didChange` and `openDocument()` is idempotent (it
returns the cached URI without re-reading from disk).

Found during a code-review → fix pass: I used `Edit` to patch three
callsites in `FerrariCarrier.Codeunit.al`, then asked the bridge for
diagnostics. Got `[]`. Had to re-send the *entire* file via
`al_apply_edit` (with identical content to what was already on disk)
just to force a `didChange` that made the LSP re-analyze. This is
non-obvious, slow, and silently produces false negatives.

**Proposed fix — pick one:**

1. **FS watcher in the bridge** — on `didChange` outside the bridge's
   own writes, re-read + `textDocument/didChange` the LSP. Most
   robust. One chokidar dependency.
2. **`al_resync_file({ file })` tool** — explicit caller-driven resync.
   Reads disk, compares to the cached open-version text, sends
   `didChange` if different. Cheap to implement.
3. **Make `al_get_diagnostics` self-healing** — on every call, compare
   disk mtime to a tracked "last synced" timestamp per URI and
   re-send `didChange` if stale. Invisible to callers.

Option 3 is the best UX; option 2 is the smallest PR.

**Acceptance.** After this, the flow "Edit → al_get_diagnostics" returns
*disk-truth* diagnostics without the caller needing to know about the
open-buffer version model.

---

## 1. Working LinterCop diagnostics (**blocker**)

Already captured in [TODO.md](./TODO.md). The bridge currently cannot
surface LinterCop rule hits (`LC0001`, `LC0015`, `LC0033`, `LC0052`, …)
because `Microsoft.Dynamics.Nav.Analyzers.Common.dll` fails to load at
analyzer probe time. Until this is fixed, `al_get_diagnostics` on a clean
compiler build returns `[]` for *every* file regardless of style quality —
which means the tool gives the same answer for "actually clean" and
"riddled with `var` params never assigned". That's the #1 review-quality
gap.

---

## 2. `al_find_callers` / call hierarchy

**Use case.** During review I needed to decide whether a 2-arg overload
`BuildCreateOrderRequest(header, setup)` was used in production or only
from tests. `al_find_references` works if I already have the exact cursor
position on the declaration — but the natural question is "given symbol
`Ferrari Request Builder::BuildCreateOrderRequest` with arity 2, where is
it called from?" without needing to open the declaration file first.

**Proposed shape.**
```
al_find_callers({
  symbol: "Ferrari Request Builder::BuildCreateOrderRequest",
  arity?: 2,                 // disambiguate overloads
  scope?: "project" | "test" | "all"
})
→ [{ file, line, col, callerSymbol }]
```
Implementation-wise this is `textDocument/prepareCallHierarchy` +
`callHierarchy/incomingCalls` under the hood — the AL LS implements
both.

---

## 3. Attribute-aware outline

**Use case.** `al_document_outline` returns symbol names and ranges but
drops AL attributes: `[TryFunction]`, `[EventSubscriber(...)]`,
`[IntegrationEvent]`, `[NonDebuggable]`, `Access = Internal`. During
review I missed a `SkipContentCheck_OnBeforeHasContent` event subscriber
in an outline-only pass — it looked like an ordinary `local procedure`
until I opened the file. Attributes change the *semantic weight* of a
procedure (hidden side-effects, platform coupling) and reviewers care.

**Proposed shape.** Extend the existing outline with an `attributes: string[]`
field per symbol, and optionally a `modifiers: string[]` field for
`local`/`internal`/`[TryFunction]`-ish markers.

---

## 4. `al_run_analyzer_on_file` (force-invoke)

**Use case.** When push diagnostics stay silent, there's no way to know
whether the analyzer ran and found nothing vs. the analyzer failed
silently vs. the background-analysis queue is backed up. During the
debugging session that led to `TODO.md`, this uncertainty ate
significant time. A tool that says "run all registered analyzers on
this URI *now*, synchronously, return results" would have made the
LinterCop load failure visible in one call.

**Proposed shape.**
```
al_run_analyzer_on_file({
  file: "...Ferrari Carrier.al",
  analyzer?: "LinterCop" | "CodeCop" | "AppSourceCop" | "PerTenantExtensionCop" | "all"
})
→ { diagnostics: [...], analyzerErrors: [ { analyzer, exceptionType, message } ] }
```
The second output field is the critical one — exposes `AD0001` shape
errors directly instead of burying them as diagnostics on `app.json`.

---

## 5. `al_cross_file_diff` / structural-compare

**Use case.** The codebase has N parallel carrier integrations
(GOExpress, Ferrari, likely DHL/UPS coming). Review quality depends on
"does Ferrari follow the same pattern as GOExpress?" — but checking that
requires reading both in full. A tool that aligns two codeunits by
procedure signature and shows the structural diff (missing procedures,
divergent signatures, parameters reordered) would let a reviewer spot
drift in one call.

**Proposed shape.**
```
al_structural_diff({
  left: "Ferrari Carrier",
  right: "GOExpress Carrier",
  mode: "interface-surface" | "all"
})
→ { onlyInLeft: [...], onlyInRight: [...], signatureMismatch: [...] }
```
Niche. Probably punted in favor of "reviewer reads both files".

---

## 6. `al_list_symbols_by_pattern`

**Use case.** `al_symbol_search` does fuzzy matching but I often want a
strict enumeration: "list every procedure whose name starts with
`Build`" or "list every `var` parameter of type `Text`". The current
tools don't let me scope by symbol *kind* cleanly across the project.

**Proposed shape.**
```
al_list_symbols({
  namePattern?: "Build*",
  kind?: "procedure" | "var-parameter" | "field" | "integration-event",
  ofType?: "Text" | "Code[*]" | "Record ..."
})
```

---

## 7. Diagnostics **by rule-id** across the workspace

**Use case.** "Show me every `AA0150` in the project right now." Today
`al_get_diagnostics` is per-file — a reviewer wanting a rule-rollup has
to iterate. A workspace-scoped query that filters by code would turn the
bridge into a usable triage tool.

**Proposed shape.**
```
al_diagnostics_by_code({
  codes: ["AA0150", "LC0015"],
  severity?: "error" | "warning"
})
→ [{ file, line, code, message }, ...]
```
Probably the cheapest new tool to build — iterate the push-diagnostics
cache + the pull channel, filter, return.

---

## 8. `al_object_graph` (dependencies)

**Use case.** "What does `Ferrari Carrier` depend on, transitively?" —
codeunit calls, table usage, enum usage, event subscriptions. Useful for
impact analysis before refactoring. AL has enough reflection in the
symbol files to build this cheaply, but no current tool exposes it.

**Proposed shape.**
```
al_object_graph({
  root: "Ferrari Carrier",
  depth?: 2,
  edges?: ["calls", "uses-table", "subscribes-to", "implements"]
})
```

---

## What's already good

For completeness — the tools that *did* carry weight in this review:

- `al_list_objects` — fastest way to enumerate a folder of carrier
  codeunits by name/type before deciding which to read in depth.
- `al_document_outline` — gave me the procedure list for all 5 files in
  one call. The attribute gap (see #3) is the only complaint.
- `al_get_diagnostics` — works well for compile-level issues; the only
  problem is that "clean" diagnostics today are ambiguous (see #1).
- `al_list_code_actions` — surfaces LinterCop's `Fix0001…` quickfixes
  even when the diagnostic side is broken, which is actually useful as
  a side-channel indicator that the analyzer *tried* to run.

---

## Priority rollup for implementers

| # | Tool                         | Cost | Value | Notes                                    |
| - | ---------------------------- | ---- | ----- | ---------------------------------------- |
| 0 | Disk→LSP resync on Edit      | S    | ★★★★★ | Silent false-negatives today; cheapest win |
| 1 | LinterCop load fix           | M    | ★★★★★ | See `TODO.md` — biggest single unlock    |
| 2 | `al_diagnostics_by_code`     | S    | ★★★★  | Iterate existing cache, filter           |
| 3 | Attribute-aware outline      | S    | ★★★★  | One extra field on current outline       |
| 4 | `al_find_callers`            | M    | ★★★   | LS supports `callHierarchy/*` natively   |
| 5 | `al_run_analyzer_on_file`    | M    | ★★★   | Exposes analyzer-error channel           |
| 6 | `al_list_symbols_by_pattern` | M    | ★★    | Nice-to-have; grep often substitutes     |
| 7 | `al_structural_diff`         | L    | ★★    | Niche but high-leverage when applicable  |
| 8 | `al_object_graph`            | L    | ★★    | Easier once symbol index is in place     |
