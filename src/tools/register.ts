import type { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import type { AlLspClient } from "../lsp/client.js";
import type { BridgeConfig } from "../config.js";
import { FindReferencesInput, findReferences } from "./references.js";
import { ApplyEditInput, createApplyEdit } from "./edit.js";

/**
 * Wires MCP tool declarations to their backing functions. Every tool goes
 * through the same shape: validated input via zod → LSP client call →
 * normalized return.
 *
 * Add new tools here. Stubs below mark the planned set.
 */
export function registerTools(
  mcp: McpServer,
  client: AlLspClient,
  config: BridgeConfig,
): void {
  const applyEdit = createApplyEdit(client, config);

  mcp.tool(
    "al_find_references",
    "Find all references to the symbol at the given position. " +
      "Uses textDocument/references — semantic, not textual.",
    FindReferencesInput.shape,
    async (input) => {
      const result = await findReferences(client, input);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    },
  );

  mcp.tool(
    "al_apply_edit",
    "Apply a full-text replacement to an AL file and return fresh " +
      "diagnostics from the AL compiler. Set persist=false to preview " +
      "without writing to disk.",
    ApplyEditInput.shape,
    async (input) => {
      const result = await applyEdit(input);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    },
  );

  // TODO — M1..M5 tool stubs, to be filled in following the two above.
  //
  //   al_document_outline      → textDocument/documentSymbol
  //   al_get_symbol_at         → textDocument/hover + definition
  //   al_rename                → textDocument/rename
  //   al_format                → textDocument/formatting | rangeFormatting
  //   al_list_code_actions     → textDocument/codeAction
  //   al_run_code_action       → al/runCodeAction
  //   al_symbol_search         → al/symbolSearch
  //   al_list_objects          → al/getApplicationObjects
  //   al_get_object            → al/getApplicationObject
  //   al_list_event_publishers → al/getEventPublishersRequest
  //   al_check_symbols         → al/checkSymbols
  //   al_get_diagnostics       → cache read, no LSP call
}
