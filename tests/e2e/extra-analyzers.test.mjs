/**
 * End-to-end: AL_EXTRA_CODE_ANALYZERS must inject analyzers regardless of
 * what the workspace's `.vscode/settings.json` says. The README explicitly
 * promises this for team-wide enforcement, and at least one user shipped
 * a comma-separated value (instead of `;`) that silently dropped every
 * extra. Tests:
 *
 *   S1 — settings.json lists only MS cops; env injects ALCops.LinterCop.
 *        LC0090 (cognitive complexity) must surface from the env-supplied DLL.
 *   S2 — settings.json has NO `al.codeAnalyzers` key at all; env extras
 *        must still load on their own.
 *   S3 — settings.json sets `al.enableCodeAnalysis: false`. The env extras
 *        must override the master switch (always-active promise) and still
 *        produce diagnostics.
 *   S4 — env value uses `,` as a delimiter instead of `;`. Both must be
 *        accepted; otherwise users hit a silent drop.
 *
 * Each scenario builds a temp copy of the analyzers-sanity fixture with a
 * rewritten settings.json, boots the bridge with the env override, and
 * asserts on LC0090.
 */
import { test, before, after } from "node:test";
import assert from "node:assert/strict";
import { cpSync, existsSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";
import { fixturePath, readAlLsCurrent, REPO_ROOT, waitFor } from "../helpers/bridge.mjs";

const SRC_FIXTURE = fixturePath("analyzers-sanity");
const ALLS = readAlLsCurrent();
const LINTERCOP = join(ALLS.analyzersDir, "ALCops.LinterCop.dll");

// Skip the whole suite cleanly if the analyzer DLL isn't where we expect —
// the install script may not have run with a full Analyzers folder.
const SKIP_REASON = existsSync(LINTERCOP)
  ? false
  : `ALCops.LinterCop.dll not found at ${LINTERCOP}`;

const tmpDirs = [];
after(() => {
  for (const d of tmpDirs) {
    try {
      rmSync(d, { recursive: true, force: true });
    } catch {
      // best-effort cleanup
    }
  }
});

/**
 * Build a temp copy of the analyzers-sanity fixture with a custom
 * `.vscode/settings.json`, boot the bridge against it with the supplied
 * AL_EXTRA_CODE_ANALYZERS value (raw string, so tests can exercise both
 * delimiters), and return a tiny client that can be closed.
 */
async function bootBridge({ settingsJson, extraAnalyzersRaw }) {
  const work = mkdtempSync(join(tmpdir(), "al-extra-test-"));
  tmpDirs.push(work);
  cpSync(SRC_FIXTURE, work, { recursive: true });
  writeFileSync(join(work, ".vscode", "settings.json"), JSON.stringify(settingsJson, null, 2));

  const env = {
    ...process.env,
    AL_LS_PATH: ALLS.languageServerPath,
    AL_WORKSPACE: work,
    AL_DIAGNOSTICS_SETTLE_MS: "5000",
  };
  if (extraAnalyzersRaw !== undefined) env.AL_EXTRA_CODE_ANALYZERS = extraAnalyzersRaw;

  const distIndex = join(REPO_ROOT, "dist", "index.js");
  const transport = new StdioClientTransport({
    command: process.execPath,
    args: [distIndex],
    env,
    stderr: "inherit",
  });
  const client = new Client({ name: "extra-analyzers-test", version: "0" }, { capabilities: {} });
  await client.connect(transport);
  return {
    client,
    work,
    triggersFile: join(work, "src", "Diagnostics.Triggers.Codeunit.al"),
    async close() {
      await client.close().catch(() => {});
    },
  };
}

async function findLc0090(bridge) {
  // Prime every AL file once to schedule analyzer passes, then poll the
  // triggers file for LC0090.
  for (const f of [
    join(bridge.work, "src", "Diagnostics.Codeunit.al"),
    join(bridge.work, "src", "Diagnostics.Page.al"),
    bridge.triggersFile,
  ]) {
    await bridge.client.callTool({
      name: "al_get_diagnostics",
      arguments: { file: f, waitForFresh: true },
    });
  }
  return waitFor(
    async () => {
      const res = await bridge.client.callTool({
        name: "al_get_diagnostics",
        arguments: { file: bridge.triggersFile, waitForFresh: true },
      });
      const text = res.content?.[0]?.text ?? "";
      const parsed = JSON.parse(text);
      return (parsed.diagnostics ?? []).find((d) => d.code === "LC0090") ?? null;
    },
    { timeoutMs: 60_000, intervalMs: 1000, label: "LC0090 on triggers file" },
  );
}

test(
  "S1: env extras add to workspace al.codeAnalyzers — LinterCop fires even when not in settings.json",
  { timeout: 120_000, skip: SKIP_REASON },
  async () => {
    const bridge = await bootBridge({
      settingsJson: {
        "al.codeAnalyzers": [
          "${analyzerFolder}Microsoft.Dynamics.Nav.CodeCop.dll",
          "${analyzerFolder}Microsoft.Dynamics.Nav.UICop.dll",
          "${analyzerFolder}Microsoft.Dynamics.Nav.PerTenantExtensionCop.dll",
        ],
        "al.enableCodeAnalysis": true,
        "al.enableCodeActions": true,
        "al.backgroundCodeAnalysis": "File",
      },
      extraAnalyzersRaw: LINTERCOP,
    });
    try {
      const match = await findLc0090(bridge);
      assert.equal(match.code, "LC0090");
      assert.equal(match.severity, "warning");
    } finally {
      await bridge.close();
    }
  },
);

test(
  "S2: settings.json has no al.codeAnalyzers key — env-only extras still load",
  { timeout: 120_000, skip: SKIP_REASON },
  async () => {
    const bridge = await bootBridge({
      settingsJson: {
        "al.enableCodeAnalysis": true,
        "al.enableCodeActions": true,
        "al.backgroundCodeAnalysis": "File",
      },
      extraAnalyzersRaw: LINTERCOP,
    });
    try {
      const match = await findLc0090(bridge);
      assert.equal(match.code, "LC0090");
    } finally {
      await bridge.close();
    }
  },
);

test(
  "S3: workspace sets al.enableCodeAnalysis:false — env extras override the master switch",
  { timeout: 120_000, skip: SKIP_REASON },
  async () => {
    const bridge = await bootBridge({
      settingsJson: {
        "al.codeAnalyzers": [],
        "al.enableCodeAnalysis": false,
        "al.enableCodeActions": true,
        "al.backgroundCodeAnalysis": "File",
      },
      extraAnalyzersRaw: LINTERCOP,
    });
    try {
      const match = await findLc0090(bridge);
      assert.equal(match.code, "LC0090");
    } finally {
      await bridge.close();
    }
  },
);

test(
  "S4: env value uses ',' instead of ';' — both delimiters must be accepted",
  { timeout: 120_000, skip: SKIP_REASON },
  async () => {
    // Pair the analyzer DLL with a non-existent path on the OTHER side of the
    // comma. If the parser still treats the whole string as one mega-path,
    // existsSync fails on it and LC0090 never fires.
    const bridge = await bootBridge({
      settingsJson: {
        "al.codeAnalyzers": [
          "${analyzerFolder}Microsoft.Dynamics.Nav.CodeCop.dll",
        ],
        "al.enableCodeAnalysis": true,
        "al.enableCodeActions": true,
        "al.backgroundCodeAnalysis": "File",
      },
      extraAnalyzersRaw: `${LINTERCOP},/nonexistent/path/that/should/be/skipped.dll`,
    });
    try {
      const match = await findLc0090(bridge);
      assert.equal(match.code, "LC0090");
    } finally {
      await bridge.close();
    }
  },
);
