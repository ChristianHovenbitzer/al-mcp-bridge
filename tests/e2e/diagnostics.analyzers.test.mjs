/**
 * End-to-end: every MS analyzer shipped in the AL extension must surface
 * diagnostics through the bridge. One test per cop, each asserting a
 * specific rule code AND its expected severity — regressions in either
 * direction (wrong code, wrong severity) should fail loudly.
 *
 * Fixture: `tests/fixtures/analyzers-sanity/` with all four MS cops enabled
 * in .vscode/settings.json.
 *
 * Rule inventory (empirically confirmed against AL 17.0.2273547):
 *
 *   CodeCop        AA0137  warning  Codeunit       unused local variable
 *   AppSourceCop   AS0062  error    Page           field/action needs ApplicationArea
 *   UICop          AW0006  info     Page           page needs UsageCategory+ApplicationArea
 *   PerTenantExt   PTE0008 error    Page           field/action needs ApplicationArea
 *   ApplicationCop AC0009  warning  PermissionSet  caption needs MaxLength=30 or Locked=true
 *   AL compiler    AL0305  error    PermissionSet  object identifier length > 20 chars
 *
 * A separate test tracks the third-party ALCops.LinterCop loader bug
 * (AD0001 — `ALCops.Common` can't resolve) so regressions there don't hide
 * behind the MS coverage.
 */
import { test, before, after } from "node:test";
import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { startBridge, fixturePath, waitFor } from "../helpers/bridge.mjs";

// Path to a built Socitas.ReviewerCop.dll. Not on nuget.org yet, so the
// test that depends on it is skipped when the env var is missing. Typical
// local value:
//   AL_REVIEWERCOP_DLL=~/git/ReviewerCop/src/Socitas.ReviewerCop/bin/Release/net8.0/Socitas.ReviewerCop.dll
const REVIEWERCOP_DLL = process.env.AL_REVIEWERCOP_DLL ?? "";
const REVIEWERCOP_AVAILABLE = REVIEWERCOP_DLL && existsSync(REVIEWERCOP_DLL);
const REVIEWERCOP_SKIP_REASON = REVIEWERCOP_AVAILABLE
  ? false
  : `AL_REVIEWERCOP_DLL unset or points to missing file (${REVIEWERCOP_DLL || "<empty>"})`;

const FIXTURE = fixturePath("analyzers-sanity");
const CODEUNIT_FILE = join(FIXTURE, "src", "Diagnostics.Codeunit.al");
const PAGE_FILE = join(FIXTURE, "src", "Diagnostics.Page.al");
const TABLE_FILE = join(FIXTURE, "src", "Diagnostics.Table.al");
const TRIGGERS_FILE = join(FIXTURE, "src", "Diagnostics.Triggers.Codeunit.al");
const PERMISSIONSET_FILE = join(FIXTURE, "src", "Diagnostics.PermissionSet.al");

let bridge;

before(async () => {
  bridge = await startBridge({
    workspace: FIXTURE,
    extraAnalyzers: REVIEWERCOP_AVAILABLE ? [REVIEWERCOP_DLL] : undefined,
  });
  // Prime every AL file once so the LS opens and schedules an analyzer pass
  // on each. Individual tests then await the specific rule they care about.
  for (const file of [CODEUNIT_FILE, PAGE_FILE, TABLE_FILE, TRIGGERS_FILE, PERMISSIONSET_FILE]) {
    await bridge.callTool("al_get_diagnostics", { file, waitForFresh: true });
  }
});

after(async () => {
  await bridge?.close();
});

test("bridge registers al_get_diagnostics tool", async () => {
  const tools = await bridge.client.listTools();
  const names = tools.tools.map((t) => t.name);
  assert.ok(
    names.includes("al_get_diagnostics"),
    `al_get_diagnostics missing; got [${names.join(", ")}]`,
  );
});

test("CodeCop (AA) surfaces AA0137 as warning on unused local", { timeout: 90_000 }, async () => {
  const match = await findDiagnostic(CODEUNIT_FILE, (d) => d.code === "AA0137");
  assert.equal(match.code, "AA0137");
  assert.equal(match.severity, "warning", `AA0137 severity should be warning, got ${match.severity}`);
  assert.match(
    match.message,
    /UnusedBuffer/,
    `AA0137 message should name the unused variable, got: ${match.message}`,
  );
});

