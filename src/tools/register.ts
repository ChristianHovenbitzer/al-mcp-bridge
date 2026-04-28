import type { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import type { AlLspClient } from "../lsp/client.js";
import type { BridgeConfig } from "../config.js";
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

export function registerTools(
  mcp: McpServer,
  client: AlLspClient,
  config: BridgeConfig,
  lspReady: Promise<unknown>,
): void {
  const applyEdit = createApplyEdit(client, config);
  const getDiagnostics = createGetDiagnostics(client, config);
  const runCodeAction = createRunCodeAction(client, config);
  const runTests = createRunTests(config.workspaceRoot);
  const compile = createCompile(config);
  const publish = createPublish(config.workspaceRoot);

  const json = (v: unknown) => ({
    content: [{ type: "text" as const, text: JSON.stringify(v, null, 2) }],
  });

  mcp.registerTool(
    "al_document_outline",
    {
      description:
        "Return the AL object/member structure of a file (objects, triggers, procedures, fields).",
      inputSchema: OutlineInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await documentOutline(client, input));
    },
  );

  mcp.registerTool(
    "al_get_symbol_at",
    {
      description:
        "Resolve the symbol at (file, line, character). Returns hover (type + XML doc) plus definition location(s). Combines hover + definition in one call.",
      inputSchema: SymbolAtInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await symbolAt(client, input));
    },
  );

  mcp.registerTool(
    "al_find_references",
    {
      description:
        "Find all references to the symbol at a position. Semantic (compiler-driven), not textual.",
      inputSchema: FindReferencesInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await findReferences(client, input));
    },
  );

  mcp.registerTool(
    "al_rename",
    {
      description:
        "Semantic rename of the symbol at a position. Returns a per-file edit set for review. Does NOT apply to disk — use al_apply_edit afterwards.",
      inputSchema: RenameInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await rename(client, input));
    },
  );

  mcp.registerTool(
    "al_format",
    {
      description:
        "Run the AL formatter on the full document or a range. Returns text edits to apply.",
      inputSchema: FormatInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await formatDocument(client, input));
    },
  );

  mcp.registerTool(
    "al_apply_edit",
    {
      description:
        "Replace the entire text of an AL file and return fresh compiler diagnostics. persist=false keeps the change in memory only (preview mode).",
      inputSchema: ApplyEditInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await applyEdit(input));
    },
  );

  mcp.registerTool(
    "al_get_diagnostics",
    {
      description:
        "Return current AL compiler diagnostics for a file. waitForFresh=true waits briefly for the next publish (use after an edit that didn't go through al_apply_edit).",
      inputSchema: GetDiagnosticsInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await getDiagnostics(input));
    },
  );

  mcp.registerTool(
    "al_symbol_search",
    {
      description:
        "Search AL symbols (tables, codeunits, pages, fields, methods) across project and dependencies. Pass query='*' with filters to enumerate. " +
        "IMPORTANT: member-level symbols (procedures, fields, triggers, etc.) are only included in results when at least one of `memberKinds` or `objectName` is specified. " +
        "To find a procedure by name: pass query='ProcedureName' and filters={memberKinds:['Method']}. " +
        "To list all members of an object: pass query='*' and filters={objectName:'MyCodeunit', memberKinds:['Method']}. " +
        "Without memberKinds/objectName, only top-level objects (codeunits, tables, pages, …) are returned.",
      inputSchema: SymbolSearchInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await symbolSearch(client, input));
    },
  );

  mcp.registerTool(
    "al_list_objects",
    {
      description:
        "List AL application objects (tables, pages, codeunits, ...) without reading files.",
      inputSchema: ListObjectsInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await listObjects(client, input));
    },
  );

  mcp.registerTool(
    "al_list_code_actions",
    {
      description:
        "List available code actions (quickfixes, refactorings) at a position or range. " +
        "Returns AL-specific action objects whose `identifier` can be passed to al_run_code_action. " +
        "When analyzer DLLs (e.g. ALCops) are loaded via AL_CODE_ANALYZERS, their fixes appear here.",
      inputSchema: ListCodeActionsInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await listCodeActions(client, input));
    },
  );

  mcp.registerTool(
    "al_run_code_action",
    {
      description:
        "Execute a code action returned by al_list_code_actions. Applies the resulting " +
        "WorkspaceEdit and returns fresh diagnostics per file. persist=false keeps changes " +
        "in the LS buffer only (preview mode).",
      inputSchema: RunCodeActionInput.shape,
    },
    async (input) => {
      await lspReady;
      return json(await runCodeAction(input));
    },
  );

  mcp.registerTool(
    "al_run_tests",
    {
      description:
        "Run an AL test codeunit against a Business Central on-premise dev service tier. " +
        "Reads connection info from the project's .vscode/launch.json. " +
        "Credentials come from BC_USER/BC_PASSWORD env vars, or ~/.config/al-mcp-bridge/credentials.json " +
        "(mode 0600 required). Returns per-method pass/fail/skipped results. " +
        "Linux-compatible — does not use the Windows Credential Manager path Microsoft's tool requires.",
      inputSchema: RunTestsInput.shape,
    },
    async (input) => {
      // No lspReady wait — this tool talks to BC directly, not the LSP.
      return json(await runTests(input));
    },
  );

  mcp.registerTool(
    "al_compile",
    {
      description:
        "Compile an AL project to a .app package by invoking the `alc` binary that ships with the AL extension. " +
        "Returns exit code, parsed SARIF diagnostics (with file/line ranges), and the produced .app path. " +
        "Defaults for analyzers, package cache, and ruleset come from the bridge's resolved config (same as the LSP), " +
        "but can be overridden per call. Runs on Linux — does not depend on the MS `al-mcp` server.",
      inputSchema: CompileInput.shape,
    },
    async (input) => {
      return json(await compile(input));
    },
  );

  mcp.registerTool(
    "al_publish",
    {
      description:
        "Publish a compiled .app to a Business Central on-premise dev service tier. " +
        "Reads server/instance/tenant from .vscode/launch.json and reuses the same BC_USER/BC_PASSWORD " +
        "credential flow as al_run_tests. Uploads via multipart/form-data POST to /<instance>/dev/apps. " +
        "Scope: on-prem + UserPassword auth only. Run al_compile first to produce the .app.",
      inputSchema: PublishInput.shape,
    },
    async (input) => {
      return json(await publish(input));
    },
  );

  // TODO (M5 remainder): al_list_event_publishers, al_check_symbols
}
