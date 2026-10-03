/**
 * Unit tests for al_compile_delta's pure parts: the -U0 diff map, the
 * base↔head matcher, the verdict, and diagnostic normalization. Plus the
 * git snapshot/diff plumbing against a throwaway repo (git only, no alc).
 */
import test from "node:test";
import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { pathToFileURL } from "node:url";
import { addedLines, approxLine, mapLine, parseDiff } from "../../dist/tools/compileDelta/diffMap.js";
import { computeVerdict, digitsOnlyChange, matchDiagnostics } from "../../dist/tools/compileDelta/match.js";
import { classifyAicop, keyOf, normalizeDiagnostic, normalizeMessage, orderNew } from "../../dist/tools/compileDelta/index.js";
import {
  diffU0,
  repoInfo,
  resolveBase,
  snapshotCommit,
  snapshotWorktree,
} from "../../dist/tools/compileDelta/gitSnapshot.js";

// ---------------------------------------------------------------------------
// helpers
// ---------------------------------------------------------------------------

function row(over) {
  const r = {
    severity: "warning",
    code: "AA0001",
    file: "src/A.al",
    line: 10,
    column: 5,
    endLine: 10,
    endColumn: 9,
    message: "msg",
    ...over,
  };
  r.fileKey = keyOf(r.file);
  r.normMessage = r.normMessage ?? r.message;
  return r;
}

const DIFF_A = [
  "diff --git a/src/A.al b/src/A.al",
  "index 111..222 100644",
  "--- a/src/A.al",
  "+++ b/src/A.al",
  "@@ -5,0 +6,10 @@ codeunit 50100 A",          // 10 lines inserted after base line 5
  "+x",
  "@@ -20,2 +31,3 @@",                          // base 20-21 replaced by head 31-33
  "-y",
  "+z",
  "",
].join("\n");

function match(base, head, diffText = DIFF_A, extra = {}) {
  return matchDiagnostics({
    base,
    head,
    diff: parseDiff(diffText),
    wholeAddedFiles: extra.wholeAdded ?? new Set(),
    window: 3,
    keyOf,
  });
}

// ---------------------------------------------------------------------------
// diffMap
// ---------------------------------------------------------------------------

test("diffMap: insertion shifts later lines, leaves earlier ones", () => {
  const f = parseDiff(DIFF_A).files.get("src/A.al");
  assert.ok(f);
  assert.equal(f.status, "modified");
  assert.equal(mapLine(f, 5), 5, "line at the insertion point stays");
  assert.equal(mapLine(f, 6), 16, "line after a 10-line insertion moves by 10");
  assert.equal(mapLine(f, 19), 29);
  assert.equal(mapLine(f, 20), null, "replaced line has no exact image");
  assert.equal(mapLine(f, 21), null);
  assert.equal(mapLine(f, 22), 33, "after a 2→3 replacement: +10 +1");
  assert.deepEqual([...addedLines(f)].sort((a, b) => a - b), [6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 31, 32, 33]);
  assert.equal(approxLine(f, 21), 32, "approx anchors inside the replacing range");
});

test("diffMap: pure deletion, missing counts, rename, added, deleted, CRLF", () => {
  const text = [
    "diff --git a/src/B.al b/src/B.al",
    "--- a/src/B.al",
    "+++ b/src/B.al",
    "@@ -3 +2,0 @@",                         // base line 3 deleted (count defaults to 1)
    "-gone",
    "diff --git a/src/Old Name.al b/src/New Name.al",
    "similarity index 90%",
    "rename from src/Old Name.al",
    "rename to src/New Name.al",
    "--- a/src/Old Name.al",
    "+++ b/src/New Name.al",
    "@@ -1 +1 @@",
    "diff --git a/src/N.al b/src/N.al",
    "new file mode 100644",
    "--- /dev/null",
    "+++ b/src/N.al",
    "@@ -0,0 +1,4 @@",
    "diff --git a/src/D.al b/src/D.al",
    "deleted file mode 100644",
    "--- a/src/D.al",
    "+++ /dev/null",
    "@@ -1,2 +0,0 @@",
  ].join("\r\n");
  const d = parseDiff(text);
  const b = d.files.get("src/B.al");
  assert.equal(mapLine(b, 3), null);
  assert.equal(mapLine(b, 4), 3);
  assert.equal(mapLine(b, 2), 2);
  const r = d.files.get("src/New Name.al");
  assert.equal(r.status, "renamed");
  assert.equal(r.oldPath, "src/Old Name.al");
  const n = d.files.get("src/N.al");
  assert.equal(n.status, "added");
  assert.equal(n.oldPath, null);
  const del = d.files.get("src/D.al");
  assert.equal(del.status, "deleted");
  assert.equal(del.newPath, null);
});

