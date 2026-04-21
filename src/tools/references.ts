import { z } from "zod";
import type { DocumentHighlight, Hover, Location } from "vscode-languageserver-protocol";
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
  error?: string;
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
  const position = { line: input.line, character: input.character };

  let locations = await client.request<Location[]>("textDocument/references", {
    textDocument: { uri },
    position,
    context: { includeDeclaration: input.includeDeclaration },
  });

  if (locations && locations.length > 0) {
    return normalize(locations);
  }

  // Before attempting recovery, verify that a symbol actually exists at this
  // position. Hover returns null on comments, whitespace, and keywords.
  const hover = await client
    .request<Hover | null>("textDocument/hover", { textDocument: { uri }, position })
    .catch(() => null);

  if (!hasHoverContent(hover)) {
    return {
      references: [],
      error:
        `No symbol found at line ${input.line}, character ${input.character}. ` +
        `The position may point to a comment, whitespace, or keyword. ` +
        `Use al_document_outline to find the exact line/character of a symbol, ` +
        `or al_get_symbol_at to confirm a position resolves to a symbol.`,
    };
  }

  // AL LS quirk: textDocument/references returns empty when the cursor is at a
  // declaration site. Work around by using documentHighlight (file-scoped) to
  // find a call site within the same file, then retry references from there.
  const highlights = await client
    .request<DocumentHighlight[]>("textDocument/documentHighlight", {
      textDocument: { uri },
      position,
    })
    .catch(() => null);

  if (highlights && highlights.length > 0) {
    const callSite = highlights.find(
      (h) =>
        h.range.start.line !== input.line ||
        h.range.start.character !== input.character,
    );
    if (callSite) {
      const retried = await client
        .request<Location[]>("textDocument/references", {
          textDocument: { uri },
          position: callSite.range.start,
          context: { includeDeclaration: input.includeDeclaration },
        })
        .catch(() => null);
      if (retried) locations = retried;
    }
  }

  return normalize(locations ?? []);
}

function hasHoverContent(hover: Hover | null | undefined): boolean {
  if (!hover) return false;
  const c = hover.contents;
  if (typeof c === "string") return c.trim().length > 0;
  if (Array.isArray(c)) return c.some((x) => (typeof x === "string" ? x : x.value).trim().length > 0);
  return (c as { value: string }).value.trim().length > 0;
}

function normalize(locations: Location[]): FindReferencesResult {
  return {
    references: locations.map((l) => ({
      uri: l.uri,
      startLine: l.range.start.line,
      startChar: l.range.start.character,
      endLine: l.range.end.line,
      endChar: l.range.end.character,
    })),
  };
}
