#!/usr/bin/env node
/**
 * Offline smoke test for src/tools/compile.ts:
 *   - Real alc invocation on the analyzers-sanity fixture (no BC needed).
 *   - Verifies the tool surfaces both AL-compiler and analyzer diagnostics
 *     from the SARIF errorlog, and reports a non-zero exit.
 *
 * Usage: node scripts/smoke-compile.mjs
 */
import { createCompile } from "../dist/tools/compile.js";
import { loadConfig } from "../dist/config.js";
import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import assert from "node:assert/strict";

// Point AL_WORKSPACE at our test fixture so config picks it up.
const repoRoot = resolve(new URL("..", import.meta.url).pathname);
const fixturePath = resolve(repoRoot, "tests/fixtures/analyzers-sanity");
const alLsCurrentPath = resolve(repoRoot, "tests/.al-ls/current.json");
const alLsInfo = JSON.parse(readFileSync(alLsCurrentPath, "utf8"));

process.env.AL_LS_PATH = alLsInfo.languageServerPath;
process.env.AL_WORKSPACE = fixturePath;

const config = loadConfig();
const compile = createCompile(config);

console.log(`alc path: ${config.languageServerPath.replace(/[^/]+$/, "") + "alc"}`);
console.log(`project:  ${config.workspaceRoot}`);

const result = await compile({
  generateCode: false, // validation-only; no .app needed for this smoke
  continueOnError: true,
  verbose: true, // this smoke asserts on per-diagnostic detail (file/line/code)
});

// The fixture ships without symbols, so expect AL1022 + AL0305 + assorted cop diagnostics.
console.log(`succeeded: ${result.succeeded}`);
console.log(`exitCode:  ${result.exitCode}`);
console.log(`counts:    ${JSON.stringify(result.counts)}`);
console.log(`message:   ${result.message}`);
console.log(`first 5 diagnostics:`);
for (const d of result.diagnostics.slice(0, 5)) {
  const loc = d.file ? `${d.file.replace(fixturePath, "<fixture>")}:${d.startLine ?? "?"}:${d.startChar ?? "?"}` : "(no location)";
  console.log(`  [${d.severity}] ${d.code} @ ${loc} — ${d.message.split("\n")[0]}`);
}

// --- assertions ---
assert.equal(result.succeeded, false, "compile should fail on fixture (missing symbols + AL0305)");
assert.notEqual(result.exitCode, 0, "alc should exit non-zero when errors are present");
assert.ok(result.diagnostics.length > 0, "expected diagnostics to be parsed from errorlog");
assert.ok(
  result.diagnostics.some((d) => d.code === "AL0305"),
  "expected AL0305 (name > 20) from the PermissionSet fixture",
);
// AL1021 (no /packagecachepath) vs AL1022 (package not found in cache) —
// both are acceptable proofs that symbol resolution fired. The fixture
// intentionally has no .alpackages, so exactly one of these will surface.
assert.ok(
  result.diagnostics.some((d) => d.code === "AL1021" || d.code === "AL1022"),
  "expected AL1021 or AL1022 (symbol-resolution failure) on a fixture without symbols",
);

// A diagnostic with a file+region should have been parsed cleanly.
const localized = result.diagnostics.find((d) => d.file && d.startLine !== undefined);
assert.ok(
  localized,
  "expected at least one diagnostic with file + startLine from the SARIF locations[]",
);

console.log("\n✓ smoke-compile passed");
