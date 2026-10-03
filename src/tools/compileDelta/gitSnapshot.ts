/**
 * Git plumbing for al_compile_delta: resolve the base, snapshot each side
 * into a sandbox folder, and produce the -U0 diff between them.
 *
 * Snapshots instead of compiling in place: no `.alcache` can exist in a
 * fresh folder (a repeat compile goes incremental and under-reports), agents
 * editing the tree mid-compile cannot produce a half-written state, and alc
 * never rewrites report layouts in the user's working tree. The base side
 * uses a throwaway index (`read-tree` + `checkout-index`), so no worktree is
 * registered and nothing has to be removed afterwards.
 */
import { execFileSync } from "node:child_process";
import { copyFileSync, existsSync, mkdirSync, mkdtempSync, readdirSync, rmSync, statSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join, relative, resolve, sep } from "node:path";

export class CompileDeltaError extends Error {
  constructor(
    readonly code:
      | "NOT_AN_AL_PROJECT"
      | "NOT_A_GIT_REPO"
      | "BASE_UNRESOLVED"
      | "ALC_NOT_FOUND"
      | "AICOP_MISSING"
      | "BASE_COMPILE_FAILED"
      | "HEAD_COMPILE_FAILED"
      | "SNAPSHOT_FAILED",
    message: string,
  ) {
    super(`${code}: ${message}`);
    this.name = "CompileDeltaError";
  }
}

const GIT_MAX_BUFFER = 512 * 1024 * 1024;

export function git(cwd: string, args: string[], env?: NodeJS.ProcessEnv): string {
  return execFileSync("git", ["-c", "core.quotepath=false", ...args], {
    cwd,
    env: env ? { ...process.env, ...env } : process.env,
    encoding: "utf8",
    maxBuffer: GIT_MAX_BUFFER,
    stdio: ["ignore", "pipe", "pipe"],
    windowsHide: true,
  });
}

function gitTry(cwd: string, args: string[]): string | undefined {
  try {
    return git(cwd, args).trim();
  } catch {
    return undefined;
  }
}

export interface RepoInfo {
  /** Absolute git top level. */
  topLevel: string;
  /** Project path relative to the top level, `/` separators, "" at the root. */
  appRel: string;
}

export function repoInfo(projectPath: string): RepoInfo {
  const top = gitTry(projectPath, ["rev-parse", "--show-toplevel"]);
  if (!top) {
    throw new CompileDeltaError("NOT_A_GIT_REPO", `${projectPath} is not inside a git repository.`);
  }
  const topLevel = resolve(top);
  const appRel = relative(topLevel, resolve(projectPath)).split(sep).join("/");
  return { topLevel, appRel };
}

export interface ResolvedBase {
  sha: string;
  /** What the caller named (baseRef, or targetBranch for a merge-base). */
  ref: string;
  /** Age in days of the targetBranch tip, when a targetBranch was used. */
  targetAgeDays?: number;
}

export function resolveBase(
  projectPath: string,
  headRev: string,
  baseRef: string | undefined,
  targetBranch: string | undefined,
): ResolvedBase {
  if (baseRef) {
    const sha = gitTry(projectPath, ["rev-parse", "--verify", "--quiet", `${baseRef}^{commit}`]);
    if (!sha) {
      throw new CompileDeltaError("BASE_UNRESOLVED", `baseRef '${baseRef}' is not a known commit. Run git fetch, or check the name.`);
    }
    return { sha, ref: baseRef };
  }
  if (!targetBranch) {
    throw new CompileDeltaError(
      "BASE_UNRESOLVED",
      "Pass baseRef or targetBranch (e.g. targetBranch: 'origin/stage'). The tool never guesses the target, because repos differ (stage / dev / main).",
    );
  }
  const tip = gitTry(projectPath, ["rev-parse", "--verify", "--quiet", `${targetBranch}^{commit}`]);
  if (!tip) {
    throw new CompileDeltaError("BASE_UNRESOLVED", `targetBranch '${targetBranch}' is not a known ref. Run git fetch, or check the name.`);
  }
  const sha = gitTry(projectPath, ["merge-base", headRev, tip]);
  if (!sha) {
    throw new CompileDeltaError("BASE_UNRESOLVED", `No merge base between ${headRev} and ${targetBranch}.`);
  }
  const ct = Number(gitTry(projectPath, ["log", "-1", "--format=%ct", tip]));
  const targetAgeDays = Number.isFinite(ct) ? (Date.now() / 1000 - ct) / 86400 : undefined;
  return { sha, ref: targetBranch, targetAgeDays };
}

export function revParse(cwd: string, rev: string): string {
  const sha = gitTry(cwd, ["rev-parse", "--verify", "--quiet", `${rev}^{commit}`]);
  if (!sha) throw new CompileDeltaError("BASE_UNRESOLVED", `head '${rev}' is not a known commit.`);
  return sha;
}

/** Root for sandboxes and reports: outside OneDrive / .claude and outside the project. */
export function deltaRoot(): string {
  const base = process.env.LOCALAPPDATA ?? tmpdir();
  return join(base, "al-mcp-bridge", "delta");
}

