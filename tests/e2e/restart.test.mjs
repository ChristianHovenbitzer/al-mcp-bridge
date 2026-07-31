/**
 * e2e: al_restart_lsp against a real AL language server.
 *
 * Covers the two things the tool exists for: replacing a wedged/stale LS
 * process, and re-targeting the bridge at a different AL project without
 * restarting the MCP server (the bridge is one long-lived stdio process per
 * agent session, usually launched outside any AL repo).
 */
import test from "node:test";
import assert from "node:assert/strict";
import { fixturePath, startBridge } from "../helpers/bridge.mjs";

const WS = fixturePath("analyzers-sanity");

test("al_restart_lsp respawns the LS and keeps the bridge usable", async () => {
  const bridge = await startBridge({ workspace: WS });
  try {
    const before = await bridge.callTool("al_lsp_status");
    assert.equal(before.parsed.ls.generation, 1);
    assert.equal(before.parsed.ls.alive, true);
    const oldPid = before.parsed.ls.pid;
    assert.ok(oldPid, "expected a spawned LS pid");

    const restarted = await bridge.callTool("al_restart_lsp", {});
    assert.equal(restarted.parsed.restarted, true, restarted.raw);
    assert.equal(restarted.parsed.generation, 2);
    assert.equal(restarted.parsed.previous.pid, oldPid);
    assert.notEqual(restarted.parsed.current.pid, oldPid, "LS must be a new process");
    assert.deepEqual(restarted.parsed.current.workspaceFolders, [WS]);

    // The fresh generation must actually serve LSP traffic, not just exist.
    const objects = await bridge.callTool("al_list_objects", {});
    assert.ok(objects.parsed, `al_list_objects returned no JSON: ${objects.raw}`);

    const after = await bridge.callTool("al_lsp_status");
    assert.equal(after.parsed.ls.generation, 2);
    assert.equal(after.parsed.ls.alive, true);
    // The old generation's cache must not leak into the new one.
    assert.deepEqual(after.parsed.openDocuments, []);
  } finally {
    await bridge.close();
  }
});

test("al_restart_lsp re-targets the bridge at another AL project", async () => {
  // Second "project": the bridge repo's own fixture, re-registered from a
  // path the bridge did not start with. Any folder with an app.json qualifies.
  const bridge = await startBridge({ workspace: WS });
  try {
    const bad = await bridge.callTool("al_restart_lsp", { workspace: fixturePath(".") });
    assert.match(bad.raw, /No app\.json/, "a non-project path must be refused");

    const ok = await bridge.callTool("al_restart_lsp", { workspace: WS });
    assert.equal(ok.parsed.current.workspaceRoot, WS);

    const list = await bridge.callTool("al_list_workspaces");
    assert.deepEqual(
      list.parsed.workspaceFolders.map((w) => w.path),
      [WS],
    );
  } finally {
    await bridge.close();
  }
});
