/**
 * Base-result cache for al_compile_delta.
 *
 * In a fix loop the base never changes, so only the head has to compile.
 * The key covers everything that can change the base diagnostics without
 * changing the base commit: the ruleset content (and local includes), the
 * analyzer DLLs, the CONTENT of every symbol package (the same version
 * string can hold different bytes — two-symbol-wave caches), and alc itself.
 *
 * Store: one JSON file per key under <deltaRoot>/cache, written atomically
 * (temp + rename), touched on read, LRU-evicted by count and size. Two
 * sessions asking for the same base wait on one compile: an in-process
 * promise map plus a cross-process `<key>.lock` file.
 */
import { createHash } from "node:crypto";
import {
  closeSync,
  createReadStream,
  existsSync,
  mkdirSync,
  openSync,
  readFileSync,
  readdirSync,
  renameSync,
  rmSync,
  statSync,
  utimesSync,
  writeFileSync,
  writeSync,
} from "node:fs";
import { basename, dirname, isAbsolute, join, resolve } from "node:path";
import { pidAlive } from "../alcSlots.js";
import type { DeltaRow } from "./match.js";

/** Bump when the cached row shape or normalization changes. */
export const MATCHER_SCHEMA_VERSION = 1;
export const MAX_ENTRIES = 50;
export const MAX_BYTES = 300 * 1024 * 1024;
/** Entries whose ruleset includes a remote URL are re-validated daily. */
export const REMOTE_RULESET_TTL_MS = 24 * 60 * 60 * 1000;

export interface CachedBase {
  schema: number;
  key: string;
  baseSha: string;
  createdAt: number;
  remoteRuleset: boolean;
  /** Every severity, unfiltered; the caller filters per request. */
  rows: DeltaRow[];
}

export interface KeyInput {
  baseSha: string;
  appRel: string;
  ruleSet?: string;
  analyzers: string[];
  packageCachePaths: string[];
  alcPath: string;
  assemblyProbingPaths: string[];
}

export interface CacheKey {
  key: string;
  remoteRuleset: boolean;
}

// ---------------------------------------------------------------------------
// Key
// ---------------------------------------------------------------------------

/** sha1 per .app, memoized by path + size + mtime for this process. */
const appDigestMemo = new Map<string, string>();

async function sha1File(path: string): Promise<string> {
  return new Promise((resolvePromise, reject) => {
    const h = createHash("sha1");
    createReadStream(path)
      .on("data", (b) => h.update(b))
      .on("error", reject)
      .on("end", () => resolvePromise(h.digest("hex")));
  });
}

async function appDigest(path: string): Promise<string> {
  const st = statSync(path);
  const memoKey = `${path}|${st.size}|${st.mtimeMs}`;
  let d = appDigestMemo.get(memoKey);
  if (!d) {
    d = await sha1File(path);
    appDigestMemo.set(memoKey, d);
  }
  return `${basename(path)}:${st.size}:${d}`;
}

function fileStamp(path: string): string {
  try {
    const st = statSync(path);
    return `${path}|${st.size}|${st.mtimeMs}`;
  } catch {
    return `${path}|missing`;
  }
}

/**
 * Ruleset content plus every local `includedRuleSets` file, recursively.
 * Remote includes (http/https) cannot be hashed offline: their URL goes into
 * the key and the entry gets a TTL instead.
 */
export function rulesetDigest(path: string | undefined): { digest: string; remote: boolean } {
  if (!path) return { digest: "(none)", remote: false };
  const h = createHash("sha256");
  let remote = false;
  const seen = new Set<string>();
  const visit = (p: string) => {
    const abs = resolve(p);
    if (seen.has(abs.toLowerCase())) return;
    seen.add(abs.toLowerCase());
    let text: string;
    try {
      text = readFileSync(abs, "utf8");
    } catch {
      h.update(`${abs}|missing\n`);
      return;
    }
    h.update(`${abs}\n${text}\n`);
    let parsed: { includedRuleSets?: Array<{ path?: string }> };
    try {
      parsed = JSON.parse(text.replace(/^﻿/, ""));
    } catch {
      return;
    }
    for (const inc of parsed.includedRuleSets ?? []) {
      const ip = inc.path;
      if (!ip) continue;
      if (/^https?:\/\//i.test(ip)) {
        remote = true;
        h.update(`remote:${ip}\n`);
        continue;
      }
      visit(isAbsolute(ip) ? ip : join(dirname(abs), ip));
    }
  };
  visit(path);
  return { digest: h.digest("hex"), remote };
}

export async function computeKey(k: KeyInput): Promise<CacheKey> {
  const h = createHash("sha256");
  h.update(`schema:${MATCHER_SCHEMA_VERSION}\nbase:${k.baseSha}\napp:${k.appRel}\n`);
  const rs = rulesetDigest(k.ruleSet);
  h.update(`ruleset:${rs.digest}\n`);
  for (const a of k.analyzers) h.update(`analyzer:${fileStamp(a)}\n`);
  for (const p of k.assemblyProbingPaths) h.update(`probe:${p}\n`);
  h.update(`alc:${fileStamp(k.alcPath)}\n`);
  for (const dir of k.packageCachePaths) {
    let entries: string[] = [];
    try {
      entries = readdirSync(dir).filter((e) => e.toLowerCase().endsWith(".app")).sort();
    } catch {
      h.update(`pkgdir:${dir}|missing\n`);
      continue;
    }
    h.update(`pkgdir:${dir}\n`);
    for (const e of entries) h.update(`app:${await appDigest(join(dir, e))}\n`);
  }
  return { key: h.digest("hex"), remoteRuleset: rs.remote };
}

