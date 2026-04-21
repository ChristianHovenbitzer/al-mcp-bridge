import { z } from "zod";
import type { AlLspClient } from "../lsp/client.js";

/**
 * AL-specific search endpoints. These forward to the LS's own MCP-style
 * tools exposed over LSP (see SymbolSearchService in the decompiled
 * workspaces bundle). We intentionally pass the `params` object through
 * with minimal reshaping — the LS side is the source of truth for the
 * filter schema.
 */

export const SymbolSearchInput = z.object({
  query: z.string().describe("Search query, '*' for all."),
  filters: z
    .object({
      kinds: z.array(z.string()).optional(),
      memberKinds: z.array(z.string()).optional(),
      objectName: z.string().optional(),
      namespace: z.string().optional(),
      access: z.array(z.string()).optional(),
      obsoleteState: z.array(z.string()).optional(),
      match: z.enum(["name", "doc", "all"]).optional(),
      scope: z.enum(["project", "dependencies", "all"]).optional(),
      limit: z.number().int().positive().max(200).optional(),
    })
    .optional(),
});

export type SymbolSearchInputT = z.infer<typeof SymbolSearchInput>;

export async function symbolSearch(
  client: AlLspClient,
  input: SymbolSearchInputT,
): Promise<unknown> {
  return client.request<unknown>("al/symbolSearch", {
    query: input.query,
    ...(input.filters ?? {}),
  });
}

export const ListObjectsInput = z.object({
  types: z.array(z.string()).optional().describe("e.g. ['Table','Codeunit']"),
  name: z.string().optional(),
});

export type ListObjectsInputT = z.infer<typeof ListObjectsInput>;

export async function listObjects(
  client: AlLspClient,
  input: ListObjectsInputT,
): Promise<unknown> {
  return client.request<unknown>("al/getApplicationObjects", input);
}
