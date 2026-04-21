import { z } from "zod";
import { pathToFileURL } from "node:url";
import type { Diagnostic } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";
import type { BridgeConfig } from "../config.js";

export const GetDiagnosticsInput = z.object({
  file: z.string().describe("Absolute path to the AL file."),
  waitForFresh: z
    .boolean()
    .default(false)
    .describe("If true, waits up to diagnosticsSettleMs for a new publish; otherwise returns cached."),
  debugCache: z
    .boolean()
    .default(false)
    .describe("If true, also returns the full push-diagnostics cache (all URIs + codes) for debugging why the requested file appears empty."),
});

export type GetDiagnosticsInputT = z.infer<typeof GetDiagnosticsInput>;

export interface GetDiagnosticsResult {
  uri: string;
  diagnostics: Array<{
    severity: "error" | "warning" | "info" | "hint" | "unknown";
    code?: string;
    message: string;
    startLine: number;
    startChar: number;
    endLine: number;
    endChar: number;
  }>;
  debugCache?: Array<{ uri: string; count: number; codes: string[] }>;
}

export function createGetDiagnostics(client: AlLspClient, config: BridgeConfig) {
  return async (input: GetDiagnosticsInputT): Promise<GetDiagnosticsResult> => {
    // didOpen ensures the LS sees the file; essential for first-call diagnostics.
    const uri = await client.openDocument(input.file);
    const alternate = pathToFileURL(input.file).toString();

    const pushed = input.waitForFresh
      ? await client.diagnostics.awaitNext(uri, config.diagnosticsSettleMs)
      : client.diagnostics.current(uri);

    const pushList = pushed.length === 0 ? client.diagnostics.current(alternate) : pushed;

    // MS AL LS routes third-party analyzer findings (LinterCop, ALCops)
    // exclusively through LSP 3.17 pull diagnostics — they never arrive
    // via `textDocument/publishDiagnostics`. Merge both sources so tools
    // see the full picture regardless of channel.
    const pulled = (await client.pullDiagnostics(uri)) ?? [];
    const merged = mergeDiagnostics(pushList, pulled);

    return {
      uri,
      diagnostics: merged.map((d) => ({
        severity: severityName(d.severity),
        code: d.code === undefined ? undefined : String(d.code),
        message: d.message,
        startLine: d.range.start.line,
        startChar: d.range.start.character,
        endLine: d.range.end.line,
        endChar: d.range.end.character,
      })),
      ...(input.debugCache ? { debugCache: client.diagnostics.snapshotSummary() } : {}),
    };
  };
}

function mergeDiagnostics(push: Diagnostic[], pull: Diagnostic[]): Diagnostic[] {
  const seen = new Set<string>();
  const out: Diagnostic[] = [];
  for (const d of [...push, ...pull]) {
    const key = `${d.code ?? ""}|${d.range.start.line}:${d.range.start.character}-${d.range.end.line}:${d.range.end.character}|${d.message}`;
    if (seen.has(key)) continue;
    seen.add(key);
    out.push(d);
  }
  return out;
}

function severityName(s: number | undefined): GetDiagnosticsResult["diagnostics"][number]["severity"] {
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