test("AppSourceCop (AS) surfaces AS0062 as error on page without ApplicationArea", { timeout: 90_000 }, async () => {
  const match = await findDiagnostic(PAGE_FILE, (d) => d.code === "AS0062");
  assert.equal(match.code, "AS0062");
  assert.equal(match.severity, "error", `AS0062 severity should be error, got ${match.severity}`);
  assert.match(
    match.message,
    /ApplicationArea/,
    `AS0062 message should reference ApplicationArea, got: ${match.message}`,
  );
});

test("UICop (AW) surfaces AW0006 as info on page missing UsageCategory", { timeout: 90_000 }, async () => {
  const match = await findDiagnostic(PAGE_FILE, (d) => d.code === "AW0006");
  assert.equal(match.code, "AW0006");
  assert.equal(match.severity, "info", `AW0006 severity should be info, got ${match.severity}`);
  assert.match(
    match.message,
    /UsageCategory|ApplicationArea/,
    `AW0006 message should reference UsageCategory/ApplicationArea, got: ${match.message}`,
  );
});

test("PerTenantExtensionCop (PTE) surfaces PTE0008 as error on page without ApplicationArea", { timeout: 90_000 }, async () => {
  const match = await findDiagnostic(PAGE_FILE, (d) => d.code === "PTE0008");
  assert.equal(match.code, "PTE0008");
  assert.equal(match.severity, "error", `PTE0008 severity should be error, got ${match.severity}`);
  assert.match(
    match.message,
    /ApplicationArea/,
    `PTE0008 message should reference ApplicationArea, got: ${match.message}`,
  );
});

// Third-party analyzer coverage. Regression gate for the analyzer load-
// context bug: if sibling augmentation breaks, `ALCops.LinterCop` can't
// resolve `ALCops.Common` and every LinterCop rule crashes with AD0001.
// Guard by pre-checking app.json for AD0001 so a failure message names the
// exact load error rather than timing out waiting for LC diagnostics.
test("third-party ALCops.LinterCop loads and emits LC0090 as warning", { timeout: 90_000 }, async () => {
  const appJsonRes = await bridge.callTool("al_get_diagnostics", {
    file: join(FIXTURE, "app.json"),
    waitForFresh: false,
  });
  const ad0001 = (appJsonRes.parsed?.diagnostics ?? []).filter((d) => d.code === "AD0001");
  if (ad0001.length > 0) {
    const sample = ad0001[0].message.split("\n")[0];
    assert.fail(
      `ALCops.LinterCop load failed (${ad0001.length} AD0001 on app.json). ` +
        `Check src/config.ts:augmentWithAnalyzerSiblings rules.\n  first: ${sample}`,
    );
  }
  // LC0090 (cognitive complexity) reliably fires on the engineered nested
  // conditionals in Diagnostics.Triggers.Codeunit.al.
  const match = await findDiagnostic(TRIGGERS_FILE, (d) => d.code === "LC0090");
  assert.equal(match.code, "LC0090");
  assert.equal(match.severity, "warning", `LC0090 severity should be warning, got ${match.severity}`);
  assert.match(
    match.message,
    /Cognitive Complexity/i,
    `LC0090 message should mention Cognitive Complexity, got: ${match.message}`,
  );
});

// Third-party analyzer with a different layout than ALCops.LinterCop:
// `Socitas.ReviewerCop.dll` depends on its own `Socitas.ReviewerCop.Common.dll`
// plus the ALCops helpers. Gated on AL_REVIEWERCOP_DLL because the package
// isn't on nuget.org yet — when set, the test exercises the `socitas.*`
// sibling rule in config.ts. A regression in that rule surfaces here as
// AD0001 on app.json just like the LinterCop path.
test(
  "third-party Socitas.ReviewerCop loads and emits CC0009 as warning",
  { timeout: 90_000, skip: REVIEWERCOP_SKIP_REASON },
  async () => {
    const appJsonRes = await bridge.callTool("al_get_diagnostics", {
      file: join(FIXTURE, "app.json"),
      waitForFresh: false,
    });
    const ad0001 = (appJsonRes.parsed?.diagnostics ?? []).filter((d) => d.code === "AD0001");
    const reviewerCrashes = ad0001.filter((d) => /reviewercop/i.test(d.message));
    if (reviewerCrashes.length > 0) {
      const sample = reviewerCrashes[0].message.split("\n")[0];
      assert.fail(
        `Socitas.ReviewerCop load failed (${reviewerCrashes.length} AD0001). ` +
          `Check src/config.ts:ANALYZER_SIBLING_RULES for socitas.reviewercop.dll.\n  first: ${sample}`,
      );
    }

    // CC0009 = "Data Classification on Table". The fixture repeats
    // `DataClassification = CustomerContent` on a field that inherits it
    // from the table; ReviewerCop flags the redundancy.
    const match = await findDiagnostic(TABLE_FILE, (d) => d.code === "CC0009");
    assert.equal(match.code, "CC0009");
    assert.equal(match.severity, "warning", `CC0009 severity should be warning, got ${match.severity}`);
    assert.match(
      match.message,
      /DataClassification/,
      `CC0009 message should mention DataClassification, got: ${match.message}`,
    );
  },
);

