/**
 * Parse `git diff -U0` output into per-file line maps.
 *
 * Everything here is 1-based, like git's hunk headers. With -U0 a hunk
 * `@@ -a,b +c,d @@` says: base lines a..a+b-1 were replaced by head lines
 * c..c+d-1 (b = 0 is a pure insertion after base line a, d = 0 a pure
 * deletion after head line c). Lines outside every hunk are unchanged and
 * shift by the running (d - b) offset of the hunks above them.
 */

export interface Hunk {
  oldStart: number;
  oldCount: number;
  newStart: number;
  newCount: number;
}

export type FileStatus = "added" | "deleted" | "modified" | "renamed";

export interface FileDiff {
  /** Path in the base tree (null for an added file). Relative, `/` separators. */
  oldPath: string | null;
  /** Path in the head tree (null for a deleted file). */
  newPath: string | null;
  status: FileStatus;
  hunks: Hunk[];
}

export interface DiffMap {
  /** Keyed by head path (deleted files by their base path). */
  files: Map<string, FileDiff>;
}

/** Strip git's `a/` / `b/` prefix and C-style quoting from a header path. */
function headerPath(raw: string): string | null {
  let p = raw.trim();
  if (p === "/dev/null") return null;
  if (p.startsWith('"') && p.endsWith('"')) {
    p = unquoteC(p.slice(1, -1));
  }
  if (p.startsWith("a/") || p.startsWith("b/")) p = p.slice(2);
  return p;
}

/** A `rename from/to` path: quoted like a header path, but without the a/ b/ prefix. */
function plainPath(raw: string): string {
  const p = raw.trim();
  return p.startsWith('"') && p.endsWith('"') ? unquoteC(p.slice(1, -1)) : p;
}

/** Undo git's C-quoting (core.quotePath=false still quotes `"`, `\` and control chars). */
function unquoteC(s: string): string {
  const bytes: number[] = [];
  for (let i = 0; i < s.length; i++) {
    const ch = s[i]!;
    if (ch !== "\\") {
      for (const b of Buffer.from(ch, "utf8")) bytes.push(b);
      continue;
    }
    const next = s[++i];
    if (next === undefined) break;
    if (/[0-7]/.test(next)) {
      const oct = s.slice(i, i + 3);
      bytes.push(parseInt(oct, 8));
      i += 2;
      continue;
    }
    const esc: Record<string, number> = { n: 10, t: 9, r: 13, '"': 34, "\\": 92, a: 7, b: 8, f: 12, v: 11 };
    bytes.push(esc[next] ?? next.charCodeAt(0));
  }
  return Buffer.from(bytes).toString("utf8");
}

const HUNK_RE = /^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@/;

export function parseDiff(text: string): DiffMap {
  const files = new Map<string, FileDiff>();
  let cur: FileDiff | null = null;

  const flush = () => {
    if (!cur) return;
    const key = cur.newPath ?? cur.oldPath;
    if (key) files.set(key, cur);
    cur = null;
  };

  for (const rawLine of text.split("\n")) {
    const line = rawLine.endsWith("\r") ? rawLine.slice(0, -1) : rawLine;
    if (line.startsWith("diff --git ")) {
      flush();
      cur = { oldPath: null, newPath: null, status: "modified", hunks: [] };
      // Seed paths from the header for diffs that carry no ---/+++ lines
      // (binary files, pure renames, mode changes). Ambiguous when paths
      // contain " b/", which is why ---/+++ and rename lines override it.
      const m = /^diff --git (?:"?a\/)(.+?)"? (?:"?b\/)(.+?)"?$/.exec(line);
      if (m) {
        cur.oldPath = m[1]!;
        cur.newPath = m[2]!;
      }
      continue;
    }
    if (!cur) continue;
    if (line.startsWith("new file mode")) cur.status = "added";
    else if (line.startsWith("deleted file mode")) cur.status = "deleted";
    else if (line.startsWith("rename from ")) {
      cur.oldPath = plainPath(line.slice("rename from ".length));
      cur.status = "renamed";
    } else if (line.startsWith("rename to ")) {
      cur.newPath = plainPath(line.slice("rename to ".length));
      cur.status = "renamed";
    } else if (line.startsWith("--- ")) {
      cur.oldPath = headerPath(line.slice(4));
    } else if (line.startsWith("+++ ")) {
      cur.newPath = headerPath(line.slice(4));
    } else {
      const h = HUNK_RE.exec(line);
      if (h) {
        cur.hunks.push({
          oldStart: Number(h[1]),
          oldCount: h[2] === undefined ? 1 : Number(h[2]),
          newStart: Number(h[3]),
          newCount: h[4] === undefined ? 1 : Number(h[4]),
        });
      }
    }
  }
  flush();

  for (const f of files.values()) {
    if (f.status === "added") f.oldPath = null;
    if (f.status === "deleted") f.newPath = null;
    f.hunks.sort((a, b) => a.oldStart - b.oldStart);
  }
  return { files };
}

/** Head lines the change added or replaced (1-based). */
export function addedLines(f: FileDiff): Set<number> {
  const out = new Set<number>();
  for (const h of f.hunks) {
    for (let i = 0; i < h.newCount; i++) out.add(h.newStart + i);
  }
  return out;
}

/**
 * Map a 1-based base line to its head line. Returns null when the base line
 * was itself replaced or deleted (it sits inside a hunk's old range).
 */
export function mapLine(f: FileDiff | undefined, baseLine: number): number | null {
  if (!f) return baseLine;
  let offset = 0;
  for (const h of f.hunks) {
    if (h.oldCount > 0 && baseLine >= h.oldStart && baseLine < h.oldStart + h.oldCount) return null;
    // A hunk lies "above" the line when its old range ends before it. A pure
    // insertion (oldCount 0) at oldStart = n inserts AFTER base line n.
    const endsBefore = h.oldCount === 0 ? h.oldStart < baseLine : h.oldStart + h.oldCount - 1 < baseLine;
    if (!endsBefore) break;
    offset += h.newCount - h.oldCount;
  }
  return baseLine + offset;
}

/**
 * Best-guess head position for a base line that mapLine() returned null for:
 * the start of the head range that replaced it. Used only to anchor the
 * "shifted" search window, never to declare a match on its own.
 */
export function approxLine(f: FileDiff | undefined, baseLine: number): number {
  if (!f) return baseLine;
  let offset = 0;
  for (const h of f.hunks) {
    if (h.oldCount > 0 && baseLine >= h.oldStart && baseLine < h.oldStart + h.oldCount) {
      return Math.max(1, h.newStart + Math.min(baseLine - h.oldStart, Math.max(0, h.newCount - 1)));
    }
    const endsBefore = h.oldCount === 0 ? h.oldStart < baseLine : h.oldStart + h.oldCount - 1 < baseLine;
    if (!endsBefore) break;
    offset += h.newCount - h.oldCount;
  }
  return baseLine + offset;
}
