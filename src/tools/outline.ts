import { z } from "zod";
import type { DocumentSymbol, SymbolInformation } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";

export const OutlineInput = z.object({
  file: z.string().describe("Absolute path to the AL file."),
});

export type OutlineInputT = z.infer<typeof OutlineInput>;

export interface OutlineNode {
  name: string;
  /** Human-readable LSP SymbolKind name (e.g. "Method", "Field", "Object").
   *  Emitted instead of the raw numeric kind so the model doesn't need the
   *  SymbolKind enum table to interpret the outline. */
  kind: string;
  detail?: string;
  startLine: number;
  startChar: number;
  endLine: number;
  endChar: number;
  children?: OutlineNode[];
}

/** LSP SymbolKind (1-26) → name. Unknown values fall back to the number. */
const SYMBOL_KIND_NAMES: Record<number, string> = {
  1: "File", 2: "Module", 3: "Namespace", 4: "Package", 5: "Class",
  6: "Method", 7: "Property", 8: "Field", 9: "Constructor", 10: "Enum",
  11: "Interface", 12: "Function", 13: "Variable", 14: "Constant",
  15: "String", 16: "Number", 17: "Boolean", 18: "Array", 19: "Object",
  20: "Key", 21: "Null", 22: "EnumMember", 23: "Struct", 24: "Event",
  25: "Operator", 26: "TypeParameter",
};

function kindName(kind: number): string {
  return SYMBOL_KIND_NAMES[kind] ?? String(kind);
}

export async function documentOutline(
  client: AlLspClient,
  input: OutlineInputT,
): Promise<{ outline: OutlineNode[] }> {
  const uri = await client.openDocument(input.file);

  // The AL LS parses asynchronously after didOpen. documentSymbol can return
  // an empty list before the first parse completes. Wait briefly for the
  // initial publishDiagnostics on this URI (which only fires post-parse),
  // then retry. Falls through after timeout so empty files still resolve.
  if (!client.diagnostics.hasPublishedFor(uri)) {
    await client.diagnostics.awaitNext(uri, 3000).catch(() => undefined);
  }

  const raw = await client.request<(DocumentSymbol | SymbolInformation)[] | null>(
    "textDocument/documentSymbol",
    { textDocument: { uri } },
  );
  return { outline: (raw ?? []).map(normalize) };
}

function normalize(s: DocumentSymbol | SymbolInformation): OutlineNode {
  if ("range" in s && "selectionRange" in s) {
    return {
      name: s.name,
      kind: kindName(s.kind),
      detail: s.detail,
      startLine: s.range.start.line,
      startChar: s.range.start.character,
      endLine: s.range.end.line,
      endChar: s.range.end.character,
      children: s.children?.map(normalize),
    };
  }
  return {
    name: s.name,
    kind: kindName(s.kind),
    startLine: s.location.range.start.line,
    startChar: s.location.range.start.character,
    endLine: s.location.range.end.line,
    endChar: s.location.range.end.character,
  };
}
