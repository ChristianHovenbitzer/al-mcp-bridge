#!/usr/bin/env node
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { loadConfig } from "./config.js";
import { AlLspClient } from "./lsp/client.js";
import { registerTools } from "./tools/register.js";

async function main(): Promise<void> {
  const config = loadConfig();
  const lsp = new AlLspClient(config);

  process.stderr.write(
    `[al-mcp-bridge] starting AL LS: ${config.languageServerPath}\n`,
  );
  process.stderr.write(`[al-mcp-bridge] workspace: ${config.workspaceRoot}\n`);

  await lsp.start();

  const mcp = new McpServer(
    { name: "al-mcp-bridge", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );
  registerTools(mcp, lsp, config);

  const transport = new StdioServerTransport();
  await mcp.connect(transport);

  const shutdown = async () => {
    await lsp.stop();
    process.exit(0);
  };
  process.on("SIGINT", shutdown);
  process.on("SIGTERM", shutdown);
}

main().catch((err) => {
  process.stderr.write(`[al-mcp-bridge] fatal: ${err?.stack ?? err}\n`);
  process.exit(1);
});
