/**
 * al_compile_delta — which diagnostics did THIS change introduce?
 *
 * Compiles the base commit and the head (working tree by default) as two
 * full, non-incremental alc runs in sandbox snapshots, with the same
 * analyzers, ruleset and symbol cache, then matches the two diagnostic sets
 * through the -U0 diff's line map. The result says what is new (mine =
 * on a line the change added/replaced, induced = elsewhere, e.g. an LC0044
 * partner), what was fixed, and what pre-existed — with an honest verdict:
 * a side whose analyzers were suppressed by errors never reports 0 warnings.
 *
 * Every line number in the result is 1-based (al_compile's are 0-based).
 */
import { createHash } from "node:crypto";
import { existsSync, mkdirSync, writeFileSync } from "node:fs";
import { basename, isAbsolute, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { z } from "zod";
import { resolveWorkspaceSettings, type BridgeConfig } from "../../config.js";
import {
  resolveAlcPath,
  resolvePackageCachePaths,
  runAlcCompile,
  type AlcCompileRun,
  type CompileDiagnostic,
} from "../compile.js";
import { BaseCache, MATCHER_SCHEMA_VERSION, computeKey, type CachedBase } from "./cache.js";
import { parseDiff } from "./diffMap.js";
import {
  CompileDeltaError,
  changedPaths,
  deltaRoot,
  diffU0,
  isDirty,
  newSandbox,
  removeSandbox,
  repoInfo,
  resolveBase,
  revParse,
  snapshotCommit,
  snapshotWorktree,
} from "./gitSnapshot.js";
import {
  computeVerdict,
  matchDiagnostics,
  type DeltaRow,
  type NewRow,
  type PreexistingRow,
  type Verdict,
} from "./match.js";

export { CompileDeltaError } from "./gitSnapshot.js";

export const DEFAULT_MAX_INLINE = 25;
export const DEFAULT_WINDOW = 3;
const AICOP_DLL = "socitas.aicop.dll";

export const CompileDeltaInput = z.object({
  projectPath: z.string().describe("AL project folder (contains app.json). Required — the tool never guesses."),
  baseRef: z
    .string()
    .optional()
    .describe("Base commit-ish. Wins over targetBranch."),
  targetBranch: z
    .string()
    .optional()
    .describe("Target branch, e.g. 'origin/stage'. Base = git merge-base <head> <targetBranch>. One of baseRef/targetBranch is required."),
  head: z
    .string()
    .default("worktree")
    .describe("'worktree' (default: tracked + untracked-not-ignored files incl. uncommitted edits) or a commit-ish."),
  scope: z
    .enum(["touched-files", "all"])
    .default("touched-files")
    .describe("Limits only the PRE-EXISTING listing in the full report. New diagnostics are always project-wide."),
  includeInfo: z.boolean().default(true).describe("Count info-level diagnostics (the gate does)."),
  includeHint: z.boolean().default(false).describe("Count hint-level diagnostics."),
  maxInline: z
    .number()
    .int()
    .min(1)
    .optional()
    .describe(`Cap on inline rows per list. Default ${DEFAULT_MAX_INLINE}, or AL_BRIDGE_DELTA_MAX_INLINE.`),
  packageCachePath: z.string().optional().describe("Symbol cache override (same precedence as al_compile)."),
  analyzers: z.array(z.string()).optional().describe("Analyzer DLL override (same semantics as al_compile)."),
  ruleSet: z.string().optional().describe("Ruleset override (same semantics as al_compile)."),
  useBaseCache: z
    .boolean()
    .default(true)
    .describe("Reuse the cached base result (keyed on base sha, ruleset, analyzers, symbol contents, alc). false recompiles the base and refreshes the entry."),
});

/** Per-call hooks the MCP layer supplies: cancellation and progress text. */
export interface ToolContext {
  signal?: AbortSignal;
  progress?: (message: string) => void;
}

export type CompileDeltaInputT = z.infer<typeof CompileDeltaInput>;

type Counts = { error: number; warning: number | null; info: number | null; hint: number | null };

export interface InlineRow {
  severity: CompileDiagnostic["severity"];
  code: string;
  file: string;
  line: number;
  column: number;
  endLine: number;
  endColumn: number;
  message: string;
  attribution?: NewRow["attribution"];
  changedFrom?: string | null;
  tag?: "new" | "environmental";
}

export interface CompileDeltaResult {
  verdict: Verdict;
  lineBase: 1;
  base: { ref: string; sha: string; cached: boolean; counts: Counts; analyzersSuppressed: boolean };
  head: { ref: string; sha: string; dirty: boolean; counts: Counts; analyzersSuppressed: boolean };
  delta: {
    new: number;
    newMine: number;
    newInduced: number;
    changed: number;
    fixed: number;
    preexistingOnTouchedLines: number;
    preexistingInTouchedFiles: number;
    environmental: number;
  };
  new: InlineRow[];
  onTouchedLines: InlineRow[];
  errors?: InlineRow[];
  analyzers: { loaded: string[]; aicop: "repo" | "env" | "override" | "missing" };
  environment: { hints: string[] };
  truncated: { newTotal: number; newShown: number; onTouchedLinesTotal: number; onTouchedLinesShown: number };
  fullReportPath?: string;
  timingsMs: { snapshot: number; baseCompile: number; headCompile: number; match: number };
  message: string;
}

export function resolveMaxInline(explicit: number | undefined, env: NodeJS.ProcessEnv = process.env): number {
  if (typeof explicit === "number" && Number.isInteger(explicit) && explicit >= 1) return explicit;
  const n = Number(env.AL_BRIDGE_DELTA_MAX_INLINE?.trim());
  return Number.isInteger(n) && n >= 1 ? n : DEFAULT_MAX_INLINE;
}

// ---------------------------------------------------------------------------
// Normalization (exported for unit tests)
// ---------------------------------------------------------------------------

const caseInsensitiveFs = process.platform === "win32";
export const keyOf = (p: string): string => (caseInsensitiveFs ? p.toLowerCase() : p);

/** Turn one alc diagnostic (0-based, sandbox URIs) into a 1-based, project-relative row. */
export function normalizeDiagnostic(d: CompileDiagnostic, sandboxRoot: string): DeltaRow {
  const root = resolve(sandboxRoot);
  let file = "(project)";
  if (d.file) {
    let abs = d.file;
    if (abs.startsWith("file:")) {
      try {
        abs = fileURLToPath(abs);
      } catch {
        // Keep the raw string.
      }
    }
    const rel = isAbsolute(abs) ? relative(root, resolve(abs)) : abs;
    file = rel.startsWith("..") || isAbsolute(rel) ? "(external)" : rel.split(sep).join("/");
  }
  const toOne = (n: number | undefined) => (typeof n === "number" ? n + 1 : 0);
  return {
    severity: d.severity,
    code: d.code ?? "(none)",
    file,
    fileKey: keyOf(file),
    line: toOne(d.startLine),
    column: toOne(d.startChar),
    endLine: toOne(d.endLine),
    endColumn: toOne(d.endChar),
    message: d.message,
    normMessage: normalizeMessage(d.message, root),
  };
}

/** Replace the sandbox root (any slash style, any case) with <root>; collapse whitespace. */
export function normalizeMessage(message: string, sandboxRoot: string): string {
  const variants = new Set([sandboxRoot, sandboxRoot.replace(/\\/g, "/"), sandboxRoot.replace(/\//g, "\\")]);
  let out = message;
  for (const v of variants) {
    if (!v) continue;
    out = out.replace(new RegExp(v.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"), "gi"), "<root>");
  }
  return out.replace(/\s+/g, " ").trim();
}

function countRows(rows: DeltaRow[], suppressed: boolean): Counts {
  const c = { error: 0, warning: 0, info: 0, hint: 0 };
  for (const r of rows) {
    if (r.severity === "error") c.error++;
    else if (r.severity === "warning") c.warning++;
    else if (r.severity === "info") c.info++;
    else if (r.severity === "hint") c.hint++;
  }
  return suppressed ? { error: c.error, warning: null, info: null, hint: null } : c;
}

/** Inline messages are capped (AiCop messages run ~1 KB each); the full text is in the report. */
export const INLINE_MESSAGE_CHARS = 240;

function clip(s: string): string {
  return s.length <= INLINE_MESSAGE_CHARS ? s : s.slice(0, INLINE_MESSAGE_CHARS - 1) + "…";
}

function inline(r: DeltaRow & Partial<NewRow>, extra?: Partial<InlineRow>): InlineRow {
  return {
    severity: r.severity,
    code: r.code,
    file: r.file,
    line: r.line,
    column: r.column,
    endLine: r.endLine,
    endColumn: r.endColumn,
    message: clip(r.message),
    ...(r.attribution ? { attribution: r.attribution } : {}),
    ...(r.changedFrom ? { changedFrom: clip(r.changedFrom) } : {}),
    ...extra,
  };
}

const SEV_RANK: Record<string, number> = { error: 0, warning: 1, info: 2, hint: 3, unknown: 4 };

/** Errors first, then mine before induced, then file/line. */
export function orderNew(rows: NewRow[]): NewRow[] {
  return [...rows].sort(
    (a, b) =>
      (SEV_RANK[a.severity]! > 0 ? 1 : 0) - (SEV_RANK[b.severity]! > 0 ? 1 : 0) ||
      (a.attribution === "mine" ? 0 : 1) - (b.attribution === "mine" ? 0 : 1) ||
      a.file.localeCompare(b.file) ||
      a.line - b.line,
  );
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

export function createCompileDelta(config: BridgeConfig) {
  const alcPath = resolveAlcPath(config.languageServerPath);
  const baseCache = new BaseCache(join(deltaRoot(), "cache"), { lockStaleMs: config.timeouts.compileMs + 60_000 });
  return async (input: CompileDeltaInputT, ctx: ToolContext = {}): Promise<CompileDeltaResult> => {
    const progress = (message: string) => ctx.progress?.(message);
    if (!existsSync(alcPath)) {
      throw new CompileDeltaError("ALC_NOT_FOUND", `AL compiler not found at ${alcPath}. Point AL_LS_PATH at the EditorServices host that ships alongside alc.`);
    }
    const projectPath = resolve(input.projectPath);
    if (!existsSync(join(projectPath, "app.json"))) {
      throw new CompileDeltaError("NOT_AN_AL_PROJECT", `No app.json at ${projectPath}.`);
    }
    const repo = repoInfo(projectPath);
    const worktreeHead = input.head === "worktree";
    const headRev = worktreeHead ? "HEAD" : input.head;
    const headSha = revParse(projectPath, headRev);
    const base = resolveBase(projectPath, headRev, input.baseRef, input.targetBranch);

    // Settings come from the REAL project, so both sides compile with the
    // same analyzers, ruleset (absolute path) and symbol cache.
    const settings =
      config.workspaceSettings.get(projectPath) ?? resolveWorkspaceSettings(projectPath, config.languageServerPath);
    const analyzers = input.analyzers ?? settings.codeAnalyzers;
    const ruleSet = input.ruleSet ?? settings.ruleSetPath;
    const packageCachePaths = resolvePackageCachePaths(input.packageCachePath, config.packageCachePaths, projectPath);
    const aicop = classifyAicop(analyzers, input.analyzers !== undefined);
    if (aicop === "missing" && process.env.AL_BRIDGE_DELTA_REQUIRE_AICOP === "1") {
      throw new CompileDeltaError("AICOP_MISSING", "Socitas.AiCop.dll is not among the resolved analyzers (AL_BRIDGE_DELTA_REQUIRE_AICOP=1).");
    }

    const hints: string[] = [];
    if (base.targetAgeDays !== undefined && base.targetAgeDays > 7) {
      hints.push(`${base.ref} tip is ${Math.floor(base.targetAgeDays)} days old — git fetch may be due.`);
    }
    const changed = changedPaths(projectPath, base.sha, worktreeHead ? undefined : headSha);
    if (changed.some((p) => /(^|\/)\.vscode\/settings\.json$/i.test(p) || /ruleset.*\.json$/i.test(p))) {
      hints.push("ruleset or .vscode/settings.json changed in the diff — the head configuration was used for both sides.");
    }

    const sandbox = newSandbox();
    try {
      const t0 = Date.now();
      const baseDir = join(sandbox, "base");
      const headDir = join(sandbox, "head");
      progress("snapshot");
      let untracked: string[] = [];
      if (worktreeHead) {
        untracked = snapshotWorktree(projectPath, headDir).untracked;
      } else {
        snapshotCommit(repo, headSha, headDir);
      }
      // Diff right after the head snapshot, so both describe the same moment.
      const diff = parseDiff(diffU0(projectPath, base.sha, worktreeHead ? undefined : headSha));
      const tSnap = Date.now() - t0;

      const compileSide = async (dir: string, side: "base" | "head") => {
        progress(`${side} compile`);
        const heartbeat = setInterval(() => progress(`${side} compile (still running)`), 15_000);
        let run: AlcCompileRun;
        try {
          run = await runAlcCompile({
            alcPath,
            projectPath: dir,
            packageCachePaths,
            assemblyProbingPaths: config.assemblyProbingPaths,
            analyzers,
            ruleSet,
            enableExternalRulesets: true,
            generateCode: false,
            continueOnError: true,
            timeoutMs: config.timeouts.compileMs,
            maxOutputChars: 64 * 1024,
            signal: ctx.signal,
            onWait: (m) => progress(`${side}: ${m}`),
          });
        } finally {
          clearInterval(heartbeat);
        }
        if (!run.errorLogWritten) {
          throw new CompileDeltaError(
            side === "base" ? "BASE_COMPILE_FAILED" : "HEAD_COMPILE_FAILED",
            `alc exited ${run.exitCode} without an errorlog. stderr: ${tailOf(run.stderr) || "(empty)"} stdout: ${tailOf(run.stdout) || "(empty)"}`,
          );
        }
        return run;
      };

      // Base: from the cache when nothing that shapes its diagnostics changed.
      // Sequential with the head on purpose: two GC-sized alc runs at once is
      // how the 16 GB machine starts paging.
      const t1 = Date.now();
      const computeBase = async (key: string, remoteRuleset: boolean): Promise<CachedBase> => {
        snapshotCommit(repo, base.sha, baseDir);
        const run = await compileSide(baseDir, "base");
        return { schema: MATCHER_SCHEMA_VERSION, key, baseSha: base.sha, createdAt: Date.now(), remoteRuleset, rows: rowsOf(run, baseDir) };
      };
      const ck = await computeKey({
        baseSha: base.sha,
        appRel: repo.appRel,
        ruleSet,
        analyzers: analyzers ?? [],
        packageCachePaths,
        alcPath,
        assemblyProbingPaths: config.assemblyProbingPaths,
      });
      let baseRows: DeltaRow[];
      let baseCached = false;
      // Only an explicit false bypasses: a direct (non-MCP) caller skips zod's default.
      if (input.useBaseCache !== false) {
        const got = await baseCache.getOrCompute(ck.key, () => computeBase(ck.key, ck.remoteRuleset), ctx.signal, progress);
        baseRows = got.entry.rows;
        baseCached = got.cached;
      } else {
        // Bypass the read, but refresh the entry for the next call.
        const entry = await computeBase(ck.key, ck.remoteRuleset);
        baseCache.write(entry);
        baseRows = entry.rows;
      }
      if (baseCached) progress("base from cache");
      const tBase = Date.now() - t1;
      const t2 = Date.now();
      const headRun = await compileSide(headDir, "head");
      const tHead = Date.now() - t2;

      progress("match");
      const t3 = Date.now();
      const keepSev = (r: DeltaRow) =>
        r.severity === "error" ||
        r.severity === "warning" ||
        (r.severity === "info" && input.includeInfo) ||
        (r.severity === "hint" && input.includeHint);
      const baseAll = baseRows.filter(keepSev);
      const headAll = rowsOf(headRun, headDir).filter(keepSev);
      const baseErrors = baseAll.filter((r) => r.severity === "error").length;
      const headErrors = headAll.filter((r) => r.severity === "error").length;
      const baseSuppressed = baseErrors > 0;
      const headSuppressed = headErrors > 0;
      // With either side suppressed only errors are comparable: the other
      // side's warnings would all read as new (or fixed).
      const onlyErrors = baseSuppressed || headSuppressed;
      const comparable = (rows: DeltaRow[]) => (onlyErrors ? rows.filter((r) => r.severity === "error") : rows);

      const wholeAdded = new Set<string>(untracked.map((p) => keyOf(p.split(sep).join("/"))));
      for (const f of diff.files.values()) {
        if (f.status === "added" && f.newPath) wholeAdded.add(keyOf(f.newPath));
      }
      const m = matchDiagnostics({
        base: comparable(baseAll),
        head: comparable(headAll),
        diff,
        wholeAddedFiles: wholeAdded,
        window: deltaWindow(),
        keyOf,
      });
      const environmental = m.preexisting.filter((r) => r.severity === "error");
      const verdict = computeVerdict({ baseErrors, headErrors, newRows: m.newRows, aicopMissing: aicop === "missing" });

      const touchedFiles = new Set<string>([...wholeAdded]);
      for (const f of diff.files.values()) if (f.newPath) touchedFiles.add(keyOf(f.newPath));
      const onTouched = m.preexisting.filter((r) => r.onTouchedLine);
      const inTouchedFiles = m.preexisting.filter((r) => touchedFiles.has(r.fileKey));
      const newOrdered = orderNew(m.newRows);
      const tMatch = Date.now() - t3;

      if (onlyErrors && environmental.length > 0 && m.newRows.length === 0) {
        hints.push(
          `${environmental.length} error(s) exist on both sides — most likely the local symbol cache, not the change. ` +
            "Fix the cache (profile-trto:al-symbols) before trusting a gate result here.",
        );
      }
      if (aicop === "missing") hints.push("Socitas.AiCop.dll is not among the analyzers — AI#### rules did not run, so the verdict cannot be 'clean'.");

      const maxInline = resolveMaxInline(input.maxInline);
      const headCounts = countRows(headAll, headSuppressed);
      const baseCounts = countRows(baseAll, baseSuppressed);
      const mine = m.newRows.filter((r) => r.attribution === "mine").length;

      const fullReportPath = writeReport(projectPath, {
        projectPath,
        generatedAt: new Date().toISOString(),
        verdict,
        lineBase: 1,
        base: { ref: base.ref, sha: base.sha, counts: baseCounts },
        head: { ref: worktreeHead ? "worktree" : input.head, sha: headSha, counts: headCounts },
        alc: { analyzers, ruleSet, packageCachePaths },
        rows: [
          ...newOrdered.map((r) => ({ class: "new", ...publicRow(r), attribution: r.attribution, changedFrom: r.changedFrom })),
          ...m.preexisting
            .filter((r) => input.scope === "all" || touchedFiles.has(r.fileKey))
            .map((r) => ({ class: r.severity === "error" ? "environmental" : "preexisting", ...publicRow(r), onTouchedLine: r.onTouchedLine, matchedBy: r.matchedBy })),
          ...m.fixed.map((r) => ({ class: "fixed", ...publicRow(r) })),
        ],
        diff: [...diff.files.values()].map((f) => ({ oldPath: f.oldPath, newPath: f.newPath, status: f.status, hunks: f.hunks })),
        untracked,
      });

      const result: CompileDeltaResult = {
        verdict,
        lineBase: 1,
        base: { ref: base.ref, sha: base.sha, cached: baseCached, counts: baseCounts, analyzersSuppressed: baseSuppressed },
        head: {
          ref: worktreeHead ? "worktree" : input.head,
          sha: headSha,
          dirty: worktreeHead ? isDirty(projectPath) : false,
          counts: headCounts,
          analyzersSuppressed: headSuppressed,
        },
        delta: {
          new: m.newRows.length,
          newMine: mine,
          newInduced: m.newRows.length - mine,
          changed: m.changed,
          fixed: m.fixed.length,
          preexistingOnTouchedLines: onTouched.length,
          preexistingInTouchedFiles: inTouchedFiles.length,
          environmental: environmental.length,
        },
        new: newOrdered.slice(0, maxInline).map((r) => inline(r)),
        onTouchedLines: onTouched.slice(0, maxInline).map((r) => inline(r)),
        ...(headSuppressed
          ? {
              errors: [
                ...newOrdered.filter((r) => r.severity === "error").map((r) => inline(r, { tag: "new" })),
                ...environmental.map((r) => inline(r, { tag: "environmental" })),
              ].slice(0, maxInline),
            }
          : {}),
        analyzers: { loaded: loadedAnalyzers(analyzers), aicop },
        environment: { hints },
        truncated: {
          newTotal: m.newRows.length,
          newShown: Math.min(m.newRows.length, maxInline),
          onTouchedLinesTotal: onTouched.length,
          onTouchedLinesShown: Math.min(onTouched.length, maxInline),
        },
        ...(fullReportPath ? { fullReportPath } : {}),
        timingsMs: { snapshot: tSnap, baseCompile: tBase, headCompile: tHead, match: tMatch },
        message: summarize(verdict, m.newRows, mine, onTouched.length, environmental.length, fullReportPath) + (baseCached ? " Base from cache." : ""),
      };
      return result;
    } finally {
      removeSandbox(sandbox);
    }
  };
}

function rowsOf(run: AlcCompileRun, dir: string): DeltaRow[] {
  return run.diagnostics.map((d) => normalizeDiagnostic(d, dir));
}

function publicRow(r: DeltaRow) {
  const { fileKey: _k, normMessage: _n, ...rest } = r;
  return rest;
}

function deltaWindow(): number {
  const n = Number(process.env.AL_BRIDGE_DELTA_WINDOW?.trim());
  return Number.isInteger(n) && n >= 0 ? n : DEFAULT_WINDOW;
}

export function classifyAicop(analyzers: string[] | undefined, override: boolean): "repo" | "env" | "override" | "missing" {
  const hit = (analyzers ?? []).find((a) => basename(a).toLowerCase() === AICOP_DLL);
  if (!hit || !existsSync(hit)) return "missing";
  if (override) return "override";
  const extra = (process.env.AL_EXTRA_CODE_ANALYZERS ?? "").toLowerCase();
  return extra.includes(AICOP_DLL) ? "env" : "repo";
}

function loadedAnalyzers(analyzers: string[] | undefined): string[] {
  return (analyzers ?? [])
    .filter((a) => existsSync(a))
    .map((a) => basename(a).replace(/\.dll$/i, ""))
    .map((n) => n.replace(/^Microsoft\.Dynamics\.Nav\./i, "").replace(/^BusinessCentral\./i, ""));
}

function summarize(
  verdict: Verdict,
  newRows: NewRow[],
  mine: number,
  onTouched: number,
  environmental: number,
  reportPath: string | undefined,
): string {
  const byCode = new Map<string, number>();
  for (const r of newRows) byCode.set(r.code, (byCode.get(r.code) ?? 0) + 1);
  const codes = [...byCode.entries()]
    .sort((a, b) => b[1] - a[1])
    .slice(0, 6)
    .map(([c, n]) => `${c} x${n}`)
    .join(", ");
  const head =
    verdict === "clean"
      ? "No new diagnostics."
      : verdict === "inconclusive"
        ? "Inconclusive: analyzers did not run on both sides (or AiCop is missing) — see environment.hints."
        : `${newRows.length} new (${mine} mine, ${newRows.length - mine} induced)${codes ? ` — ${codes}` : ""}.`;
  const tail = [
    onTouched > 0 ? `${onTouched} pre-existing on touched lines (onTouchedLines).` : "",
    environmental > 0 ? `${environmental} error(s) on both sides (environmental).` : "",
    reportPath ? `Full report: ${reportPath}` : "Full report could not be written.",
  ]
    .filter(Boolean)
    .join(" ");
  return `${head} ${tail}`.trim();
}

function writeReport(projectPath: string, body: { rows: unknown[] } & Record<string, unknown>): string | undefined {
  const dir = join(deltaRoot(), "reports");
  const hash = createHash("sha1").update(projectPath.toLowerCase()).digest("hex").slice(0, 8);
  const file = join(dir, `${hash}.compile-delta.json`);
  const { rows, ...header } = body;
  // `{...header..., "rows": [\n one per line \n]}` — greppable in slices, like al_compile's full list.
  const headerText = JSON.stringify(header);
  const lines = rows.map((r) => "  " + JSON.stringify(r));
  const text = `${headerText.slice(0, -1)},"rows":[\n${lines.join(",\n")}\n]}\n`;
  try {
    mkdirSync(dir, { recursive: true });
    writeFileSync(file, text, "utf8");
    return resolve(file);
  } catch (err) {
    process.stderr.write(`[al-mcp-bridge] failed to write compile-delta report at ${file}: ${(err as Error).message}\n`);
    return undefined;
  }
}

function tailOf(s: string): string {
  return s.length <= 1500 ? s.trim() : "…" + s.slice(-1500).trim();
}
