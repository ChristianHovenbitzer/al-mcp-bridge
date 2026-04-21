import { z } from "zod";
import type { WorkspaceEdit } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";

export const RenameInput = z.object({
  file: z.string(),
  line: z.number().int().nonnegative(),
  character: z.number().int().nonnegative(),
  newName: z.string().min(1),
});

export type RenameInputT = z.infer<typeof RenameInput>;

export interface RenameResult {
  /** Per-URI list of text edits; AI reviews and decides whether to apply. */
  changes: Array<{
    uri: string;
    edits: Array<{
      startLine: number;
      startChar: number;
      endLine: number;
      endChar: number;
      newText: string;
    }>;
  }>;
}

/**
 * Semantic rename. Returns the WorkspaceEdit for review — does NOT auto-
 * apply. A subsequent `al_apply_edit` per affected file is the commit step.
 */
export async function rename(
  client: AlLspClient,
  input: RenameInputT,
): Promise<RenameResult> {
  const uri = await client.openDocument(input.file);
  const edit = await client.request<WorkspaceEdit | null>(
    "textDocument/rename",
    {
      textDocument: { uri },
      position: { line: input.line, character: input.character },
      newName: input.newName,
    },
  );

  if (!edit) return { changes: [] };

  const out: RenameResult["changes"] = [];
  if (edit.changes) {
    for (const [u, edits] of Object.entries(edit.changes)) {
      out.push({
        uri: u,
        edits: edits.map((e) => ({
          startLine: e.range.start.line,
          startChar: e.range.start.character,
          endLine: e.range.end.line,
          endChar: e.range.end.character,
          newText: e.newText,
        })),
      });
    }
  }
  if (edit.documentChanges) {
    for (const dc of edit.documentChanges) {
      if ("edits" in dc) {
        out.push({
          uri: dc.textDocument.uri,
          edits: dc.edits.map((e) => ({
            startLine: e.range.start.line,
            startChar: e.range.start.character,
            endLine: e.range.end.line,
            endChar: e.range.end.character,
            newText: "newText" in e ? e.newText : "",
          })),
        });
      }
    }
  }
  return { changes: out };
}
