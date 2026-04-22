/**
 * Spawn the built MCP bridge (`dist/index.js`) as a child, connect an MCP
 * client over stdio, and return a small wrapper tests can use.
 *
 * Reads `tests/.al-ls/current.json` to find the AL language server. Throws a
 * clear error (rather than silently skipping) if the install script hasn't
 * run — the alternative is a flaky "no tools" error from deep inside the LS
 * spawn path.
 */
import { readFileSync, existsSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";

const HERE = dirname(fileURLToPath(import.meta.url));
export const REPO_ROOT = resolve(HERE, "..", "..");

export function readAlLsCurrent() {
  const p = resolve(REPO_ROOT, "tests", ".al-ls", "current.json");
  if (!existsSync(p)) {
    throw new Error(
      `[bridge] ${p} not found — run \`npm run install:al-ls\` first.`,
    );
  }
  return JSON.parse(readFileSync(p, "utf8"));
}

export function fixturePath(name) {
  return resolve(REPO_ROOT, "tests", "fixtures", name);
}

/**
 * Boot the bridge with a fixture workspace and connect an MCP client.
 * Caller owns `close()` — always call it in a try/finally.
 *
 * @param {object} opts
 * @param {string} opts.workspace — absolute path to an AL project (app.json).
 * @param {string[]=} opts.extraAnalyzers — analyzer DLLs to layer on top of
 *   whatever `.vscode/settings.json` already specifies. Semicolon-joined
 *   into `AL_EXTRA_CODE_ANALYZERS` for the bridge.
 * @param {number=} opts.diagnosticsSettleMs — overrides `AL_DIAGNOSTICS_SETTLE_MS`.
 *   Tests usually want this higher than the 750ms default because first-pass
 *   analyzer runs on a cold workspace take several seconds.
 */
export async function startBridge(opts) {
  const info = readAlLsCurrent();
  const distIndex = resolve(REPO_ROOT, "dist", "index.js");
  if (!existsSync(distIndex)) {
    throw new Error(`[bridge] ${distIndex} not found — run \`npm run build\`.`);
  }

  const env = {
    ...process.env,
    AL_LS_PATH: info.languageServerPath,
    AL_WORKSPACE: opts.workspace,
    AL_DIAGNOSTICS_SETTLE_MS: String(opts.diagnosticsSettleMs ?? 5000),
  };
  if (opts.extraAnalyzers?.length) {
    env.AL_EXTRA_CODE_ANALYZERS = opts.extraAnalyzers.join(";");
  }

  const transport = new StdioClientTransport({
    command: process.execPath,
    args: [distIndex],
    env,
    stderr: "inherit", // surface LS + bridge logs during test runs
  });

  const client = new Client({ name: "al-mcp-bridge-tests", version: "0.0.1" }, { capabilities: {} });
  await client.connect(transport);

  return {
    client,
    info,
    async callTool(name, args = {}) {
      const res = await client.callTool({ name, arguments: args });
      const text = res.content?.[0]?.text ?? "";
      try {
        return { parsed: JSON.parse(text), raw: text };
      } catch {
        return { parsed: null, raw: text };
      }
    },
    async close() {
      await client.close().catch(() => {});
    },
  };
}

/**
 * Poll an async probe until it returns truthy or the budget expires.
 * Analyzer diagnostics arrive asynchronously after the first file open, so
 * tests that assert on them almost always need some form of retry.
 */
export async function waitFor(probe, { timeoutMs = 20000, intervalMs = 500, label = "condition" } = {}) {
  const deadline = Date.now() + timeoutMs;
  let lastValue;
  while (Date.now() < deadline) {
    lastValue = await probe();
    if (lastValue) return lastValue;
    await new Promise((r) => setTimeout(r, intervalMs));
  }
  throw new Error(`waitFor timed out after ${timeoutMs}ms — ${label}. Last value: ${JSON.stringify(lastValue)}`);
}