test("diffMap: quoted paths are unquoted", () => {
  const d = parseDiff(['diff --git "a/src/Q\\"x.al" "b/src/Q\\"x.al"', '--- "a/src/Q\\"x.al"', '+++ "b/src/Q\\"x.al"', "@@ -1 +1 @@"].join("\n"));
  assert.ok(d.files.has('src/Q"x.al'));
});

// ---------------------------------------------------------------------------
// match
// ---------------------------------------------------------------------------

test("match: a warning shifted by an insertion above stays pre-existing", () => {
  const m = match([row({ line: 8 })], [row({ line: 18, endLine: 18 })]);
  assert.equal(m.newRows.length, 0);
  assert.equal(m.fixed.length, 0);
  assert.equal(m.preexisting.length, 1);
  assert.equal(m.preexisting[0].matchedBy, "exact");
  assert.equal(m.preexisting[0].onTouchedLine, false);
});

test("match: a new diagnostic on an added line is mine; on an untouched line it is induced", () => {
  const m = match([], [row({ line: 7 }), row({ line: 40, code: "LC0044" })]);
  const byLine = Object.fromEntries(m.newRows.map((r) => [r.line, r.attribution]));
  assert.deepEqual(byLine, { 7: "mine", 40: "induced" });
});

test("match: LC0044 partner in an UNTOUCHED file is new + induced, never pre-existing", () => {
  const base = [row({ file: "src/A.al", line: 3, code: "AA0137" })];
  const head = [
    row({ file: "src/A.al", line: 3, code: "AA0137" }),
    row({ file: "src/Other.TableExt.al", line: 12, code: "LC0044", message: "field id 50100 conflicts" }),
    row({ file: "src/A.al", line: 8, code: "LC0044", message: "field id 50100 conflicts" }),
  ];
  const m = match(base, head);
  assert.equal(m.preexisting.length, 1);
  const lc = m.newRows.filter((r) => r.code === "LC0044");
  assert.equal(lc.length, 2);
  assert.equal(lc.find((r) => r.file === "src/Other.TableExt.al").attribution, "induced");
  assert.equal(lc.find((r) => r.file === "src/A.al").attribution, "mine");
});

test("match: identical messages repeat — each base row is consumed once (multiset)", () => {
  const base = [row({ line: 30 }), row({ line: 30 })];
  const head = [row({ line: 40 }), row({ line: 40 }), row({ line: 40 })];
  const m = match(base, head);
  assert.equal(m.preexisting.length, 2);
  assert.equal(m.newRows.length, 1);
});

test("match: a removed diagnostic is fixed", () => {
  const m = match([row({ line: 2 })], []);
  assert.equal(m.fixed.length, 1);
});

test("match: changed message on a touched line is new with changedFrom", () => {
  // base line 20 is inside the replaced range, so it has no exact image; shift
  // into the head range: the "changed" pass needs a mapped line, so use line 22→33.
  const base = [row({ code: "LC0010", line: 22, message: "complexity 15" })];
  const head = [row({ code: "LC0010", line: 33, message: "complexity 18" })];
  const m = match(base, head);
  assert.equal(m.newRows.length, 1);
  assert.equal(m.newRows[0].changedFrom, "complexity 15");
  assert.equal(m.newRows[0].attribution, "mine");
  assert.equal(m.changed, 1);
});

test("match: digits-only change on an UNTOUCHED line stays pre-existing", () => {
  const base = [row({ code: "LC0010", line: 2, message: "complexity 15" })];
  const head = [row({ code: "LC0010", line: 2, message: "complexity 16" })];
  const m = match(base, head);
  assert.equal(m.newRows.length, 0);
  assert.equal(m.preexisting[0].matchedBy, "changed-digits");
});

test("match: a diagnostic inside a replaced hunk is matched by the shifted window and flagged onTouchedLine", () => {
  const base = [row({ line: 21 })];
  const head = [row({ line: 32 })];
  const m = match(base, head);
  assert.equal(m.newRows.length, 0);
  assert.equal(m.preexisting[0].matchedBy, "shifted");
  assert.equal(m.preexisting[0].onTouchedLine, true);
});

test("match: beyond the window it is fixed + new, not pre-existing", () => {
  const m = match([row({ line: 21 })], [row({ line: 60 })]);
  assert.equal(m.preexisting.length, 0);
  assert.equal(m.fixed.length, 1);
  assert.equal(m.newRows.length, 1);
});

