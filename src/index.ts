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
  if (config.workspaceFolders.length === 0) {
    process.stderr.write(
      `[al-mcp-bridge] no workspace loaded - the bridge never guesses one.\n` +
        `[al-mcp-bridge] Call al_load_workspace with the absolute path of the AL project root ` +
        `(the folder holding app.json) before using any LSP-backed tool.\n` +
        `[al-mcp-bridge] al_compile / al_publish / al_run_tests work without it - pass projectPath.\n`,
    );
  } else {
    process.stderr.write(
      `[al-mcp-bridge] ${config.workspaceFolders.length} AL project(s) preloaded via AL_WORKSPACE:\n`,
    );
    for (const f of config.workspaceFolders) {
      process.stderr.write(`[al-mcp-bridge]   - ${f}\n`);
    }
  }

  // Kick LSP init in the background so MCP is responsive immediately; tool
  // calls await `client.ready()` individually (bounded). The rejection is
  // consumed here so a failed startup surfaces as a per-tool error rather than
  // an unhandled rejection that takes the whole bridge down - al_lsp_status
  // and al_restart_lsp stay callable in that state.
  const firstGeneration = lsp.getGeneration() + 1;
  lsp.launch().then(
    () => process.stderr.write(`[al-mcp-bridge] LSP initialized\n`),
    (err) => {
      // A restart disposes the previous connection, which rejects its pending
      // `initialize`. That's not a startup failure - don't report it as one.
      if (lsp.getGeneration() !== firstGeneration) return;
      process.stderr.write(
        `[al-mcp-bridge] LSP init failed: ${err?.message ?? err}\n` +
          `[al-mcp-bridge] call al_restart_lsp to retry (optionally with a different workspace)\n`,
      );
    },
  );

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
