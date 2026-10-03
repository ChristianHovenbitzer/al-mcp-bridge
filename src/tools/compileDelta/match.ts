/**
 * Base↔head diagnostic matching for al_compile_delta. Pure: no git, no alc,
 * no filesystem, so every rule here is unit-testable with synthetic rows.
 *
 * All lines are 1-based; 0 means "no line" (project-level diagnostics).
 */
import { addedLines, approxLine, mapLine, type DiffMap, type FileDiff } from "./diffMap.js";
import type { CompileDiagnostic } from "../compile.js";

export interface DeltaRow {
  severity: CompileDiagnostic["severity"];
  /** Rule id, or "(none)". */
  code: string;
  /** Project-relative path with `/`, or "(project)" / "(external)". */
  file: string;
  /** Comparison key for `file` (lower-cased on case-insensitive file systems). */
  fileKey: string;
  line: number;
  column: number;
  endLine: number;
  endColumn: number;
  message: string;
  /** Message with the sandbox root replaced and whitespace collapsed. */
  normMessage: string;
}

export type Attribution = "mine" | "induced";

export interface NewRow extends DeltaRow {
  attribution: Attribution;
  /** For a "changed" match: the base message this row replaced. */
  changedFrom: string | null;
}

export interface PreexistingRow extends DeltaRow {
  /** The head line sits on a line the change added or replaced. */
  onTouchedLine: boolean;
  matchedBy: "exact" | "changed-digits" | "shifted";
}

export interface MatchResult {
  newRows: NewRow[];
  preexisting: PreexistingRow[];
  fixed: DeltaRow[];
  changed: number;
}

export interface MatchInput {
  base: DeltaRow[];
  head: DeltaRow[];
  diff: DiffMap;
  /** Keys of head files that exist only on the head side (new or untracked). */
  wholeAddedFiles: Set<string>;
  /** ±lines for the "shifted" pass. */
  window: number;
  /** Turns a diff path into a fileKey; must match how DeltaRow.fileKey was built. */
  keyOf: (path: string) => string;
}

/** Messages equal once every digit run is masked (e.g. "complexity 15" vs "complexity 18"). */
export function digitsOnlyChange(a: string, b: string): boolean {
  return a !== b && a.replace(/\d+/g, "#") === b.replace(/\d+/g, "#");
}

