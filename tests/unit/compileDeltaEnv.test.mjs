/**
 * Unit tests for al_compile_delta Phase 3: symbol-cache diagnosis from file
 * names and app.json only (two waves, platform mismatch, stale sibling).
 */
import test from "node:test";
import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import {
  compareVersions,
  environmentHints,
  listPackages,
  parsePackageFile,
  platformMismatch,
  staleSiblingPackages,
  symbolWaves,
} from "../../dist/tools/compileDelta/envHints.js";

test("parsePackageFile: names with spaces, versions with 2-4 parts, non-packages ignored", () => {
  assert.deepEqual(
    { ...parsePackageFile("Microsoft_Base Application_27.5.46862.50227.app"), dir: undefined },
    { publisher: "Microsoft", name: "Base Application", version: "27.5.46862.50227", dir: undefined, file: "Microsoft_Base Application_27.5.46862.50227.app" },
  );
  assert.equal(parsePackageFile("SOCITAS GmbH_General Customizations_27.1.20251128.5.app").name, "General Customizations");
  assert.equal(parsePackageFile("readme.txt"), undefined);
  assert.ok(compareVersions("27.10.0.0", "27.9.9.9") > 0);
});

test("symbolWaves: only packages with more than one version", () => {
  const pkgs = [
    "Microsoft_Base Application_27.4.45366.51998.app",
    "Microsoft_Base Application_27.5.46862.0.app",
    "Microsoft_System_27.0.46760.0.app",
  ].map((f) => parsePackageFile(f));
  assert.deepEqual(symbolWaves(pkgs), [{ id: "Microsoft_Base Application", versions: ["27.4.45366.51998", "27.5.46862.0"] }]);
});

test("platformMismatch: app.json 28 vs newest Base Application 27 is flagged; same major is not", () => {
  const pkgs = ["Microsoft_Base Application_27.5.46862.0.app", "Microsoft_Application_27.5.46862.0.app"].map((f) => parsePackageFile(f));
  assert.deepEqual(platformMismatch({ application: "28.0.0.0" }, pkgs), { appMajor: 28, cacheMajor: 27 });
  assert.equal(platformMismatch({ application: "27.0.0.0" }, pkgs), undefined);
  assert.equal(platformMismatch({}, pkgs), undefined);
});

test("staleSiblingPackages + environmentHints on a GC-shaped repo", () => {
  const repo = mkdtempSync(join(tmpdir(), "al-env-"));
  try {
    const app = join(repo, "app");
    const testProj = join(repo, "test");
    const cache = join(testProj, ".alpackages");
    mkdirSync(app, { recursive: true });
    mkdirSync(cache, { recursive: true });
    writeFileSync(join(app, "app.json"), JSON.stringify({ publisher: "SOCITAS GmbH", name: "General Customizations", application: "27.0.0.0", version: "27.1.20251128.6" }));
    writeFileSync(join(testProj, "app.json"), JSON.stringify({ publisher: "SOCITAS GmbH", name: "GC Tests", application: "28.0.0.0" }));
    for (const f of [
      "SOCITAS GmbH_General Customizations_27.1.20251128.5.app",
      "Microsoft_Base Application_27.4.45366.51998.app",
      "Microsoft_Base Application_27.5.46862.0.app",
    ]) {
      writeFileSync(join(cache, f), "x");
    }
    const pkgs = listPackages([cache]);
    assert.deepEqual(staleSiblingPackages(testProj, repo, pkgs), [
      { sibling: "app", file: "SOCITAS GmbH_General Customizations_27.1.20251128.5.app", packageVersion: "27.1.20251128.5", siblingVersion: "27.1.20251128.6" },
    ]);

    const hints = environmentHints(testProj, repo, [cache]);
    assert.equal(hints.length, 3, JSON.stringify(hints));
    assert.ok(hints.some((h) => /Platform mismatch: app.json application is 28.x .* 27.x/.test(h)));
    assert.ok(hints.some((h) => /several waves: Microsoft_Base Application \(27.4.45366.51998, 27.5.46862.0\)/.test(h)));
    assert.ok(hints.some((h) => /sibling project 'app' is at 27.1.20251128.6 — that package is stale/.test(h)));

    assert.deepEqual(environmentHints(app, repo, []).length, 1, "no cache path at all is its own hint");
  } finally {
    rmSync(repo, { recursive: true, force: true });
  }
});