export function newSandbox(): string {
  const work = join(deltaRoot(), "work");
  mkdirSync(work, { recursive: true });
  sweepStaleSandboxes(work);
  return mkdtempSync(join(work, "d-"));
}

/** Remove sandboxes left by a crashed run (older than one hour). Best-effort. */
function sweepStaleSandboxes(work: string): void {
  const cutoff = Date.now() - 60 * 60 * 1000;
  for (const name of safeReadDir(work)) {
    const p = join(work, name);
    try {
      if (statSync(p).mtimeMs < cutoff) rmSync(p, { recursive: true, force: true });
    } catch {
      // In use or already gone.
    }
  }
}

export function removeSandbox(dir: string): void {
  try {
    rmSync(dir, { recursive: true, force: true });
  } catch {
    // Swept on a later run.
  }
}

/** Never copied into a snapshot: build outputs and symbol caches. */
const SKIP_DIRS = new Set([".alcache", ".alpackages", ".snapshots"]);

function skipped(rel: string): boolean {
  return rel.split("/").some((seg) => SKIP_DIRS.has(seg.toLowerCase()));
}

/**
 * Materialize the project folder of `commit` into `dest` (dest becomes the
 * project root). Uses a private index file, so the repo's own index and
 * worktree list are never touched.
 */
export function snapshotCommit(repo: RepoInfo, commit: string, dest: string): void {
  const indexFile = join(dirname(dest), `${commit.slice(0, 12)}-${Math.random().toString(36).slice(2)}.index`);
  const treeish = repo.appRel ? `${commit}:${repo.appRel}` : `${commit}^{tree}`;
  try {
    mkdirSync(dest, { recursive: true });
    const env = { GIT_INDEX_FILE: indexFile };
    git(repo.topLevel, ["read-tree", treeish], env);
    // checkout-index needs a trailing separator to treat --prefix as a folder.
    git(repo.topLevel, ["checkout-index", "-a", "-f", `--prefix=${dest.replace(/\\/g, "/")}/`], env);
  } catch (err) {
    throw new CompileDeltaError("SNAPSHOT_FAILED", `Could not materialize ${treeish}: ${(err as Error).message.split("\n")[0]}`);
  } finally {
    rmSync(indexFile, { force: true });
  }
  for (const seg of SKIP_DIRS) rmSync(join(dest, seg), { recursive: true, force: true });
}

export interface WorktreeSnapshot {
  /** Project-relative paths of untracked (not ignored) files: wholly "added". */
  untracked: string[];
  copied: number;
}

/** Copy tracked + untracked-not-ignored files of the project folder into `dest`. */
export function snapshotWorktree(projectPath: string, dest: string): WorktreeSnapshot {
  let tracked: string[];
  let untracked: string[];
  try {
    tracked = splitZ(git(projectPath, ["ls-files", "-z", "-c", "--", "."]));
    untracked = splitZ(git(projectPath, ["ls-files", "-z", "-o", "--exclude-standard", "--", "."]));
  } catch (err) {
    throw new CompileDeltaError("SNAPSHOT_FAILED", `git ls-files failed: ${(err as Error).message.split("\n")[0]}`);
  }
  let copied = 0;
  mkdirSync(dest, { recursive: true });
  for (const rel of [...tracked, ...untracked]) {
    if (skipped(rel)) continue;
    const src = join(projectPath, rel);
    // A tracked file deleted in the worktree is absent on the head side.
    if (!existsSync(src)) continue;
    const out = join(dest, rel);
    mkdirSync(dirname(out), { recursive: true });
    copyFileSync(src, out);
    copied++;
  }
  return { untracked: untracked.filter((r) => !skipped(r)), copied };
}

/** True when the project folder has uncommitted changes (tracked or untracked). */
export function isDirty(projectPath: string): boolean {
  return (gitTry(projectPath, ["status", "--porcelain", "--", "."]) ?? "") !== "";
}

/**
 * -U0 diff of the project folder, paths relative to the project. `head`
 * undefined diffs against the working tree (tracked files only; untracked
 * files are reported separately by snapshotWorktree).
 */
export function diffU0(projectPath: string, baseSha: string, head?: string): string {
  const args = ["diff", "-U0", "-M", "--ignore-cr-at-eol", "--no-color", "--no-ext-diff", "--relative", baseSha];
  if (head) args.push(head);
  args.push("--", ".");
  return git(projectPath, args);
}

/** Files that changed in the diff, relative to the project (used for ruleset hints). */
export function changedPaths(projectPath: string, baseSha: string, head?: string): string[] {
  const args = ["diff", "--name-only", "-z", "--relative", baseSha];
  if (head) args.push(head);
  args.push("--", ".");
  return splitZ(gitTry(projectPath, args) ?? "");
}

function splitZ(s: string): string[] {
  return s.split("\0").filter((x) => x.length > 0);
}

function safeReadDir(p: string): string[] {
  try {
    return readdirSync(p);
  } catch {
    return [];
  }
}
