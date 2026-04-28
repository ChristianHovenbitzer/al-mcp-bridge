#!/usr/bin/env node
/**
 * Offline smoke test for src/tools/runTests.ts:
 *   - launch.json parsing
 *   - credential-missing error shape (no creds in message)
 *   - redact() helper
 *
 * Usage: node scripts/smoke-runtests.mjs <path-to-al-project-with-launch.json>
 *
 * For the real end-to-end test against BC, set BC_USER / BC_PASSWORD and run
 * the `al_run_tests` tool via an MCP client (e.g. Claude Code).
 */

import { createRunTests, redact } from "../dist/tools/runTests.js";
import assert from "node:assert/strict";

const projectPath =
  process.argv[2] ?? "/home/hoch/git/proaurum/General Customizations/test";

// ---------- 1. redact() ------------------------------------------------------
{
  const cases = [
    [
      'GET https://u:p@host/dev/TestRunnerHub failed: Authorization: Basic abc123==',
      ["u:p@", "abc123==", "Basic abc123=="],
    ],
    [
      '{"password":"hunter2","user":"admin"}',
      ["hunter2"],
    ],
    [
      "Auth query: ...&Authentication=Basic%20dXNlcjpwdw==&tenant=default",
      ["dXNlcjpwdw==", "Basic%20dXNlcjpwdw=="],
    ],
  ];
  for (const [input, forbidden] of cases) {
    const out = redact(input);
    for (const needle of forbidden) {
      assert.ok(
        !out.includes(needle),
        `redact() leaked "${needle}" — got: ${out}`,
      );
    }
  }
  console.log("✓ redact() scrubs credentials");
}

// ---------- 2. launch.json + missing-creds error shape ----------------------
{
  delete process.env.BC_USER;
  delete process.env.BC_PASSWORD;
  // Point XDG to a dir we know does NOT contain a credentials file.
  process.env.XDG_CONFIG_HOME = "/tmp/al-mcp-bridge-smoke-empty";

  const runTests = createRunTests(projectPath);
  let threw = false;
  try {
    await runTests({ codeunitId: 99999, launchConfig: "run tests from here" });
  } catch (err) {
    threw = true;
    const msg = String(err?.message ?? err);
    assert.ok(
      msg.includes("No credentials available"),
      `Expected credential-missing error, got: ${msg}`,
    );
    // Must not include anything that looks like a secret or a full URL with creds.
    assert.ok(!/Basic\s+[A-Za-z0-9+/=]+/.test(msg), "Error leaks Basic token");
    assert.ok(!/https?:\/\/[^\s:]+:[^@\s]+@/.test(msg), "Error leaks inline creds");
    console.log("✓ launch.json parsed, credential-missing error is clean");
    console.log("  error:", msg);
  }
  assert.ok(threw, "Expected credential-missing error but none was thrown");
}

// ---------- 3. launch.json missing config name -----------------------------
{
  process.env.BC_USER = "x";
  process.env.BC_PASSWORD = "y";
  const runTests = createRunTests(projectPath);
  let threw = false;
  try {
    await runTests({
      codeunitId: 99999,
      launchConfig: "this-config-does-not-exist",
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
console.log(
  "To run a real test, set BC_USER and BC_PASSWORD and invoke the al_run_tests MCP tool.",
);
