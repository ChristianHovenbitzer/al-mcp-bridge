import { z } from "zod";
import type { Hover, Location, LocationLink, MarkupContent } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";

export const SymbolAtInput = z.object({
  file: z.string(),
  line: z.number().int().nonnegative(),
  character: z.number().int().nonnegative(),
});

export type SymbolAtInputT = z.infer<typeof SymbolAtInput>;

export interface SymbolAtResult {
  hover: string | null;
  definition: Array<{
    uri: string;
    startLine: number;
    startChar: number;
    endLine: number;
    endChar: number;
  }>;
}

/**
 * Combines hover (what this is) with definition (where it's defined) in a
 * single MCP call. Saves the AI from round-tripping twice for the most
 * common "resolve symbol" question.
 */
export async function symbolAt(
  client: AlLspClient,
  input: SymbolAtInputT,
): Promise<SymbolAtResult> {
  const uri = await client.openDocument(input.file);
  const position = { line: input.line, character: input.character };

  const [hoverRaw, defRaw] = await Promise.all([
    client.request<Hover | null>("textDocument/hover", {
      textDocument: { uri },
      position,
    }),
    client.request<Location | Location[] | LocationLink[] | null>(
      "textDocument/definition",
      { textDocument: { uri }, position },
    ),
  ]);

  return {
    hover: renderHover(hoverRaw),
    definition: normalizeDefinition(defRaw),
  };
}

function renderHover(h: Hover | null): string | null {
  if (!h) return null;
  const c = h.contents;
  if (typeof c === "string") return c;
  if (Array.isArray(c)) {
    return c
      .map((x) => (typeof x === "string" ? x : x.value))
      .join("\n\n");
  }
  return (c as MarkupContent).value;
}

function normalizeDefinition(
  d: Location | Location[] | LocationLink[] | null,
): SymbolAtResult["definition"] {
  if (!d) return [];
  const arr = Array.isArray(d) ? d : [d];
  return arr.map((item) => {
    if ("targetUri" in item) {
      return {
        uri: item.targetUri,
        startLine: item.targetRange.start.line,
        startChar: item.targetRange.start.character,
        endLine: item.targetRange.end.line,
        endChar: item.targetRange.end.character,
      };
    }
    return {
      uri: item.uri,
      startLine: item.range.start.line,
      startChar: item.range.start.character,
      endLine: item.range.end.line,
      endChar: item.range.end.character,
    };
  });
}
