import type { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import type { AlLspClient } from "../lsp/client.js";
import type { BridgeConfig } from "../config.js";
import type { ZodRawShape } from "zod";
import { withTimeout } from "../timeouts.js";
import { FindReferencesInput, findReferences } from "./references.js";
import { ApplyEditInput, createApplyEdit } from "./edit.js";
import { OutlineInput, documentOutline } from "./outline.js";
import { SymbolAtInput, symbolAt } from "./symbol.js";
import { FormatInput, formatDocument } from "./format.js";
import { GetDiagnosticsInput, createGetDiagnostics } from "./diagnostics.js";
import { RenameInput, rename } from "./rename.js";
import {
  ListObjectsInput,
  SymbolSearchInput,
  listObjects,
  symbolSearch,
} from "./search.js";
import {
  ListCodeActionsInput,
  RunCodeActionInput,
  createRunCodeAction,
  listCodeActions,
} from "./codeActions.js";
import { RunTestsInput, createRunTests } from "./runTests.js";
import { CompileInput, createCompile } from "./compile.js";
import { PublishInput, createPublish } from "./publish.js";
import {
  ListWorkspacesInput,
  LoadWorkspaceInput,
  assertFileInWorkspace,
  createListWorkspaces,
  createLoadWorkspace,
} from "./workspace.js";
import { RestartLspInput, createRestartLsp } from "./restart.js";
import { LspStatusInput, createLspStatus } from "./status.js";

export function registerTools(
  mcp: McpServer,
  client: AlLspClient,
  config: BridgeConfig,
): void {
  const applyEdit = createApplyEdit(client, config);
  const getDiagnostics = createGetDiagnostics(client, config);
  const runCodeAction = createRunCodeAction(client, config);
  const runTests = createRunTests(config.workspaceRoot);
  const compile = createCompile(config);
  const publish = createPublish(config.workspaceRoot);
  const loadWorkspace = createLoadWorkspace(client, config);
  const listWorkspaces = createListWorkspaces(client, config);
  const restartLsp = createRestartLsp(client, config);
  const lspStatus = createLspStatus(client, config);
  const t = config.timeouts;

  // Compact serialization (no pretty-print indentation). The consumer is an
  // LLM that parses compact and pretty JSON identically, so the per-line
  // newlines + indentation that `JSON.stringify(v, null, 2)` adds to every
  // field of every array element are pure token overhead. Dropping them
  // shrinks diagnostic/reference/outline payloads ~20-40% with no loss of
  // information. Tests consume these via `JSON.parse`, so compact is safe.
  const json = (v: unknown) => ({
    content: [{ type: "text" as const, text: JSON.stringify(v) }],
  });

  /**
   * Register a tool with a hard outer deadline.
   *
   * Belt to the per-request braces inside the LSP client: any handler that
   * ends up waiting on something without its own bound (a settle window, a
   * child process, a chain of several LSP round trips) still fails loudly
   * instead of leaving an MCP call outstanding forever. A hung tool call is
   * the worst failure mode available here - the client shows no output, no
   * error, and cannot be retried.
   */
  // The SDK's `registerTool` overloads infer the handler's argument type from
  // the zod shape, which doesn't survive being passed through a generic
  // wrapper. Bind a shape-agnostic view of it once; per-tool input types stay
  // enforced at runtime by zod and at design time by each tool's `*InputT`.
  const register = mcp.registerTool.bind(mcp) as unknown as (
    name: string,
    config: { description: string; inputSchema: ZodRawShape },
    cb: (input: any) => Promise<{ content: Array<{ type: "text"; text: string }> }>,
  ) => void;

  const tool = (
    name: string,
    description: string,
    inputSchema: ZodRawShape,
    handler: (input: any) => Promise<unknown>,
    timeoutMs = t.toolMs,
  ): void => {
    register(name, { description, inputSchema }, async (input) =>
      json(await withTimeout(handler(input), timeoutMs, `tool ${name}`)),
    );
  };

  /** Reject inputs whose `file` path isn't under any loaded workspace.
   *  Surfaces clearly instead of silently returning empty diagnostics or
   *  outline data - the bridge's most confusing failure mode. */
  const guardFile = (input: { file: string }) =>
    assertFileInWorkspace(input.file, client.getWorkspaceFolders());

  /** Wait for the current LS generation, bounded by `lspReadyMs`. */
  const ready = () => client.ready();

  tool(
    "al_document_outline",
    "Return the AL object/member structure of a file (objects, triggers, procedures, fields).",
    OutlineInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return documentOutline(client, input);
    },
  );

  tool(
    "al_get_symbol_at",
    "Resolve the symbol at (file, line, character). Returns hover (type + XML doc) plus definition location(s). Combines hover + definition in one call.",
    SymbolAtInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return symbolAt(client, input);
    },
  );

  tool(
    "al_find_references",
    "Find all references to the symbol at a position. Semantic (compiler-driven), not textual.",
    FindReferencesInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return findReferences(client, input);
    },
  );

  tool(
    "al_rename",
    "Semantic rename of the symbol at a position. Returns a per-file edit set for review. Does NOT apply to disk - use al_apply_edit afterwards.",
    RenameInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return rename(client, input);
    },
  );

  tool(
    "al_format",
    "Run the AL formatter on the full document or a range. Returns text edits to apply.",
    FormatInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return formatDocument(client, input);
    },
  );

  tool(
    "al_apply_edit",
    "Replace the entire text of an AL file and return fresh compiler diagnostics. persist=false keeps the change in memory only (preview mode).",
    ApplyEditInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return applyEdit(input);
    },
  );

  tool(
    "al_get_diagnostics",
    "Return current AL compiler diagnostics for a file. waitForFresh=true waits briefly for the next publish (use after an edit that didn't go through al_apply_edit).",
    GetDiagnosticsInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return getDiagnostics(input);
    },
  );

  tool(
    "al_symbol_search",
    "Search AL symbols (tables, codeunits, pages, fields, methods) across project and dependencies. Pass query='*' with filters to enumerate. " +
      "IMPORTANT: member-level symbols (procedures, fields, triggers, etc.) are only included in results when at least one of `memberKinds` or `objectName` is specified. " +
      "To find a procedure by name: pass query='ProcedureName' and filters={memberKinds:['Method']}. " +
      "To list all members of an object: pass query='*' and filters={objectName:'MyCodeunit', memberKinds:['Method']}. " +
      "Without memberKinds/objectName, only top-level objects (codeunits, tables, pages, …) are returned.",
    SymbolSearchInput.shape,
    async (input) => {
      await ready();
      return symbolSearch(client, input);
    },
  );

  tool(
    "al_list_objects",
    "List AL application objects (tables, pages, codeunits, ...) without reading files.",
    ListObjectsInput.shape,
    async (input) => {
      await ready();
      return listObjects(client, input);
    },
  );

  tool(
    "al_list_code_actions",
    "List available code actions (quickfixes, refactorings) at a position or range. " +
      "Returns AL-specific action objects whose `identifier` can be passed to al_run_code_action. " +
      "When analyzer DLLs (e.g. ALCops) are loaded via AL_CODE_ANALYZERS, their fixes appear here.",
    ListCodeActionsInput.shape,
    async (input) => {
      await ready();
      guardFile(input);
      return listCodeActions(client, input);
    },
  );

  tool(
    "al_run_code_action",
    "Execute a code action returned by al_list_code_actions. Applies the resulting " +
      "WorkspaceEdit and returns fresh diagnostics per file. persist=false keeps changes " +
      "in the LS buffer only (preview mode).",
    RunCodeActionInput.shape,
    async (input) => {
      await ready();
      assertFileInWorkspace(input.action.fileName, client.getWorkspaceFolders());
      return runCodeAction(input);
    },
  );

  tool(
    "al_run_tests",
    "Run an AL test codeunit against a Business Central on-premise dev service tier. " +
      "Reads connection info from the project's .vscode/launch.json. " +
      "Credentials come from BC_USER/BC_PASSWORD env vars, or ~/.config/al-mcp-bridge/credentials.json " +
      "(mode 0600 required). Returns per-method pass/fail/skipped results. " +
      "Linux-compatible - does not use the Windows Credential Manager path Microsoft's tool requires.",
    RunTestsInput.shape,
    // No `ready()` wait - this tool talks to BC directly, not the LSP.
    async (input) => runTests(input),
    t.runTestsMs,
  );

  tool(
    "al_compile",
    "Compile an AL project to a .app package by invoking the `alc` binary that ships with the AL extension. " +
      "Returns exit code, severity counts, the produced .app path, and a per-file overview (`files`: " +
      "path + severity counts + distinct rule IDs). For line-level message/range detail on a file, call " +
      "al_get_diagnostics with that file path, or pass verbose=true to inline the full per-diagnostic array. " +
      "Defaults for analyzers, package cache, and ruleset come from the bridge's resolved config (same as the LSP), " +
      "but can be overridden per call. Runs on Linux - does not depend on the MS `al-mcp` server.",
    CompileInput.shape,
    async (input) => compile(input),
    // The alc child enforces its own kill deadline; keep the outer guard above
    // it so the inner error (with partial output) is what surfaces.
    t.compileMs + 30_000,
  );

  tool(
    "al_publish",
    "Publish a compiled .app to a Business Central on-premise dev service tier. " +
      "Reads server/instance/tenant from .vscode/launch.json and reuses the same BC_USER/BC_PASSWORD " +
      "credential flow as al_run_tests. Uploads via multipart/form-data POST to /<instance>/dev/apps. " +
      "Scope: on-prem + UserPassword auth only. Run al_compile first to produce the .app.",
    PublishInput.shape,
    async (input) => publish(input),
    t.publishMs + 30_000,
  );

  tool(
    "al_load_workspace",
    "Register an additional AL project folder with the running LSP at runtime. " +
      "Use this when al_get_diagnostics / al_document_outline / etc. fail with " +
      '"file is not inside any loaded AL workspace" - typically because the bridge ' +
      "was launched from a directory that doesn't share an `app.json` ancestor with the file. " +
      "The path must be the AL project root (the folder containing `app.json`). " +
      "Re-reads that folder's `.vscode/settings.json` so its analyzers and ruleset apply " +
      "to its own files. If this ever times out, the LS is wedged - use al_restart_lsp " +
      "with that path instead, which gives the project a clean LS of its own.",
    LoadWorkspaceInput.shape,
    async (input) => {
      await ready();
      return loadWorkspace(input);
    },
  );

  tool(
    "al_list_workspaces",
    "Return the AL project folders currently registered with the LSP, the primary " +
      "workspace, and a flag indicating whether the initial set was inferred via a " +
      "downward filesystem scan (a common cause of the bridge attaching to the wrong project).",
    ListWorkspacesInput.shape,
    async () => {
      await ready();
      return listWorkspaces();
    },
  );

  tool(
    "al_restart_lsp",
    "Restart the AL language server process behind this bridge, optionally re-targeting it " +
      "at another AL project. Use this when (a) LSP tools time out or report the LS as unusable, " +
      "(b) an analyzer DLL was rebuilt (the CLR pins assemblies for the LS process lifetime, so " +
      "only a respawn picks up new rules), or (c) the session moved to a different repo and the " +
      "LSP is still initialized against the previous project - pass `workspace` with the new " +
      "project root (the folder containing app.json). Does NOT restart the MCP server process " +
      "itself, so no Claude Code reconnect is needed. Returns the old and new PID plus the " +
      "workspaces the fresh LS registered.",
    RestartLspInput.shape,
    // Intentionally no `ready()` wait: this is the recovery path for a startup
    // that never completes.
    async (input) => restartLsp(input),
    Math.max(t.toolMs, t.lspReadyMs + 60_000),
  );

  tool(
    "al_lsp_status",
    "Diagnose why the LSP-driven tools (al_get_diagnostics, al_list_code_actions, …) " +
      "return less than expected. Returns the running LS process info (pid, start time, " +
      "uptime, generation, liveness), every registered workspace with its resolved analyzer DLL " +
      "paths plus on-disk mtime/size, open documents, the push-diagnostics cache snapshot, " +
      "pull-diagnostics traffic counters, the effective timeouts, and a `warnings` array calling " +
      "out common failure modes (stale analyzer DLL, missing ruleset, code-analysis disabled, " +
      "empty cache with open docs, dead LS). Use this first whenever VSCode shows a diagnostic " +
      "the bridge doesn't, or when a tool call timed out.",
    LspStatusInput.shape,
    // No `ready()` wait: status must answer precisely when startup is stuck.
    async (input) => lspStatus(input),
    30_000,
  );

  // TODO (M5 remainder): al_list_event_publishers, al_check_symbols
}
