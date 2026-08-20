#!/usr/bin/env node
/**
 * Offline smoke test for src/tools/publish.ts:
 *   - launch.json parsing (reused from runTests.ts)
 *   - missing-credentials error shape (no creds in message)
 *   - missing-.app error shape
 *
 * No BC server is contacted. For the real round-trip, point at a live
 * dev service tier with BC_USER/BC_PASSWORD set and pass a valid .app.
 *
 * Usage: node scripts/smoke-publish.mjs [path-to-al-project-with-launch.json]
 */
import { createPublish } from "../dist/tools/publish.js";
import assert from "node:assert/strict";

const projectPath =
  process.argv[2] ?? "/path/to/al-project/test";

// ---------- 1. missing-credentials error ------------------------------------
{
  delete process.env.BC_USER;
  delete process.env.BC_PASSWORD;
  process.env.XDG_CONFIG_HOME = "/tmp/al-mcp-bridge-smoke-empty-publish";

  const publish = createPublish(projectPath);
  let threw = false;
  try {
    // appPath points at /dev/null so file existence passes but no bytes
    // would ever be sent; credential check fires before that anyway.
    await publish({
      projectPath,
      appPath: "/dev/null",
      schemaUpdateMode: "synchronize",
      forceUpgrade: false,
      dependencyPublishingOption: "default",
    });
  } catch (err) {
    threw = true;
    const msg = String(err?.message ?? err);
    assert.ok(
      msg.includes("No credentials available"),
      `Expected credential-missing error, got: ${msg}`,
    );
    assert.ok(!/Basic\s+[A-Za-z0-9+/=]+/.test(msg), "Error leaks Basic token");
    assert.ok(!/https?:\/\/[^\s:]+:[^@\s]+@/.test(msg), "Error leaks inline creds");
    console.log("✓ launch.json parsed, credential-missing error is clean");
    console.log("  error:", msg);
  }
  assert.ok(threw, "Expected credential-missing error but none was thrown");
}

// ---------- 2. missing-.app error -------------------------------------------
{
  process.env.BC_USER = "x";
  process.env.BC_PASSWORD = "y";
  const publish = createPublish(projectPath);
  let threw = false;
  try {
    await publish({
      projectPath,
      appPath: "/tmp/al-mcp-bridge-does-not-exist.app",
      schemaUpdateMode: "synchronize",
      forceUpgrade: false,
      dependencyPublishingOption: "default",
    });
  } catch (err) {
    threw = true;
    const msg = String(err?.message ?? err);
    assert.ok(
      msg.includes("appPath does not exist"),
      `Expected missing-.app error, got: ${msg}`,
    );
    console.log("✓ explicit appPath that doesn't exist is rejected cleanly");
  }
  assert.ok(threw, "Expected missing-.app error");
}

// ---------- 3. unknown launchConfig name ------------------------------------
{
  const publish = createPublish(projectPath);
  let threw = false;
  try {
    await publish({
      projectPath,
      launchConfig: "this-config-does-not-exist",
      schemaUpdateMode: "synchronize",
      forceUpgrade: false,
      dependencyPublishingOption: "default",
    });
  } catch (err) {
    threw = true;
    const msg = String(err?.message ?? err);
    assert.ok(
      msg.includes("No launch configuration named"),
      `Expected missing-config error, got: ${msg}`,
    );
    console.log("✓ unknown launchConfig name is rejected cleanly");
  }
  assert.ok(threw, "Expected missing-config error");
}

console.log("\nAll offline smoke checks passed.");
