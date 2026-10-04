/**
 * E2E for al_compile_delta: a throwaway git repo built at test time (no
 * nested repo is committed), real alc, real analyzers, driven over MCP.
 *
 * Symbols: alc needs Microsoft System/Application packages. Point
 * AL_DELTA_E2E_PACKAGES at any .alpackages folder that has them (e.g. a BC 28
 * app's cache); without it the green-path tests skip with a visible reason.
 * The no-symbols test always runs: it is the "environmental errors on both
 * sides" case and must come out inconclusive, never clean.
 *
 * Windows: run directly (`node --test --test-reporter=spec tests/e2e/compileDelta.test.mjs`
 * after `npm run build`), because `pretest:e2e` bails on win32.
 */
import test from "node:test";
import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { cpSync, existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { fixturePath, startBridge } from "../helpers/bridge.mjs";

const PACKAGES = process.env.AL_DELTA_E2E_PACKAGES;
const SKIP_NO_PACKAGES = PACKAGES && existsSync(PACKAGES)
  ? false
  : "AL_DELTA_E2E_PACKAGES is not set to a .alpackages folder with Microsoft System/Application symbols";

function sh(cwd, ...args) {
  return execFileSync("git", ["-c", "user.email=t@example.com", "-c", "user.name=t", ...args], {
    cwd,
    encoding: "utf8",
    stdio: ["ignore", "pipe", "pipe"],
  });
}

const cu = (body) => `codeunit 50100 "Diag Delta"\n{\n${body}}\n`;
// Base: one procedure with an unused local (AA0137 on line 5).
const PROC_A = `    procedure A()\n    var\n        U: Integer;\n    begin\n    end;\n`;
// Head: 4 lines inserted ABOVE it (its AA0137 moves to line 9 — must stay
// pre-existing), plus a new procedure with its own unused local on line 15.
const HEAD = cu(`    procedure Pad()\n    begin\n    end;\n\n` + PROC_A + `\n    procedure B()\n    var\n        V: Integer;\n    begin\n    end;\n`);

function makeRepo() {
  const repo = mkdtempSync(join(tmpdir(), "al-delta-e2e-"));
  const app = join(repo, "app");
  mkdirSync(join(app, "src"), { recursive: true });
  mkdirSync(join(app, ".vscode"), { recursive: true });
  cpSync(join(fixturePath("analyzers-sanity"), "app.json"), join(app, "app.json"));
  // CodeCop + UICop only: AppSourceCop raises AS0051 manifest errors on the fixture app.json.
  writeFileSync(
    join(app, ".vscode", "settings.json"),
    JSON.stringify({
      "al.codeAnalyzers": [
        "${analyzerFolder}Microsoft.Dynamics.Nav.CodeCop.dll",
        "${analyzerFolder}Microsoft.Dynamics.Nav.UICop.dll",
      ],
    }),
  );
  writeFileSync(join(app, "src", "Diag.Codeunit.al"), cu(PROC_A));
  writeFileSync(join(repo, ".gitignore"), ".alpackages/\n.alcache/\n*.app\n");
  sh(repo, "init", "-q", "-b", "main");
  sh(repo, "add", "-A");
  sh(repo, "commit", "-q", "-m", "base");
  sh(repo, "switch", "-q", "-c", "feature");
  return { repo, app };
}

async function withRepo(fn) {
  const { repo, app } = makeRepo();
  const bridge = await startBridge({ workspace: app });
  try {
    await fn({ app, bridge });
  } finally {
    await bridge.close();
    rmSync(repo, { recursive: true, force: true });
  }
}

test("al_compile_delta: shifted pre-existing stays, added unused local is new + mine, 1-based", { timeout: 600_000, skip: SKIP_NO_PACKAGES }, async () => {
  await withRepo(async ({ app, bridge }) => {
    writeFileSync(join(app, "src", "Diag.Codeunit.al"), HEAD);
    const { parsed, raw } = await bridge.callTool("al_compile_delta", { projectPath: app, targetBranch: "main", packageCachePath: PACKAGES });
    assert.ok(parsed, `tool returned non-JSON: ${raw.slice(0, 500)}`);
    assert.equal(parsed.lineBase, 1);
    assert.equal(parsed.base.analyzersSuppressed, false, `base did not compile: ${JSON.stringify(parsed.errors ?? parsed.environment)}`);

    const aa = parsed.new.filter((r) => r.code === "AA0137");
    assert.equal(aa.length, 1, `exactly one new AA0137, got ${JSON.stringify(parsed.new.map((r) => [r.code, r.line]))}`);
    assert.equal(aa[0].attribution, "mine");
    assert.equal(aa[0].line, 15, "V sits on 1-based line 15");
    assert.ok(!parsed.new.some((r) => r.line >= 5 && r.line <= 11), "procedure A's shifted diagnostics are not new");
    assert.equal(parsed.delta.fixed, 0);
    assert.equal(parsed.verdict, "new-diagnostics");
    assert.ok(parsed.message.length > 0);

    const report = readFileSync(parsed.fullReportPath, "utf8");
    assert.match(report, /"class":"preexisting"/, "the shifted AA0137 is in the report as pre-existing");
    assert.ok(JSON.stringify(parsed).length < 15_000, "inline result stays under 15 KB");
  });
});

test("al_compile_delta: second call reuses the cached base and streams progress", { timeout: 600_000, skip: SKIP_NO_PACKAGES }, async () => {
  await withRepo(async ({ app, bridge }) => {
    writeFileSync(join(app, "src", "Diag.Codeunit.al"), HEAD);
    const args = { projectPath: app, baseRef: "main", packageCachePath: PACKAGES };
    const call = async () => {
      const notes = [];
      const res = await bridge.client.callTool({ name: "al_compile_delta", arguments: args }, undefined, {
        onprogress: (p) => notes.push(p.message),
        timeout: 600_000,
      });
      return { parsed: JSON.parse(res.content[0].text), notes };
    };
    const first = await call();
    const second = await call();
    assert.equal(first.parsed.base.cached, false);
    assert.equal(second.parsed.base.cached, true, "base comes from the cache on the second call");
    assert.ok(second.parsed.timingsMs.baseCompile < first.parsed.timingsMs.baseCompile);
    assert.deepEqual(second.parsed.delta, first.parsed.delta, "cached base gives the same answer");
    assert.ok(first.notes.some((m) => /base compile/.test(m)), `progress seen: ${JSON.stringify(first.notes)}`);
    assert.ok(second.notes.some((m) => /base from cache/.test(m)));
  });
});

test("al_compile_delta: a compile error yields head-errors with null warning counts", { timeout: 600_000, skip: SKIP_NO_PACKAGES }, async () => {
  await withRepo(async ({ app, bridge }) => {
    writeFileSync(join(app, "src", "Diag.Codeunit.al"), cu(PROC_A.replace("    end;\n", "        NotAThing := 1;\n    end;\n")));
    const { parsed, raw } = await bridge.callTool("al_compile_delta", { projectPath: app, baseRef: "main", packageCachePath: PACKAGES });
    assert.ok(parsed, `tool returned non-JSON: ${raw.slice(0, 500)}`);
    assert.equal(parsed.verdict, "head-errors");
    assert.equal(parsed.head.analyzersSuppressed, true);
    assert.equal(parsed.head.counts.warning, null, "never 0 when analyzers were suppressed");
    assert.ok(parsed.errors?.some((e) => e.tag === "new" && e.line === 7));
  });
});

test("al_compile_delta: errors on both sides (no symbols) are environmental and inconclusive", { timeout: 600_000 }, async () => {
  await withRepo(async ({ app, bridge }) => {
    const empty = join(app, ".alpackages");
    mkdirSync(empty, { recursive: true });
    writeFileSync(join(app, "src", "Diag.Codeunit.al"), HEAD);
    const { parsed, raw } = await bridge.callTool("al_compile_delta", { projectPath: app, baseRef: "main", packageCachePath: empty });
    assert.ok(parsed, `tool returned non-JSON: ${raw.slice(0, 500)}`);
    assert.equal(parsed.verdict, "inconclusive");
    assert.equal(parsed.base.counts.warning, null);
    assert.ok(parsed.delta.environmental > 0);
    assert.equal(parsed.delta.new, 0);
    assert.ok(parsed.environment.hints.some((h) => /symbol cache/.test(h)));
  });
});

test("al_compile_delta: missing base is an actionable error", { timeout: 300_000 }, async () => {
  await withRepo(async ({ app, bridge }) => {
    const { raw } = await bridge.callTool("al_compile_delta", { projectPath: app });
    assert.match(raw, /BASE_UNRESOLVED/);
  });
});