test("match: rename maps base rows onto the head path", () => {
  const diff = ["diff --git a/src/Old.al b/src/New.al", "rename from src/Old.al", "rename to src/New.al", ""].join("\n");
  const m = match([row({ file: "src/Old.al", line: 4 })], [row({ file: "src/New.al", line: 4 })], diff);
  assert.equal(m.preexisting.length, 1);
  assert.equal(m.newRows.length, 0);
});

test("match: everything in a wholly added (untracked) file is mine", () => {
  const m = match([], [row({ file: "src/Untracked.al", line: 99 })], "", { wholeAdded: new Set([keyOf("src/Untracked.al")]) });
  assert.equal(m.newRows[0].attribution, "mine");
});

test("match: line-less (project) rows match on code + message", () => {
  const p = { file: "(project)", line: 0, column: 0, endLine: 0, endColumn: 0, code: "AL1021" };
  const m = match([row(p)], [row(p)]);
  assert.equal(m.preexisting.length, 1);
});

test("match golden: 2700-row base, 120 rows shifted, 3 inserted → exactly 3 new", () => {
  const base = [];
  const head = [];
  for (let i = 0; i < 2700; i++) {
    const file = `src/F${i % 30}.al`;
    const line = 100 + Math.floor(i / 30) * 3;
    base.push(row({ file, line, code: `AA${String(i % 50).padStart(4, "0")}`, message: `m${i % 7}` }));
  }
  // F0: 40 lines inserted after base line 100 → every F0 row with line > 100 shifts by 40.
  const diff = ["diff --git a/src/F0.al b/src/F0.al", "--- a/src/F0.al", "+++ b/src/F0.al", "@@ -100,0 +101,40 @@", ""].join("\n");
  for (const b of base) {
    const shifted = b.file === "src/F0.al" && b.line > 100 ? b.line + 40 : b.line;
    head.push(row({ ...b, line: shifted, endLine: shifted, normMessage: undefined }));
  }
  head.push(row({ file: "src/F0.al", line: 105, code: "AI0042", message: "new1" }));
  head.push(row({ file: "src/F0.al", line: 110, code: "AI0042", message: "new2" }));
  head.push(row({ file: "src/F5.al", line: 7, code: "LC0044", message: "new3" }));
  const m = match(base, head, diff);
  assert.equal(m.newRows.length, 3);
  assert.equal(m.fixed.length, 0);
  assert.equal(m.preexisting.length, 2700);
  assert.equal(m.newRows.filter((r) => r.attribution === "mine").length, 2);
});

test("digitsOnlyChange", () => {
  assert.equal(digitsOnlyChange("complexity 15", "complexity 18"), true);
  assert.equal(digitsOnlyChange("complexity 15", "complexity 15"), false);
  assert.equal(digitsOnlyChange("field X", "field Y"), false);
});

// ---------------------------------------------------------------------------
// verdict
// ---------------------------------------------------------------------------

test("verdict: base with errors is inconclusive (its warning baseline is unknown)", () => {
  assert.equal(computeVerdict({ baseErrors: 3, headErrors: 0, newRows: [], aicopMissing: false }), "inconclusive");
});

test("verdict: shared (environmental) errors only → inconclusive; a new error → head-errors", () => {
  assert.equal(computeVerdict({ baseErrors: 2, headErrors: 2, newRows: [], aicopMissing: false }), "inconclusive");
  assert.equal(
    computeVerdict({ baseErrors: 0, headErrors: 1, newRows: [{ severity: "error" }], aicopMissing: false }),
    "head-errors",
  );
});

test("verdict: AiCop missing can never be clean", () => {
  assert.equal(computeVerdict({ baseErrors: 0, headErrors: 0, newRows: [], aicopMissing: true }), "inconclusive");
  assert.equal(computeVerdict({ baseErrors: 0, headErrors: 0, newRows: [], aicopMissing: false }), "clean");
  assert.equal(
    computeVerdict({ baseErrors: 0, headErrors: 0, newRows: [{ severity: "warning" }], aicopMissing: false }),
    "new-diagnostics",
  );
});

test("classifyAicop: missing when absent or the file does not exist", () => {
  assert.equal(classifyAicop([], false), "missing");
  assert.equal(classifyAicop(["C:/nope/Socitas.AiCop.dll"], false), "missing");
});

test("orderNew: errors first, then mine before induced", () => {
  const rows = [
    { ...row({ line: 1 }), attribution: "induced" },
    { ...row({ line: 2 }), attribution: "mine" },
    { ...row({ line: 3, severity: "error" }), attribution: "induced" },
  ];
  assert.deepEqual(orderNew(rows).map((r) => r.line), [3, 2, 1]);
});

// ---------------------------------------------------------------------------
// normalization
// ---------------------------------------------------------------------------

