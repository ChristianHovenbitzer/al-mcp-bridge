/**
 * Unit tests for al_compile_delta Phase 2: the machine-wide alc slot pool
 * and the base-result cache (key, store, LRU, dedupe). No alc.
 */
import test from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, readdirSync, rmSync, utimesSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { CancelledError, SlotBusyError, acquireAlcSlot, maxParallelAlc } from "../../dist/tools/alcSlots.js";
import { resolveSlotWaitMs } from "../../dist/tools/compile.js";
import {
  BaseCache,
  MATCHER_SCHEMA_VERSION,
  REMOTE_RULESET_TTL_MS,
  computeKey,
  rulesetDigest,
} from "../../dist/tools/compileDelta/cache.js";

const tmp = (p) => mkdtempSync(join(tmpdir(), p));

// ---------------------------------------------------------------------------
// slots
// ---------------------------------------------------------------------------

test("slots: N holders at most; release frees a slot; release is idempotent", async () => {
  const dir = tmp("al-slots-");
  try {
    const opts = { dir, slots: 2, staleMs: 60_000, waitMs: 300, pollMs: 20 };
    const a = await acquireAlcSlot(opts);
    const b = await acquireAlcSlot(opts);
    await assert.rejects(() => acquireAlcSlot(opts), SlotBusyError);
    a.release();
    a.release();
    const c = await acquireAlcSlot(opts);
    b.release();
    c.release();
    assert.deepEqual(readdirSync(dir), []);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("slots: a lock left by a dead pid is reclaimed", async () => {
  const dir = tmp("al-slots-");
  try {
    writeFileSync(join(dir, "slot-0.lock"), JSON.stringify({ pid: 999999, at: Date.now() }));
    const s = await acquireAlcSlot({ dir, slots: 1, staleMs: 60_000, waitMs: 500, pollMs: 20 });
    s.release();
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("slots: a lock older than staleMs is reclaimed even if its pid lives", async () => {
  const dir = tmp("al-slots-");
  try {
    writeFileSync(join(dir, "slot-0.lock"), JSON.stringify({ pid: process.pid, at: Date.now() - 10_000 }));
    const s = await acquireAlcSlot({ dir, slots: 1, staleMs: 1_000, waitMs: 500, pollMs: 20 });
    s.release();
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("slots: waiting reports progress and an abort cancels the wait", async () => {
  const dir = tmp("al-slots-");
  try {
    const held = await acquireAlcSlot({ dir, slots: 1, staleMs: 60_000, waitMs: 0 });
    const ac = new AbortController();
    const notes = [];
    const p = acquireAlcSlot({ dir, slots: 1, staleMs: 60_000, waitMs: 0, pollMs: 20, signal: ac.signal, onWait: (m) => notes.push(m) });
    setTimeout(() => ac.abort(), 80);
    await assert.rejects(p, CancelledError);
    assert.ok(notes.length >= 1 && /alc slot/.test(notes[0]));
    held.release();
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("slots: env parsing", () => {
  assert.equal(maxParallelAlc({}), 2);
  assert.equal(maxParallelAlc({ AL_BRIDGE_MAX_PARALLEL_ALC: "3" }), 3);
  assert.equal(maxParallelAlc({ AL_BRIDGE_MAX_PARALLEL_ALC: "0" }), 2);
  assert.equal(resolveSlotWaitMs(600_000, {}), 600_000);
  assert.equal(resolveSlotWaitMs(600_000, { AL_BRIDGE_SLOT_WAIT_MS: "5000" }), 5000);
  assert.equal(resolveSlotWaitMs(0, {}), 600_000);
});

// ---------------------------------------------------------------------------
// cache key
// ---------------------------------------------------------------------------

function keyInput(dir, over = {}) {
  return {
    baseSha: "a".repeat(40),
    appRel: "app",
    ruleSet: undefined,
    analyzers: [],
    packageCachePaths: [dir],
    alcPath: join(dir, "alc.exe"),
    assemblyProbingPaths: [],
    ...over,
  };
}

test("cache key: same-name same-size .app with different CONTENT changes the key", async () => {
  const dir = tmp("al-key-");
  try {
    const app = join(dir, "Microsoft_Base Application_27.5.0.0.app");
    writeFileSync(app, "AAAA");
    const k1 = await computeKey(keyInput(dir));
    writeFileSync(app, "BBBB");
    utimesSync(app, new Date(), new Date(Date.now() + 5000)); // memo is keyed on mtime
    const k2 = await computeKey(keyInput(dir));
    assert.notEqual(k1.key, k2.key);
    assert.equal((await computeKey(keyInput(dir))).key, k2.key, "stable when nothing changes");
    assert.notEqual((await computeKey(keyInput(dir, { baseSha: "b".repeat(40) }))).key, k2.key);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("rulesetDigest: follows local includes, flags remote ones", () => {
  const dir = tmp("al-rs-");
  try {
    const inc = join(dir, "inc.ruleset.json");
    const main = join(dir, "main.ruleset.json");
    writeFileSync(inc, JSON.stringify({ rules: [{ id: "AA0001", action: "Hidden" }] }));
    writeFileSync(main, JSON.stringify({ includedRuleSets: [{ path: "inc.ruleset.json" }, { path: "https://example.com/r.json" }] }));
    const d1 = rulesetDigest(main);
    assert.equal(d1.remote, true);
    writeFileSync(inc, JSON.stringify({ rules: [{ id: "AA0001", action: "Warning" }] }));
    assert.notEqual(rulesetDigest(main).digest, d1.digest, "an included file's content is part of the digest");
    assert.deepEqual(rulesetDigest(undefined), { digest: "(none)", remote: false });
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

// ---------------------------------------------------------------------------
// cache store
// ---------------------------------------------------------------------------

const entry = (key, over = {}) => ({
  schema: MATCHER_SCHEMA_VERSION,
  key,
  baseSha: "x",
  createdAt: Date.now(),
  remoteRuleset: false,
  rows: [{ code: "AA0137", line: 3 }],
  ...over,
});

test("cache: write/read round trip; wrong schema or expired remote entry is a miss", () => {
  const dir = tmp("al-cache-");
  try {
    const c = new BaseCache(dir);
    c.write(entry("k1"));
    assert.equal(c.read("k1").rows[0].code, "AA0137");
    assert.equal(c.read("nope"), undefined);
    c.write(entry("k2", { schema: MATCHER_SCHEMA_VERSION + 1 }));
    assert.equal(c.read("k2"), undefined);
    c.write(entry("k3", { remoteRuleset: true, createdAt: Date.now() - REMOTE_RULESET_TTL_MS - 1 }));
    assert.equal(c.read("k3"), undefined);
    assert.ok(!readdirSync(dir).some((n) => n.endsWith(".tmp")), "no temp files left behind");
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("cache: LRU evicts the least recently used beyond maxEntries", () => {
  const dir = tmp("al-cache-");
  try {
    const c = new BaseCache(dir, { maxEntries: 2 });
    c.write(entry("old"));
    utimesSync(join(dir, "old.json"), new Date(0), new Date(Date.now() - 60_000));
    c.write(entry("mid"));
    utimesSync(join(dir, "mid.json"), new Date(0), new Date(Date.now() - 30_000));
    c.read("old"); // touch: now the most recent
    c.write(entry("new"));
    assert.ok(c.read("old"), "touched entry survives");
    assert.ok(c.read("new"));
    assert.equal(c.read("mid"), undefined, "least recently used is evicted");
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("cache: concurrent callers for one key run compute exactly once", async () => {
  const dir = tmp("al-cache-");
  try {
    const c = new BaseCache(dir, { pollMs: 20 });
    let calls = 0;
    const compute = async () => {
      calls++;
      await new Promise((r) => setTimeout(r, 100));
      return entry("same");
    };
    const [a, b, d] = await Promise.all([c.getOrCompute("same", compute), c.getOrCompute("same", compute), c.getOrCompute("same", compute)]);
    assert.equal(calls, 1);
    assert.equal(a.cached, false);
    assert.equal(b.cached, true);
    assert.equal(d.cached, true);
    const again = await c.getOrCompute("same", compute);
    assert.equal(again.cached, true);
    assert.equal(calls, 1);
    assert.ok(!readdirSync(dir).some((n) => n.endsWith(".lock")), "lock released");
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("cache: another session's lock is waited on, then its result is used", async () => {
  const dir = tmp("al-cache-");
  try {
    mkdirSync(dir, { recursive: true });
    // Simulate another live bridge (this pid counts as alive) holding the lock.
    writeFileSync(join(dir, "shared.lock"), JSON.stringify({ pid: process.pid, at: Date.now() }));
    const c = new BaseCache(dir, { pollMs: 20 });
    const other = new BaseCache(dir);
    setTimeout(() => {
      other.write(entry("shared"));
      rmSync(join(dir, "shared.lock"), { force: true });
    }, 100);
    let calls = 0;
    const notes = [];
    const got = await c.getOrCompute("shared", async () => (calls++, entry("shared")), undefined, (m) => notes.push(m));
    assert.equal(calls, 0, "did not compile a base another session was compiling");
    assert.equal(got.cached, true);
    assert.ok(notes.some((m) => /another session/.test(m)));
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("cache: a failed compute releases the lock and caches nothing", async () => {
  const dir = tmp("al-cache-");
  try {
    const c = new BaseCache(dir, { pollMs: 20 });
    await assert.rejects(() => c.getOrCompute("bad", async () => {
      throw new Error("alc crashed");
    }));
    assert.equal(c.read("bad"), undefined);
    assert.deepEqual(readdirSync(dir), []);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});