// Permission set length rules. Two rules from two different sources:
//   AL0305 — AL compiler error, fires when the object identifier exceeds
//            20 chars. Proves AL-compiler diagnostics flow through the
//            bridge alongside analyzer diagnostics.
//   AC0009 — ALCops.ApplicationCop warning, fires on every permissionset
//            Caption that lacks `MaxLength=30` or `Locked=true` (so the
//            caption is safe against over-long translations). Fires
//            regardless of the literal caption length — it's a discipline
//            rule, not a string-length check.
// ApplicationCop isn't loaded by default in many AL projects; we load it
// explicitly in tests/fixtures/analyzers-sanity/.vscode/settings.json so
// this test can pin the behavior.
test("permission set: AL compiler surfaces AL0305 on object name > 20 chars", { timeout: 90_000 }, async () => {
  const match = await findOrDump(
    PERMISSIONSET_FILE,
    (d) => d.code === "AL0305",
    "AL0305 (object identifier length > 20) on permission set",
  );
  assert.equal(match.code, "AL0305");
  assert.equal(match.severity, "error", `AL0305 is a compiler error, got ${match.severity}`);
  assert.match(
    match.message,
    /cannot exceed 20 characters/i,
    `AL0305 message should state the 20-char limit, got: ${match.message}`,
  );
  assert.match(
    match.message,
    /Diag Sanity PermSet Name Way Too Long/,
    `AL0305 message should name the offending identifier, got: ${match.message}`,
  );
});

test("permission set: ApplicationCop surfaces AC0009 warning on Caption without MaxLength/Locked", { timeout: 90_000 }, async () => {
  const match = await findOrDump(
    PERMISSIONSET_FILE,
    (d) => d.code === "AC0009",
    "AC0009 (permission set caption length) on permission set",
  );
  assert.equal(match.code, "AC0009");
  assert.equal(match.severity, "warning", `AC0009 severity should be warning, got ${match.severity}`);
  assert.match(
    match.message,
    /Caption of permissionset/i,
    `AC0009 message should reference the permissionset Caption, got: ${match.message}`,
  );
  assert.match(
    match.message,
    /MaxLength=30|Locked=true/,
    `AC0009 message should point to the MaxLength/Locked escape hatches, got: ${match.message}`,
  );
});

// ---------------------------------------------------------------------------
// helpers
// ---------------------------------------------------------------------------

/**
 * Poll `al_get_diagnostics` on `file` until `predicate` matches a diagnostic.
 * Analyzer runs are asynchronous; we retry for up to 60s before failing.
 */
async function findDiagnostic(file, predicate) {
  return waitFor(
    async () => {
      const res = await bridge.callTool("al_get_diagnostics", { file, waitForFresh: true });
      return (res.parsed?.diagnostics ?? []).find(predicate) ?? null;
    },
    { timeoutMs: 60_000, intervalMs: 1000, label: `diagnostic on ${file}` },
  );
}

/**
 * Like `findDiagnostic`, but on timeout the error message lists every
 * diagnostic currently on the file. Use this when a missing diagnostic is
 * meaningful on its own (e.g. proving that a caption-length warning is
 * *not* being surfaced) — you want to see what WAS caught, not just "null".
 */
async function findOrDump(file, predicate, label) {
  try {
    return await waitFor(
      async () => {
        const res = await bridge.callTool("al_get_diagnostics", { file, waitForFresh: true });
        return (res.parsed?.diagnostics ?? []).find(predicate) ?? null;
      },
      { timeoutMs: 60_000, intervalMs: 1000, label },
    );
  } catch (err) {
    const res = await bridge.callTool("al_get_diagnostics", { file, waitForFresh: false });
    const diags = res.parsed?.diagnostics ?? [];
    const lines = diags.map((d) => `    [${d.severity}] ${d.code} — ${String(d.message).split("\n")[0]}`);
    const dump = lines.length ? lines.join("\n") : "    (no diagnostics on file)";
    throw new Error(`${err.message}\n  Diagnostics currently on file:\n${dump}`);
  }
}
