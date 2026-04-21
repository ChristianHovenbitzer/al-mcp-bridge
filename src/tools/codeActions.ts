import { readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { z } from "zod";
import type {
  TextEdit,
  WorkspaceEdit,
  Diagnostic,
} from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";
import type { BridgeConfig } from "../config.js";

/**
 * The AL LS does not implement the stock LSP code-action flow. Instead:
 *
 *   1. `textDocument/codeAction` returns `ProtocolCodeAction[]` where each
 *      entry's `command.command === "al/runCodeAction"` and
 *      `command.arguments[0]` is a `RunCodeAction { fileName, identifier,
 *      selection, wantsTextChanges }` payload.
 *   2. To execute, the client calls `al/runCodeAction` with that payload.
 *      The LS returns an empty response, then issues a *reverse*
 *      `workspace/applyEdit` request carrying the `WorkspaceEdit`. The
 *      edit's `label` equals the action's identifier — we key on that to
 *      route the inbound edit to the waiting caller.
 *
 * See `es_proto.cs:14082` (CodeActionRequestHandler) and
 * `es_proto.cs:17426` (RunCodeActionCommandHandler).
 */

export const ListCodeActionsInput = z.object({
  file: z.string().describe("Absolute path to the AL file."),
  startLine: z.number().int().nonnegative(),
  startChar: z.number().int().nonnegative(),
  endLine: z.number().int().nonnegative().optional(),
  endChar: z.number().int().nonnegative().optional(),
  only: z
    .array(z.string())
    .optional()
    .describe(
      "Optional CodeActionKind filters (e.g. ['quickfix','refactor']). " +
        "Applied as a substring match against each action's `kind`.",
    ),
});

export type ListCodeActionsInputT = z.infer<typeof ListCodeActionsInput>;

export interface AlCodeAction {
  title: string;
  kind?: string;
  isPreferred?: boolean;
  identifier: string;
  fileName: string;
  selection: { start: { line: number; character: number }; end: { line: number; character: number } };
}

interface RawProtocolCodeAction {
  title: string;
  kind?: string;
  isPreferred?: boolean;
  command?: {
    title?: string;
    command: string;
    arguments?: RawRunCodeAction[];
  };
}

interface RawRunCodeAction {
  FileName?: string;
  fileName?: string;
  Identifier?: string;
  identifier?: string;
  Selection?: RawRange;
  selection?: RawRange;
  WantsTextChanges?: boolean;
  wantsTextChanges?: boolean;
}

interface RawRange {
  start: { line: number; character: number };
  end: { line: number; character: number };
}

export async function listCodeActions(
  client: AlLspClient,
  input: ListCodeActionsInputT,
): Promise<{ actions: AlCodeAction[] }> {
  const uri = await client.openDocument(input.file);
  const endLine = input.endLine ?? input.startLine;
  const endChar = input.endChar ?? input.startChar;

  // Diagnostics context helps analyzers return fixes tied to existing issues.
  const diagnostics = client.diagnostics.current(uri);

  const raw = await client.request<RawProtocolCodeAction[] | null>(
    "textDocument/codeAction",
    {
      textDocument: { uri },
      range: {
        start: { line: input.startLine, character: input.startChar },
        end: { line: endLine, character: endChar },
      },
      context: { diagnostics },
    },
  );

  const list = (raw ?? [])
    .map(normalize)
    .filter((a): a is AlCodeAction => a !== null);

  const filtered = input.only?.length
    ? list.filter((a) =>
        input.only!.some((k) => (a.kind ?? "").toLowerCase().includes(k.toLowerCase())),
      )
    : list;

  return { actions: filtered };
}

function normalize(raw: RawProtocolCodeAction): AlCodeAction | null {
  const arg = raw.command?.arguments?.[0];
  if (!arg) return null;
  const identifier = arg.Identifier ?? arg.identifier;
  const fileName = arg.FileName ?? arg.fileName;
  const selection = arg.Selection ?? arg.selection;
  if (!identifier || !fileName || !selection) return null;
  return {
    title: raw.title,
    kind: raw.kind,
    isPreferred: raw.isPreferred,
    identifier,
    fileName,
    selection,
  };
}

export const RunCodeActionInput = z.object({
  action: z
    .object({
      identifier: z.string(),
      fileName: z.string(),
      selection: z.object({
        start: z.object({ line: z.number().int(), character: z.number().int() }),
        end: z.object({ line: z.number().int(), character: z.number().int() }),
      }),
    })
    .describe("An action object returned by al_list_code_actions."),
  persist: z
    .boolean()
    .default(true)
    .describe("Write changes to disk. False keeps them in-memory only."),
});

export type RunCodeActionInputT = z.infer<typeof RunCodeActionInput>;

export interface RunCodeActionResult {
  identifier: string;
  persisted: boolean;
  changedFiles: Array<{
    uri: string;
    before: string;
    after: string;
    diagnostics: NormalizedDiagnostic[];
  }>;
}

export interface NormalizedDiagnostic {
  severity: "error" | "warning" | "info" | "hint" | "unknown";
  code?: string;
  message: string;
  startLine: number;
  startChar: number;
  endLine: number;
  endChar: number;
}

export function createRunCodeAction(client: AlLspClient, config: BridgeConfig) {
  return async (input: RunCodeActionInputT): Promise<RunCodeActionResult> => {
    // Wire the waiter *before* sending the request — the reverse applyEdit
    // can arrive before the request Promise resolves.
    const editPromise = client.awaitApplyEdit(input.action.identifier, 10000);

    const runPayload = {
      fileName: input.action.fileName,
      identifier: input.action.identifier,
      selection: input.action.selection,
      wantsTextChanges: true,
    };

    // Fire and forget the outbound result — the interesting payload comes
    // through the reverse workspace/applyEdit channel.
    const outbound = client.request<unknown>("al/runCodeAction", [runPayload]);

    const edit = await editPromise;
    await outbound.catch(() => undefined);

    const changed = await applyWorkspaceEdit(client, config, edit, input.persist);

    return {
      identifier: input.action.identifier,
      persisted: input.persist,
      changedFiles: changed,
    };
  };
}

async function applyWorkspaceEdit(
  client: AlLspClient,
  config: BridgeConfig,
  edit: WorkspaceEdit,
  persist: boolean,
): Promise<RunCodeActionResult["changedFiles"]> {
  const perUri = collectEditsByUri(edit);
  const results: RunCodeActionResult["changedFiles"] = [];

  for (const [uri, edits] of perUri) {
    const path = fileURLToPath(uri);
    await client.openDocument(path);
    const before = readFileSync(path, "utf8");
    const after = applyTextEdits(before, edits);

    await client.applyTextChange(uri, after);
    if (persist) writeFileSync(path, after, "utf8");

    const diagnostics = await client.diagnostics.awaitNext(uri, config.diagnosticsSettleMs);
    results.push({
      uri,
      before,
      after,
      diagnostics: diagnostics.map(normalizeDiagnostic),
    });
  }

  return results;
}

function collectEditsByUri(edit: WorkspaceEdit): Map<string, TextEdit[]> {
  const out = new Map<string, TextEdit[]>();
  if (edit.changes) {
    for (const [uri, edits] of Object.entries(edit.changes)) {
      out.set(uri, [...(out.get(uri) ?? []), ...edits]);
    }
  }
  if (edit.documentChanges) {
    for (const change of edit.documentChanges) {
      if ("textDocument" in change && "edits" in change) {
        const uri = change.textDocument.uri;
        const edits = change.edits.filter(
          (e): e is TextEdit => "range" in e && "newText" in e,
        );
        out.set(uri, [...(out.get(uri) ?? []), ...edits]);
      }
    }
  }
  return out;
}

function applyTextEdits(text: string, edits: TextEdit[]): string {
  // Apply end-first so earlier edit offsets stay valid.
  const sorted = [...edits].sort((a, b) => compareRangeEnd(b, a));
  let out = text;
  for (const e of sorted) {
    const start = offsetAt(out, e.range.start.line, e.range.start.character);
    const end = offsetAt(out, e.range.end.line, e.range.end.character);
    out = out.slice(0, start) + e.newText + out.slice(end);
  }
  return out;
}

function compareRangeEnd(a: TextEdit, b: TextEdit): number {
  if (a.range.end.line !== b.range.end.line) {
    return a.range.end.line - b.range.end.line;
  }
  return a.range.end.character - b.range.end.character;
}

function offsetAt(text: string, line: number, character: number): number {
  let offset = 0;
  let currentLine = 0;
  for (let i = 0; i < text.length && currentLine < line; i++) {
    if (text.charCodeAt(i) === 10 /* \n */) currentLine++;
    offset = i + 1;
  }
  return offset + character;
}

function normalizeDiagnostic(d: Diagnostic): NormalizedDiagnostic {
  return {
    severity: severityName(d.severity),
    code: d.code === undefined ? undefined : String(d.code),
    message: d.message,
    startLine: d.range.start.line,
    startChar: d.range.start.character,
    endLine: d.range.end.line,
    endChar: d.range.end.character,
  };
}

function severityName(s: Diagnostic["severity"]): NormalizedDiagnostic["severity"] {
  switch (s) {
    case 1:
      return "error";
    case 2:
      return "warning";
    case 3:
      return "info";
    case 4:
      return "hint";
    default:
      return "unknown";
  }
}