// ---------------------------------------------------------------------------
// Store
// ---------------------------------------------------------------------------

export class BaseCache {
  private readonly inflight = new Map<string, Promise<CachedBase>>();

  constructor(
    readonly dir: string,
    private readonly opts: { maxEntries?: number; maxBytes?: number; lockStaleMs?: number; pollMs?: number } = {},
  ) {}

  private file(key: string): string {
    return join(this.dir, `${key}.json`);
  }

  read(key: string): CachedBase | undefined {
    const f = this.file(key);
    if (!existsSync(f)) return undefined;
    try {
      const entry = JSON.parse(readFileSync(f, "utf8")) as CachedBase;
      if (entry.schema !== MATCHER_SCHEMA_VERSION || entry.key !== key) return undefined;
      if (entry.remoteRuleset && Date.now() - entry.createdAt > REMOTE_RULESET_TTL_MS) return undefined;
      const now = new Date();
      utimesSync(f, now, now); // LRU touch
      return entry;
    } catch {
      return undefined; // Corrupt or mid-eviction: recompute.
    }
  }

  write(entry: CachedBase): void {
    mkdirSync(this.dir, { recursive: true });
    const f = this.file(entry.key);
    const tmp = `${f}.${process.pid}.${Math.random().toString(36).slice(2)}.tmp`;
    writeFileSync(tmp, JSON.stringify(entry), "utf8");
    renameSync(tmp, f);
    this.evict();
  }

  /** Drop least-recently-used entries beyond the count/size caps. */
  evict(): void {
    const maxEntries = this.opts.maxEntries ?? MAX_ENTRIES;
    const maxBytes = this.opts.maxBytes ?? MAX_BYTES;
    let files: Array<{ f: string; size: number; mtime: number }>;
    try {
      files = readdirSync(this.dir)
        .filter((n) => n.endsWith(".json"))
        .map((n) => {
          const f = join(this.dir, n);
          const st = statSync(f);
          return { f, size: st.size, mtime: st.mtimeMs };
        })
        .sort((a, b) => b.mtime - a.mtime);
    } catch {
      return;
    }
    let total = 0;
    files.forEach((x, i) => {
      total += x.size;
      if (i >= maxEntries || total > maxBytes) rmSync(x.f, { force: true });
    });
  }

  /**
   * Return the cached base for `key`, or run `compute` exactly once across
   * this process and other bridges, then store it.
   */
  async getOrCompute(
    key: string,
    compute: () => Promise<CachedBase>,
    signal?: AbortSignal,
    onWait?: (message: string) => void,
  ): Promise<{ entry: CachedBase; cached: boolean }> {
    const hit = this.read(key);
    if (hit) return { entry: hit, cached: true };
    const running = this.inflight.get(key);
    if (running) return { entry: await running, cached: true };

    const p = this.computeLocked(key, compute, signal, onWait);
    const shared = p.then((r) => r.entry);
    // Waiters still see a rejection; this only stops an unwaited failure from
    // surfacing as an unhandledRejection that could take the server down.
    shared.catch(() => {});
    this.inflight.set(key, shared);
    try {
      return await p;
    } finally {
      this.inflight.delete(key);
    }
  }

  private async computeLocked(
    key: string,
    compute: () => Promise<CachedBase>,
    signal?: AbortSignal,
    onWait?: (message: string) => void,
  ): Promise<{ entry: CachedBase; cached: boolean }> {
    mkdirSync(this.dir, { recursive: true });
    const lock = join(this.dir, `${key}.lock`);
    const staleMs = this.opts.lockStaleMs ?? 15 * 60 * 1000;
    const pollMs = this.opts.pollMs ?? 1000;
    let noticed = false;
    for (;;) {
      if (signal?.aborted) throw new Error("CANCELLED: waiting for another session's base compile was cancelled.");
      if (claim(lock, staleMs)) break;
      // Another bridge is compiling this base: wait for its result.
      if (!noticed) {
        noticed = true;
        onWait?.("another session is compiling the same base — waiting for its result");
      }
      await new Promise((r) => setTimeout(r, pollMs));
      const hit = this.read(key);
      if (hit) return { entry: hit, cached: true };
    }
    try {
      const again = this.read(key); // Filled while we were acquiring.
      if (again) return { entry: again, cached: true };
      const entry = await compute();
      this.write(entry);
      return { entry, cached: false };
    } finally {
      rmSync(lock, { force: true });
    }
  }
}

function claim(lock: string, staleMs: number): boolean {
  try {
    const fd = openSync(lock, "wx");
    try {
      writeSync(fd, JSON.stringify({ pid: process.pid, at: Date.now() }));
    } finally {
      closeSync(fd);
    }
    return true;
  } catch (err) {
    if ((err as NodeJS.ErrnoException).code !== "EEXIST") throw err;
  }
  try {
    const owner = JSON.parse(readFileSync(lock, "utf8")) as { pid?: number; at?: number };
    const alive = pidAlive(owner.pid ?? -1);
    if (!alive || Date.now() - (owner.at ?? 0) > staleMs) rmSync(lock, { force: true });
  } catch {
    // Being written or removed right now; retry on the next poll.
  }
  return false;
}

