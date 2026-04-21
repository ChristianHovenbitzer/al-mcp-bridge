import { z } from "zod";
import type { DocumentSymbol, SymbolInformation } from "vscode-languageserver-protocol";
import type { AlLspClient } from "../lsp/client.js";

export const OutlineInput = z.object({
  file: z.string().describe("Absolute path to the AL file."),
});

export type OutlineInputT = z.infer<typeof OutlineInput>;

export interface OutlineNode {
  name: string;
  kind: number;
  detail?: string;
  startLine: number;
  startChar: number;
  endLine: number;
  endChar: number;
  children?: OutlineNode[];
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
      kind: s.kind,
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
    kind: s.kind,
    startLine: s.location.range.start.line,
    startChar: s.location.range.start.character,
    endLine: s.location.range.end.line,
    endChar: s.location.range.end.character,
  };
}
