/**
 * Regression: diagnostics must reflect out-of-band edits.
 *
 * The bridge mirrors each opened document to the LS via didOpen/didChange.
 * But Claude's native Edit/Write tools (and the user's editor) write the
 * .al file straight to disk, bypassing `al_apply_edit`. If the bridge only
 * syncs the LS buffer on its own edits, the LS keeps validating the text it
 * read at first `didOpen`, and `al_get_diagnostics` returns stale results
 * forever.
 *
 * This test opens a clean codeunit, overwrites it on disk to introduce an
 * unused local (CodeCop AA0137), and asserts the new diagnostic surfaces.
 */
import { test, before, after } from "node:test";
import assert from "node:assert/strict";
import { writeFileSync, rmSync } from "node:fs";
import { join } from "node:path";
import { startBridge, fixturePath } from "../helpers/bridge.mjs";

const FIXTURE = fixturePath("analyzers-sanity");
const PROBE_FILE = join(FIXTURE, "src", "StaleSyncProbe.Codeunit.al");

const CLEAN = `codeunit 50190 "Stale Sync Probe"
{
    procedure DoWork(): Integer
    begin
        exit(42);
    end;
}
`;

const WITH_UNUSED_LOCAL = `codeunit 50190 "Stale Sync Probe"
{
    procedure DoWork(): Integer
    var
        UnusedProbeVar: Integer;
    begin
        exit(42);
    end;
}
`;

let bridge;

before(async () => {
  writeFileSync(PROBE_FILE, CLEAN, "utf8");
  bridge = await startBridge({ workspace: FIXTURE });
});

after(async () => {
  await bridge?.close();
  rmSync(PROBE_FILE, { force: true });
});

test(
  "out-of-band disk edit surfaces new diagnostics (AA0137)",
  { timeout: 90_000 },
  async () => {
    // 1. Open the clean file. didOpen fires; baseline has no AA0137.
    const baseline = await bridge.callTool("al_get_diagnostics", {
      file: PROBE_FILE,
      waitForFresh: true,
    });
    const baselineHasUnused = (baseline.parsed?.diagnostics ?? []).some(
      (d) => d.code === "AA0137",
    );
    assert.equal(baselineHasUnused, false, "baseline should have no AA0137");

    // 2. Simulate Claude's Edit/Write tool: write to disk, no al_apply_edit.
    writeFileSync(PROBE_FILE, WITH_UNUSED_LOCAL, "utf8");

    // 3. The unused local must now surface. Poll: analyzer runs are async.
    const match = await pollForDiagnostic(
      PROBE_FILE,
      (d) => d.code === "AA0137",
      60_000,
    );
    assert.ok(
      match,
      "AA0137 should surface after the on-disk edit — diagnostics are stale " +
        "because the LS buffer was not re-synced from disk",
    );
    assert.equal(match.severity, "warning");
  },
);

async function pollForDiagnostic(file, predicate, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const res = await bridge.callTool("al_get_diagnostics", {
      file,
      waitForFresh: true,
    });
    const found = (res.parsed?.diagnostics ?? []).find(predicate);
    if (found) return found;
    await new Promise((r) => setTimeout(r, 1000));
  }
  return null;
}
