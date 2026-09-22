/**
 * Unit tests for the al_compile size guard (buildCompileResult). Feeds
 * synthetic diagnostics — no alc, no LSP — and checks that above the
 * threshold the inline result is bounded, errors are never displaced by
 * warnings, and the complete list lands in the file the result points at.
 */
import test from "node:test";
import assert from "node:assert/strict";
import { existsSync, mkdtempSync, readFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import {
  DEFAULT_MAX_DIAGNOSTICS,
  MAX_RULE_ROWS,
  buildCompileResult,
  countByRule,
  resolveMaxDiagnostics,
  sortErrorsFirst,
} from "../../dist/tools/compile.js";

const PROJECT = "C:/fake/project";

/** n synthetic diagnostics; every 10th is an error (AL0118), the rest cycle
 *  through warning rule IDs AA0001..AA0020 across 60 distinct files. */
function synth(n) {
  const out = [];
  for (let i = 0; i < n; i++) {
    const isError = i % 10 === 0;
    out.push({
      severity: isError ? "error" : i % 7 === 0 ? "info" : "warning",
      code: isError ? "AL0118" : `AA${String((i % 20) + 1).padStart(4, "0")}`,
      file: `file:///c:/fake/project/src/File${i % 60}.al`,
      startLine: i,
      startChar: 0,
      endLine: i,
      endChar: 10,
      message: `synthetic diagnostic ${i}`,
    });
  }
  return out;
}

function build(diagnostics, overrides = {}) {
  return buildCompileResult({
    diagnostics,
    exitCode: 1,
    stdout: "",
    stderr: "",
    projectPath: PROJECT,
    alcPath: "/fake/alc",
    verbose: true,
    maxDiagnostics: DEFAULT_MAX_DIAGNOSTICS,
    ...overrides,
  });
}

test("below the threshold the result shape is unchanged", () => {
  const diags = synth(DEFAULT_MAX_DIAGNOSTICS);
  const r = build(diags);
  assert.equal(r.diagnostics.length, DEFAULT_MAX_DIAGNOSTICS);
  assert.deepEqual(r.diagnostics, diags, "verbose inline list is the untouched alc order");
  assert.equal(r.byRule, undefined);
  assert.equal(r.truncated, undefined);
  assert.equal(r.fullDiagnosticsPath, undefined);
  assert.equal(r.alcPath, "/fake/alc");
  assert.doesNotMatch(r.message, /exceed/);

  const quiet = build(diags, { verbose: false });
  assert.equal(quiet.diagnostics, undefined, "non-verbose still omits the array below threshold");
  assert.equal(quiet.alcPath, undefined);
});

test("above the threshold: summary shape, errors first, full list written to disk", () => {
  const dir = mkdtempSync(join(tmpdir(), "al-compile-guard-test-"));
  try {
    const total = 312;
    const diags = synth(total);
    const r = build(diags, { fullListDir: dir });

    // Totals by severity are for the WHOLE run, not the inline slice.
    const errors = diags.filter((d) => d.severity === "error").length;
    assert.equal(r.counts.error, errors);
    assert.equal(r.counts.error + r.counts.warning + r.counts.info, total);

    // Inline list: bounded, errors first, no error dropped for a warning.
    assert.equal(r.diagnostics.length, DEFAULT_MAX_DIAGNOSTICS);
    assert.ok(errors <= DEFAULT_MAX_DIAGNOSTICS, "fixture keeps errors within the budget");
    assert.equal(r.diagnostics.filter((d) => d.severity === "error").length, errors, "every error is inline");
    const firstNonError = r.diagnostics.findIndex((d) => d.severity !== "error");
    assert.equal(firstNonError, errors, "all errors precede the first warning");
    for (const d of r.diagnostics) assert.equal(typeof d.startLine, "number", "line numbers untouched");

    // Rule roll-up: capped rows, sorted by count desc, error rule present.
    assert.ok(r.byRule.length <= MAX_RULE_ROWS);
    const distinctRules = countByRule(diags).length;
    assert.ok(distinctRules > MAX_RULE_ROWS, `fixture has ${distinctRules} distinct rules, needs > ${MAX_RULE_ROWS}`);
    assert.equal(r.byRule.length, MAX_RULE_ROWS, "byRule is capped to the top rows");
    for (let i = 1; i < r.byRule.length; i++) assert.ok(r.byRule[i - 1].count >= r.byRule[i].count);
    const errRow = r.byRule.find((x) => x.code === "AL0118");
    assert.ok(errRow, "the error rule is in the top rows");
    assert.equal(errRow.severity, "error");
    assert.equal(errRow.count, errors);

    // Files capped (errors-first order is summarizeByFile's own sort).
    assert.equal(r.truncated.filesTotal, 60);
    assert.equal(r.files.length, DEFAULT_MAX_DIAGNOSTICS);
    assert.deepEqual(r.truncated, {
      total,
      shown: DEFAULT_MAX_DIAGNOSTICS,
      filesTotal: 60,
      filesShown: DEFAULT_MAX_DIAGNOSTICS,
      maxDiagnostics: DEFAULT_MAX_DIAGNOSTICS,
    });

    // Full list on disk: absolute path, under the given dir, valid JSON, every diagnostic present.
    assert.ok(r.fullDiagnosticsPath, "fullDiagnosticsPath is set");
    assert.ok(r.fullDiagnosticsPath.startsWith(dir), `written under ${dir}, got ${r.fullDiagnosticsPath}`);
    assert.ok(existsSync(r.fullDiagnosticsPath));
    const full = JSON.parse(readFileSync(r.fullDiagnosticsPath, "utf8"));
    assert.equal(full.projectPath, PROJECT);
    assert.equal(full.total, total);
    assert.equal(full.diagnostics.length, total);
    assert.equal(full.byRule.length, distinctRules, "file carries ALL rule rows, not just the top 15");
    assert.deepEqual(full.counts, r.counts);
    assert.deepEqual(full.diagnostics.slice(0, DEFAULT_MAX_DIAGNOSTICS), r.diagnostics, "inline slice is the file's head");
    // One diagnostic per line so the file can be read in slices.
    const lines = readFileSync(r.fullDiagnosticsPath, "utf8").split("\n").filter((l) => l.startsWith("  {"));
    assert.equal(lines.length, total);

    assert.match(r.message, /312 diagnostics exceed maxDiagnostics=40/);
    assert.ok(r.message.includes(r.fullDiagnosticsPath));
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("guard also trips for non-verbose callers and inlines the errors-first head", () => {
  const dir = mkdtempSync(join(tmpdir(), "al-compile-guard-test-"));
  try {
    const r = build(synth(100), { verbose: false, maxDiagnostics: 10, fullListDir: dir });
    assert.equal(r.diagnostics.length, 10);
    assert.equal(r.alcPath, undefined, "alcPath stays verbose-only");
    assert.equal(r.truncated.maxDiagnostics, 10);
    assert.ok(existsSync(r.fullDiagnosticsPath));
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("sortErrorsFirst is stable within a severity; countByRule keeps the highest severity", () => {
  const d = [
    { severity: "warning", code: "X", message: "w1" },
    { severity: "error", code: "X", message: "e1" },
    { severity: "info", code: "Y", message: "i1" },
    { severity: "warning", code: "X", message: "w2" },
    { severity: "error", message: "e2" },
  ];
  assert.deepEqual(
    sortErrorsFirst(d).map((x) => x.message),
    ["e1", "e2", "w1", "w2", "i1"],
  );
  assert.deepEqual(countByRule(d), [
    { code: "X", severity: "error", count: 3 },
    { code: "(none)", severity: "error", count: 1 },
    { code: "Y", severity: "info", count: 1 },
  ]);
});

test("resolveMaxDiagnostics: parameter > env > default; junk env falls back to default", () => {
  assert.equal(resolveMaxDiagnostics(undefined, {}), DEFAULT_MAX_DIAGNOSTICS);
  assert.equal(resolveMaxDiagnostics(undefined, { AL_BRIDGE_MAX_DIAGNOSTICS: "120" }), 120);
  assert.equal(resolveMaxDiagnostics(7, { AL_BRIDGE_MAX_DIAGNOSTICS: "120" }), 7);
  assert.equal(resolveMaxDiagnostics(undefined, { AL_BRIDGE_MAX_DIAGNOSTICS: "0" }), DEFAULT_MAX_DIAGNOSTICS);
  assert.equal(resolveMaxDiagnostics(undefined, { AL_BRIDGE_MAX_DIAGNOSTICS: "lots" }), DEFAULT_MAX_DIAGNOSTICS);
});
