import { z } from "zod";
import type { AlLspClient } from "../lsp/client.js";

/**
 * AL-specific search endpoints, forwarded to the LS's own MCP-style tools over
 * LSP. The LS is the source of truth for each request shape, so match its
 * contract exactly rather than passing our own input through unreshaped — a
 * field the contract does not declare is dropped silently, with a successful
 * response and no warning.
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
  // The LS contract is SymbolSearchParameters { Query, Filters } — defined in
  // Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.dll, namespace
  // …LanguageModelTools.SymbolSearch. Filters must therefore be nested; spread
  // to the top level they match no property and are dropped without an error,
  // leaving only `query` (an object-name match) in effect.
  return client.request<unknown>("al/symbolSearch", {
    query: input.query,
    filters: input.filters ?? {},
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