export function matchDiagnostics(input: MatchInput): MatchResult {
  const { base, head, diff, wholeAddedFiles, window, keyOf } = input;

  // Diff lookups by fileKey. Renames map a base key onto its head key.
  const diffByHead = new Map<string, FileDiff>();
  const baseToHead = new Map<string, string | null>();
  for (const f of diff.files.values()) {
    if (f.newPath) diffByHead.set(keyOf(f.newPath), f);
    if (f.oldPath) baseToHead.set(keyOf(f.oldPath), f.newPath ? keyOf(f.newPath) : null);
  }
  const addedByHead = new Map<string, Set<number>>();
  const touched = (fileKey: string, line: number): boolean => {
    if (wholeAddedFiles.has(fileKey)) return true;
    const f = diffByHead.get(fileKey);
    if (!f || line <= 0) return false;
    let set = addedByHead.get(fileKey);
    if (!set) {
      set = addedLines(f);
      addedByHead.set(fileKey, set);
    }
    return set.has(line);
  };

  // Where each base row lands on the head side.
  interface Projected {
    row: DeltaRow;
    headKey: string | null; // null = file deleted
    line: number | null; // null = line replaced/deleted
    anchor: number;
  }
  const projected: Projected[] = base.map((row) => {
    if (row.file === "(project)" || row.file === "(external)") {
      return { row, headKey: row.fileKey, line: row.line, anchor: row.line };
    }
    const mapped = baseToHead.has(row.fileKey) ? baseToHead.get(row.fileKey)! : row.fileKey;
    if (mapped === null) return { row, headKey: null, line: null, anchor: 0 };
    const f = diffByHead.get(mapped);
    if (row.line <= 0) return { row, headKey: mapped, line: 0, anchor: 0 };
    return { row, headKey: mapped, line: mapLine(f, row.line), anchor: approxLine(f, row.line) };
  });

  const headUsed = new Array<boolean>(head.length).fill(false);
  const baseUsed = new Array<boolean>(base.length).fill(false);
  const preexisting: PreexistingRow[] = [];
  const newRows: NewRow[] = [];
  let changed = 0;

  const index = (keyFn: (h: DeltaRow) => string) => {
    const m = new Map<string, number[]>();
    head.forEach((h, i) => {
      if (headUsed[i]) return;
      const k = keyFn(h);
      const list = m.get(k);
      if (list) list.push(i);
      else m.set(k, [i]);
    });
    return m;
  };
  const take = (m: Map<string, number[]>, k: string): number | undefined => {
    const list = m.get(k);
    if (!list) return undefined;
    while (list.length > 0) {
      const i = list.shift()!;
      if (!headUsed[i]) return i;
    }
    return undefined;
  };
  const keep = (i: number, matchedBy: PreexistingRow["matchedBy"]) => {
    const h = head[i]!;
    headUsed[i] = true;
    preexisting.push({ ...h, onTouchedLine: touched(h.fileKey, h.line), matchedBy });
  };

  // Pass 1 — exact: same rule, file, message, and the base line maps onto the head line.
  {
    const m = index((h) => `${h.code}|${h.fileKey}|${h.normMessage}|${h.line}`);
    projected.forEach((p, bi) => {
      if (p.headKey === null || p.line === null) return;
      const hi = take(m, `${p.row.code}|${p.headKey}|${p.row.normMessage}|${p.line}`);
      if (hi === undefined) return;
      baseUsed[bi] = true;
      keep(hi, "exact");
    });
  }

  // Pass 2 — changed: same rule and mapped line, different message.
  {
    const m = index((h) => `${h.code}|${h.fileKey}|${h.line}`);
    projected.forEach((p, bi) => {
      if (baseUsed[bi] || p.headKey === null || p.line === null || p.line <= 0) return;
      const hi = take(m, `${p.row.code}|${p.headKey}|${p.line}`);
      if (hi === undefined) return;
      baseUsed[bi] = true;
      const h = head[hi]!;
      const onTouched = touched(h.fileKey, h.line);
      if (!onTouched && digitsOnlyChange(p.row.normMessage, h.normMessage)) {
        keep(hi, "changed-digits");
        return;
      }
      headUsed[hi] = true;
      changed++;
      newRows.push({ ...h, attribution: onTouched ? "mine" : "induced", changedFrom: p.row.message });
    });
  }

  // Pass 3 — shifted: same rule, file and message within ±window of where the base line went.
  {
    const m = index((h) => `${h.code}|${h.fileKey}|${h.normMessage}`);
    projected.forEach((p, bi) => {
      if (baseUsed[bi] || p.headKey === null || p.row.line <= 0) return;
      const list = m.get(`${p.row.code}|${p.headKey}|${p.row.normMessage}`);
      if (!list) return;
      const anchor = p.line ?? p.anchor;
      let best = -1;
      let bestDist = Infinity;
      let bestCol = Infinity;
      for (const i of list) {
        if (headUsed[i]) continue;
        const h = head[i]!;
        const dist = Math.abs(h.line - anchor);
        if (dist > window) continue;
        const col = Math.abs(h.column - p.row.column);
        if (dist < bestDist || (dist === bestDist && col < bestCol)) {
          best = i;
          bestDist = dist;
          bestCol = col;
        }
      }
      if (best < 0) return;
      baseUsed[bi] = true;
      keep(best, "shifted");
    });
  }

  head.forEach((h, i) => {
    if (headUsed[i]) return;
    newRows.push({ ...h, attribution: touched(h.fileKey, h.line) ? "mine" : "induced", changedFrom: null });
  });
  const fixed = base.filter((_, i) => !baseUsed[i]);

  return { newRows, preexisting, fixed, changed };
}

// ---------------------------------------------------------------------------
// Verdict
// ---------------------------------------------------------------------------

export type Verdict = "clean" | "new-diagnostics" | "head-errors" | "inconclusive";

export interface VerdictInput {
  baseErrors: number;
  headErrors: number;
  /** New rows after matching (already restricted to errors when a side was suppressed). */
  newRows: NewRow[];
  aicopMissing: boolean;
}

export function computeVerdict(v: VerdictInput): Verdict {
  if (v.newRows.some((r) => r.severity === "error")) return "head-errors";
  // alc runs no analyzers once a side has an error, so that side's warning
  // set is unknown — the gate cannot be decided either way.
  if (v.baseErrors > 0 || v.headErrors > 0) return "inconclusive";
  if (v.newRows.length > 0) return "new-diagnostics";
  // Without AiCop a clean result would be a false pass (AI#### never ran).
  return v.aicopMissing ? "inconclusive" : "clean";
}
