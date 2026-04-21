#!/usr/bin/env node
/**
 * Out-of-band smoke test: spawns the built MCP server over stdio, does the
 * MCP handshake, lists tools, and invokes `al_document_outline` on a real
 * AL file. Verifies the whole chain (MCP client → bridge → AL LS).
 *
 * Usage:
 *   AL_LS_PATH=... AL_WORKSPACE=... node scripts/smoke.mjs <al-file>
 *
 * NOTE: MCP stdio is newline-delimited JSON, not LSP Content-Length framing.
 * We use the official MCP SDK client, not vscode-jsonrpc.
 */
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";

const file = process.argv[2];
if (!file) {
  console.error("usage: smoke.mjs <absolute-path-to-.al-file>");
  process.exit(2);
}

const transport = new StdioClientTransport({
  command: process.execPath,
  args: ["dist/index.js"],
  env: process.env,
  stderr: "inherit",
});

const client = new Client(
  { name: "smoke", version: "0.0.1" },
  { capabilities: {} },
);

try {
  await client.connect(transport);
  console.error("[smoke] connected");

  const tools = await client.listTools();
  console.error(
    `[smoke] ${tools.tools.length} tools:`,
    tools.tools.map((t) => t.name).join(", "),
  );

  console.error("[smoke] al_list_objects");
  const list = await client.callTool({
    name: "al_list_objects",
    arguments: {},
  });
  console.log("=== al_list_objects ===");
  console.log((list.content[0].text ?? "").slice(0, 500));

  console.error("[smoke] al_document_outline on", file);
  const result = await client.callTool({
    name: "al_document_outline",
    arguments: { file },
  });
  console.log("=== al_document_outline ===");
  console.log(result.content[0].text);
} catch (e) {
  console.error("[smoke] failed:", e?.stack ?? e);
  process.exitCode = 1;
} finally {
  await client.close();
}
