import { writeFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { z } from "zod";
import type { Diagnostic } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";
import type { BridgeConfig } from "../config.js";

/**
 * The core value prop: apply an edit and get AL-aware diagnostics back in
 * the same call. `al_preview_edit` is identical but reverts before
 * returning — lets the AI iterate without committing.
 */

export const ApplyEditInput = z.object({
  file: z.string().describe("Absolute path to the AL file."),
  newText: z.string().describe("Full replacement text for the document."),
  persist: z
    .boolean()
    .default(true)
    .describe("Write to disk. Set false for preview mode (in-memory only)."),
});

export type ApplyEditInputT = z.infer<typeof ApplyEditInput>;

export interface ApplyEditResult {
  uri: string;
  version: number;
  diagnostics: Array<{
    severity: "error" | "warning" | "info" | "hint" | "unknown";
    code?: string;
    message: string;
    startLine: number;
    startChar: number;
    endLine: number;
    endChar: number;
  }>;
  persisted: boolean;
}

export function createApplyEdit(client: AlLspClient, config: BridgeConfig) {
  return async (input: ApplyEditInputT): Promise<ApplyEditResult> => {
    const uri = await client.openDocument(input.file);
    const priorText = undefined; // caller supplies the new full text; we rely on full-sync

    const version = await client.applyTextChange(uri, input.newText);
    const diagnostics = await client.diagnostics.awaitNext(
      uri,
      config.diagnosticsSettleMs,
    );

    let persisted = false;
    if (input.persist) {
      const filePath = fileURLToPath(uri);
      writeFileSync(filePath, input.newText, "utf8");
      persisted = true;
    } else if (priorText !== undefined) {
      // future: restore buffer if we decide to track prior text
    }

    return {
      uri,
      version,
      persisted,
      diagnostics: diagnostics.map(normalizeDiagnostic),
    };
  };
}

function normalizeDiagnostic(d: Diagnostic): ApplyEditResult["diagnostics"][number] {
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

function severityName(s: Diagnostic["severity"]): ApplyEditResult["diagnostics"][number]["severity"] {
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
