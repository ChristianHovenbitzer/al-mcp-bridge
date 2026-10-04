/**
 * Cross-process cap on concurrent alc runs.
 *
 * Every Claude session runs its own bridge, and a GC-sized alc with six
 * analyzers takes 1-2 GB. On a 16 GB machine a handful of sessions compiling
 * at once is how it starts paging, so al_compile and al_compile_delta share
 * one machine-wide pool of N slots (AL_BRIDGE_MAX_PARALLEL_ALC, default 2).
 *
 * A slot is a lock file created with O_EXCL. It records the owner's pid; a
 * slot whose owner is gone (crashed session) or that is older than the alc
 * deadline is reclaimed, so a dead bridge can never wedge the pool.
 *
 * The cap is soft: two sessions reclaiming the same stale slot at the same
 * instant can both end up compiling. That costs one extra alc for one run,
 * which is acceptable for a memory guard and not worth a lock protocol.
 */
import { closeSync, mkdirSync, openSync, readFileSync, rmSync, statSync, writeSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

export const DEFAULT_MAX_PARALLEL_ALC = 2;

export class SlotBusyError extends Error {
  constructor(waitedMs: number, slots: number) {
    super(
      `BUSY: all ${slots} alc slot(s) stayed taken for ${Math.round(waitedMs / 1000)}s — other sessions are compiling. ` +
        "Retry later, or raise AL_BRIDGE_MAX_PARALLEL_ALC / AL_BRIDGE_SLOT_WAIT_MS.",
    );
    this.name = "SlotBusyError";
  }
}

export class CancelledError extends Error {
  constructor(what: string) {
    super(`CANCELLED: ${what} was cancelled.`);
    this.name = "CancelledError";
  }
}

export function maxParallelAlc(env: NodeJS.ProcessEnv = process.env): number {
  const n = Number(env.AL_BRIDGE_MAX_PARALLEL_ALC?.trim());
  return Number.isInteger(n) && n >= 1 ? n : DEFAULT_MAX_PARALLEL_ALC;
}

export function slotDir(): string {
  return join(process.env.LOCALAPPDATA ?? tmpdir(), "al-mcp-bridge", "slots");
}

export interface SlotOptions {
  /** Reclaim a slot older than this even if its pid looks alive (pid reuse). */
  staleMs: number;
  /** Give up with SlotBusyError after waiting this long. */
  waitMs: number;
  signal?: AbortSignal;
  /** Called once when the caller has to wait, and then every ~15 s. */
  onWait?: (message: string) => void;
  /** Overrides for tests. */
  dir?: string;
  slots?: number;
  pollMs?: number;
}

export interface Slot {
  release(): void;
}

export function pidAlive(pid: number): boolean {
  if (!Number.isInteger(pid) || pid <= 0) return false;
  if (pid === process.pid) return true;
  try {
    process.kill(pid, 0);
    return true;
  } catch (err) {
    // EPERM: exists but owned by someone else.
    return (err as NodeJS.ErrnoException).code === "EPERM";
  }
}

function tryClaim(file: string, staleMs: number): boolean {
  if (tryClaimOnce(file)) return true;
  // Taken — reclaim it if the owner is gone or the lock outlived any alc run.
  try {
    const owner = JSON.parse(readFileSync(file, "utf8")) as { pid?: number; at?: number };
    const age = Date.now() - (owner.at ?? statSync(file).mtimeMs);
    if (!pidAlive(owner.pid ?? -1) || (staleMs > 0 && age > staleMs)) {
      rmSync(file, { force: true });
      return tryClaimOnce(file);
    }
  } catch {
    // Half-written by its owner, or removed between our calls: try again next poll.
  }
  return false;
}

function tryClaimOnce(file: string): boolean {
  let fd: number;
  try {
    fd = openSync(file, "wx");
  } catch (err) {
    if ((err as NodeJS.ErrnoException).code === "EEXIST") return false;
    throw err;
  }
  try {
    writeSync(fd, JSON.stringify({ pid: process.pid, at: Date.now() }));
  } finally {
    closeSync(fd);
  }
  return true;
}

/** Wait for a free slot. Resolves to a handle whose release() is idempotent. */
export async function acquireAlcSlot(opts: SlotOptions): Promise<Slot> {
  const dir = opts.dir ?? slotDir();
  const slots = opts.slots ?? maxParallelAlc();
  const pollMs = opts.pollMs ?? 1000;
  mkdirSync(dir, { recursive: true });
  const started = Date.now();
  let lastNotice = 0;

  for (;;) {
    if (opts.signal?.aborted) throw new CancelledError("waiting for an alc slot");
    for (let i = 0; i < slots; i++) {
      const file = join(dir, `slot-${i}.lock`);
      if (tryClaim(file, opts.staleMs)) {
        let released = false;
        return {
          release() {
            if (released) return;
            released = true;
            rmSync(file, { force: true });
          },
        };
      }
    }
    const waited = Date.now() - started;
    if (opts.waitMs > 0 && waited >= opts.waitMs) throw new SlotBusyError(waited, slots);
    if (opts.onWait && (lastNotice === 0 || Date.now() - lastNotice >= 15_000)) {
      lastNotice = Date.now();
      opts.onWait(`waiting for an alc slot (${slots} in use by other sessions, ${Math.round(waited / 1000)}s so far)`);
    }
    await sleep(pollMs, opts.signal);
  }
}

function sleep(ms: number, signal?: AbortSignal): Promise<void> {
  return new Promise((resolve) => {
    const t = setTimeout(done, ms);
    function done() {
      clearTimeout(t);
      signal?.removeEventListener("abort", done);
      resolve();
    }
    signal?.addEventListener("abort", done, { once: true });
  });
}
