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
  process.stderr.write(
    `[al-mcp-bridge] ${config.workspaceFolders.length} AL project(s) discovered:\n`,
  );
  for (const f of config.workspaceFolders) {
    process.stderr.write(`[al-mcp-bridge]   - ${f}\n`);
  }
  if (config.resolvedViaDownwardScan) {
    // Workspace inferred by scanning subfolders of cwd — usually means the
    // bridge was launched from a place that doesn't share an `app.json`
    // ancestor with the AL project the user actually wants. Loud warning so
    // callers debugging "no diagnostics returned" can spot the misalignment.
    process.stderr.write(
      `[al-mcp-bridge] WARNING: no app.json found upward from cwd; the workspace(s) above were ` +
        `discovered by scanning subfolders. If this isn't your target project, set AL_WORKSPACE ` +
        `or call the al_load_workspace tool at runtime.\n`,
    );
  }

  // Kick LSP init in the background so MCP is responsive immediately;
  // tool calls await `lspReady` individually.
  const lspReady = lsp.start().then(
    (r) => {
      process.stderr.write(`[al-mcp-bridge] LSP initialized\n`);
      return r;
    },
    (err) => {
      process.stderr.write(`[al-mcp-bridge] LSP init failed: ${err?.message ?? err}\n`);
      throw err;
    },
  );

  const mcp = new McpServer(
    { name: "al-mcp-bridge", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );
  registerTools(mcp, lsp, config, lspReady);

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
