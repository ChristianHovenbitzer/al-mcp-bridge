import { z } from "zod";
import type { TextEdit } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";

export const FormatInput = z.object({
  file: z.string(),
  range: z
    .object({
      startLine: z.number().int().nonnegative(),
      startChar: z.number().int().nonnegative(),
      endLine: z.number().int().nonnegative(),
      endChar: z.number().int().nonnegative(),
    })
    .optional(),
  tabSize: z.number().int().positive().default(4),
  insertSpaces: z.boolean().default(true),
});

export type FormatInputT = z.infer<typeof FormatInput>;

export interface FormatResult {
  edits: Array<{
    startLine: number;
    startChar: number;
    endLine: number;
    endChar: number;
    newText: string;
  }>;
}

export async function formatDocument(
  client: AlLspClient,
  input: FormatInputT,
): Promise<FormatResult> {
  const uri = await client.openDocument(input.file);
  const options = {
    tabSize: input.tabSize,
    insertSpaces: input.insertSpaces,
  };

  let edits: TextEdit[] | null;
  if (input.range) {
    edits = await client.request<TextEdit[] | null>(
      "textDocument/rangeFormatting",
      {
        textDocument: { uri },
        range: {
          start: { line: input.range.startLine, character: input.range.startChar },
          end: { line: input.range.endLine, character: input.range.endChar },
        },
        options,
      },
    );
  } else {
    edits = await client.request<TextEdit[] | null>(
      "textDocument/formatting",
      { textDocument: { uri }, options },
    );
  }

  return {
    edits: (edits ?? []).map((e) => ({
      startLine: e.range.start.line,
      startChar: e.range.start.character,
      endLine: e.range.end.line,
      endChar: e.range.end.character,
      newText: e.newText,
    })),
  };
}
