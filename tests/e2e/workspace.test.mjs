/**
 * Coverage for the workspace lifecycle tools and the file-in-workspace
 * guard. These behaviors exist because the bridge was silently returning
 * empty diagnostics whenever a tool was called on a file outside the LSP's
 * resolved workspace — usually after the bridge launched from a directory
 * that didn't share an `app.json` ancestor with the user's actual project.
 */
import { test, before, after } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { startBridge, fixturePath } from "../helpers/bridge.mjs";

const FIXTURE = fixturePath("analyzers-sanity");
const OUTSIDE_FILE = join(tmpdir(), "al-mcp-bridge-not-in-ws.al");

let bridge;

// Synthesize a second AL project under a temp dir so we can exercise
// `al_load_workspace` against a real `app.json`.
let secondaryProject;

before(async () => {
  // Stub file outside any AL project; used to drive the guard.
  writeFileSync(OUTSIDE_FILE, "// not an AL workspace member\n");

  secondaryProject = mkdtempSync(join(tmpdir(), "al-mcp-bridge-ws-"));
  writeFileSync(
    join(secondaryProject, "app.json"),
    JSON.stringify(
      {
        id: "11111111-1111-1111-1111-111111111111",
        name: "WorkspaceTestSecondary",
        publisher: "Tests",
        version: "1.0.0.0",
        platform: "1.0.0.0",
        application: "26.0.0.0",
        runtime: "15.0",
        idRanges: [{ from: 50000, to: 50099 }],
      },
      null,
      2,
    ),
  );
  mkdirSync(join(secondaryProject, "src"), { recursive: true });
  writeFileSync(
    join(secondaryProject, "src", "Hello.Codeunit.al"),
    `codeunit 50000 "Hello Secondary"
{
}
`,
  );

  bridge = await startBridge({ workspace: FIXTURE });
});

after(async () => {
  await bridge?.close();
  rmSync(OUTSIDE_FILE, { force: true });
  if (secondaryProject) rmSync(secondaryProject, { recursive: true, force: true });
});

test("al_get_diagnostics on a file outside any loaded workspace returns a guard error", async () => {
  const res = await bridge.callTool("al_get_diagnostics", {
    file: OUTSIDE_FILE,
    waitForFresh: false,
  });
  // MCP SDK serializes thrown errors into the result text rather than
  // throwing on the client side, so we inspect the raw payload.
  assert.match(
    res.raw,
    /not inside any loaded AL workspace/,
    `expected guard message, got: ${res.raw.slice(0, 400)}`,
  );
  assert.match(
    res.raw,
    /al_load_workspace/,
    "guard message must point callers at the al_load_workspace tool",
  );
});

test("al_list_workspaces returns the initial fixture workspace as primary", async () => {
  const res = await bridge.callTool("al_list_workspaces", {});
  const folders = res.parsed?.workspaceFolders ?? [];
  assert.ok(folders.length >= 1, `expected at least one folder, got ${JSON.stringify(folders)}`);
  const primary = folders.find((f) => f.isPrimary);
  assert.ok(primary, `expected a primary workspace, got ${JSON.stringify(folders)}`);
  assert.equal(primary.path, FIXTURE);
});

test("al_load_workspace registers a new project and the guard then accepts its files", async () => {
  const load = await bridge.callTool("al_load_workspace", { path: secondaryProject });
  assert.equal(load.parsed?.added, true, `expected added=true, got ${load.raw}`);
  assert.equal(load.parsed?.alreadyLoaded, false);
  assert.ok(
    load.parsed.workspaceFolders.includes(secondaryProject),
    `secondary not in workspace folders: ${JSON.stringify(load.parsed.workspaceFolders)}`,
  );

  // After loading, the guard should pass for files inside the new project.
  const file = join(secondaryProject, "src", "Hello.Codeunit.al");
  const res = await bridge.callTool("al_get_diagnostics", {
    file,
    waitForFresh: false,
  });
  // Either an empty diagnostics array (parse not yet complete) or a real
  // diagnostics list — but never the guard error string.
  assert.doesNotMatch(
    res.raw,
    /not inside any loaded AL workspace/,
    `guard should be cleared after al_load_workspace, got: ${res.raw.slice(0, 400)}`,
  );
});

test("al_load_workspace is idempotent for an already-registered folder", async () => {
  const again = await bridge.callTool("al_load_workspace", { path: secondaryProject });
  assert.equal(again.parsed?.added, false);
  assert.equal(again.parsed?.alreadyLoaded, true);
});

test("al_load_workspace rejects a path without app.json", async () => {
  const noProject = mkdtempSync(join(tmpdir(), "al-mcp-bridge-nows-"));
  try {
    const res = await bridge.callTool("al_load_workspace", { path: noProject });
    assert.match(
      res.raw,
      /No app\.json/,
      `expected app.json validation error, got: ${res.raw.slice(0, 400)}`,
    );
  } finally {
    rmSync(noProject, { recursive: true, force: true });
  }
});
