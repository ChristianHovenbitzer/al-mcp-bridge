/**
 * End-to-end: `al_symbol_search` must forward its filters to the AL language
 * server instead of dropping them. The bridge used to spread `filters` onto the
 * top level of the `al/symbolSearch` payload, where they match no property of
 * `SymbolSearchParameters { Query, Filters }`, so every `kinds` / `memberKinds`
 * / `objectName` / `limit` filter was discarded without an error and only the
 * free-text `query` — an object-name match — took effect. The call still
 * succeeded and still returned plausible symbols, just not the requested ones,
 * which is exactly why the bug survived normal use.
 *
 *   S1 — `kinds:["Codeunit"]` must return codeunits only. The fixture also owns
 *        a table, a page and a permission set, so a dropped filter shows up
 *        immediately as extra kinds in the result.
 *   S2 — `limit:1` must cap the result set at exactly one symbol.
 *   S3 — a filtered search must be strictly narrower than the same unfiltered
 *        query. Equal counts mean the filter never reached the server.
 */
import { test, before, after } from "node:test";
import assert from "node:assert/strict";
import { startBridge, fixturePath, waitFor } from "../helpers/bridge.mjs";

const FIXTURE = fixturePath("analyzers-sanity");
const TEST_TIMEOUT_MS = 90_000;

let bridge;

before(async () => {
  bridge = await startBridge({ workspace: FIXTURE });

  // `ready()` resolves once the workspace is set, not once the project is
  // indexed. Poll until the first search actually returns symbols so the
  // assertions below measure filtering, not a cold index.
  await waitFor(
    async () => (await search({ query: "*" })).length > 0,
    { timeoutMs: 60_000, intervalMs: 500, label: "symbol index warm" },
  );
});

after(async () => {
  await bridge?.close();
});

/** Call the tool and return its symbols, failing loudly on an unusable answer. */
async function search(args) {
  const res = await bridge.callTool("al_symbol_search", args);
  assert.ok(res.parsed, `al_symbol_search returned no JSON: ${res.raw.slice(0, 200)}`);
  assert.equal(res.parsed.succeeded, true, `al_symbol_search failed: ${res.parsed.message}`);
  assert.ok(Array.isArray(res.parsed.symbols), `no symbols array: ${res.raw.slice(0, 200)}`);
  return res.parsed.symbols;
}

test("S1 — kinds filter reaches the language server", { timeout: TEST_TIMEOUT_MS }, async () => {
  const symbols = await search({ query: "*", filters: { kinds: ["Codeunit"] } });
  assert.ok(symbols.length > 0, "the fixture owns two codeunits — expected at least one hit");

  const kinds = [...new Set(symbols.map((s) => s.kind))].sort();
  assert.deepEqual(
    kinds,
    ["Codeunit"],
    `kinds filter was dropped — the server also returned ${kinds.join(", ")}`,
  );
});

test("S2 — limit filter reaches the language server", { timeout: TEST_TIMEOUT_MS }, async () => {
  const symbols = await search({ query: "*", filters: { limit: 1 } });
  assert.equal(
    symbols.length,
    1,
    `limit filter was dropped — asked for 1, got ${symbols.length}`,
  );
});

test("S3 — a filtered search is narrower than the same unfiltered one", { timeout: TEST_TIMEOUT_MS }, async () => {
  const unfiltered = await search({ query: "*" });
  const filtered = await search({ query: "*", filters: { kinds: ["Codeunit"] } });
  assert.ok(
    filtered.length < unfiltered.length,
    `filtering changed nothing: ${filtered.length} of ${unfiltered.length} symbols`,
  );
});
