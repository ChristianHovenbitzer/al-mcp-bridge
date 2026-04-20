import { z } from "zod";
import type { Location } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";

export const FindReferencesInput = z.object({
  file: z.string().describe("Absolute path to the AL file."),
  line: z.number().int().nonnegative().describe("Zero-based line number of the symbol."),
  character: z.number().int().nonnegative().describe("Zero-based character offset in the line."),
  includeDeclaration: z.boolean().default(true),
});

export type FindReferencesInputT = z.infer<typeof FindReferencesInput>;

export interface FindReferencesResult {
  references: Array<{
    uri: string;
    startLine: number;
    startChar: number;
    endLine: number;
    endChar: number;
  }>;
}

/**
 * Fully-implemented reference finder — used as the architectural template
 * for every other tool. The pattern is:
 *   1. openDocument (idempotent didOpen)
 *   2. forward to LSP method
 *   3. normalize the response into an AI-friendly shape
 */
export async function findReferences(
  client: AlLspClient,
  input: FindReferencesInputT,
): Promise<FindReferencesResult> {
  const uri = await client.openDocument(input.file);
  const locations = await client.request<Location[], unknown>("textDocument/references", {
    textDocument: { uri },
    position: { line: input.line, character: input.character },
    context: { includeDeclaration: input.includeDeclaration },
  });

  return {
    references: (locations ?? []).map((l) => ({
      uri: l.uri,
      startLine: l.range.start.line,
      startChar: l.range.start.character,
      endLine: l.range.end.line,
      endChar: l.range.end.character,
    })),
  };
}