test("normalizeDiagnostic: 0-based → 1-based, sandbox URI → project-relative path", () => {
  const sandbox = join(tmpdir(), "al-delta-norm", "head");
  const d = {
    severity: "warning",
    code: "AA0137",
    file: pathToFileURL(join(sandbox, "src", "X.Codeunit.al")).href,
    startLine: 9,
    startChar: 4,
    endLine: 9,
    endChar: 12,
    message: `Variable  in ${sandbox}\\src is   unused`,
  };
  const r = normalizeDiagnostic(d, sandbox);
  assert.equal(r.file, "src/X.Codeunit.al");
  assert.equal(r.line, 10);
  assert.equal(r.column, 5);
  assert.match(r.normMessage, /^Variable in <root>\\src is unused$/);
  assert.equal(normalizeDiagnostic({ ...d, file: undefined }, sandbox).file, "(project)");
  assert.equal(normalizeDiagnostic({ ...d, file: pathToFileURL(join(tmpdir(), "elsewhere.al")).href }, sandbox).file, "(external)");
});

test("normalizeMessage: base and head sandboxes normalize to the same text", () => {
  const a = normalizeMessage("see C:\\x\\d-1\\base\\src\\A.al", "C:\\x\\d-1\\base");
  const b = normalizeMessage("see c:/x/d-1/head/src/A.al".replace(/\//g, "\\"), "C:\\x\\d-1\\head");
  assert.equal(a, b);
});

// ---------------------------------------------------------------------------
// git plumbing (real git, no alc)
// ---------------------------------------------------------------------------

function sh(cwd, ...args) {
  return execFileSync("git", args, { cwd, encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] });
}

test("git: base snapshot via private index, worktree snapshot, -U0 diff relative to the project", () => {
  const repo = mkdtempSync(join(tmpdir(), "al-delta-git-"));
  const out = mkdtempSync(join(tmpdir(), "al-delta-out-"));
  try {
    sh(repo, "init", "-q", "-b", "main");
    sh(repo, "config", "user.email", "t@example.com");
    sh(repo, "config", "user.name", "t");
    sh(repo, "config", "core.autocrlf", "false");
    const app = join(repo, "app");
    mkdirSync(join(app, "src"), { recursive: true });
    writeFileSync(join(repo, ".gitignore"), ".alpackages/\n*.app\n");
    writeFileSync(join(app, "app.json"), "{}\n");
    writeFileSync(join(app, "src", "A.al"), "l1\nl2\nl3\n");
    sh(repo, "add", "-A");
    sh(repo, "commit", "-q", "-m", "base");
    const baseSha = sh(repo, "rev-parse", "HEAD").trim();
    sh(repo, "switch", "-q", "-c", "feature");

    // Worktree edits: insert a line, add an untracked file, create an ignored cache.
    writeFileSync(join(app, "src", "A.al"), "l1\nNEW\nl2\nl3\n");
    writeFileSync(join(app, "src", "U.al"), "untracked\n");
    mkdirSync(join(app, ".alpackages"), { recursive: true });
    writeFileSync(join(app, ".alpackages", "x.app"), "bin");

    const info = repoInfo(app);
    assert.equal(info.appRel, "app");
    assert.equal(resolveBase(app, "HEAD", undefined, "main").sha, baseSha);
    assert.throws(() => resolveBase(app, "HEAD", undefined, undefined), /BASE_UNRESOLVED/);
    assert.throws(() => resolveBase(app, "HEAD", "nope-ref", undefined), /BASE_UNRESOLVED/);

    const baseDir = join(out, "base");
    snapshotCommit(info, baseSha, baseDir);
    assert.equal(readFileSync(join(baseDir, "src", "A.al"), "utf8"), "l1\nl2\nl3\n");
    assert.ok(existsSync(join(baseDir, "app.json")), "project folder becomes the snapshot root");
    assert.equal(sh(repo, "status", "--porcelain").includes("A.al"), true, "repo index untouched");

    const headDir = join(out, "head");
    const snap = snapshotWorktree(app, headDir);
    assert.deepEqual(snap.untracked, ["src/U.al"]);
    assert.ok(existsSync(join(headDir, "src", "U.al")));
    assert.ok(!existsSync(join(headDir, ".alpackages")), "symbol cache never copied");

    const diff = parseDiff(diffU0(app, baseSha));
    const f = diff.files.get("src/A.al");
    assert.ok(f, "diff paths are project-relative");
    assert.deepEqual([...addedLines(f)], [2]);
    assert.equal(mapLine(f, 2), 3);
  } finally {
    rmSync(repo, { recursive: true, force: true });
    rmSync(out, { recursive: true, force: true });
  }
});
