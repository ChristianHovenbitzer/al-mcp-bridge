/**
 * al_compile — build an AL project to a .app package on Linux.
 *
 * Reimplements what Microsoft's `al_build` tool does, but by directly
 * invoking the `alc` compiler binary that ships next to the AL language
 * server in the VS Code extension. MS's own MCP server is currently
 * broken on Linux; `alc` itself works fine.
 *
 * Diagnostics come from alc's `/errorlog:<file>` switch, which writes a
 * stable SARIF-like JSON document. We parse that rather than scraping
 * console output — stdout only carries a one-line summary per issue and
 * truncates location ranges.
 *
 * Defaults are sourced from the bridge's resolved `BridgeConfig`
 * (analyzers, package cache, rule set), so a compile on a real project
 * mirrors what the LSP sees for diagnostics. Callers can override any
 * of them per-invocation.
 */
import { spawn } from "node:child_process";
import { existsSync, mkdtempSync, readFileSync, readdirSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { basename, dirname, isAbsolute, join, resolve } from "node:path";
import { z } from "zod";
import type { BridgeConfig } from "../config.js";

// ---------------------------------------------------------------------------
// MCP-facing input schema
// ---------------------------------------------------------------------------

export const CompileInput = z.object({
  projectPath: z
    .string()
    .optional()
    .describe(
      "AL project folder containing app.json. Defaults to the bridge's primary workspace.",
    ),
  outputPath: z
    .string()
    .optional()
    .describe(
      "Absolute path for the produced .app file. If omitted, alc writes `<Publisher>_<Name>_<Version>.app` into the project folder.",
    ),
  packageCachePath: z
    .string()
    .optional()
    .describe(
      "Override symbol cache directory. Resolution order when omitted: AL_PACKAGE_CACHE env / al.packageCachePaths → `<projectPath>/.alpackages` if it exists.",
    ),
  analyzers: z
    .array(z.string())
    .optional()
    .describe(
      "Override analyzer DLLs. Defaults to the bridge's resolved `al.codeAnalyzers` from .vscode/settings.json plus AL_EXTRA_CODE_ANALYZERS.",
    ),
  ruleSet: z
    .string()
    .optional()
    .describe(
      "Override ruleset .json path. Defaults to the project's `al.ruleSetPath`.",
    ),
  enableExternalRulesets: z
    .boolean()
    .default(true)
    .describe(
      "Pass /enableexternalrulesets to alc so rulesets whose paths sit outside the project folder (e.g. a shared company-wide .ruleset.json under /home/<user>/shared-rules/) are honored instead of blocked with the BlockedExternalRulesets error. Default true — set false to match alc's stricter project-local default.",
    ),
  generateCode: z
    .boolean()
    .default(true)
    .describe("If false, passes /generatecode- to alc — no .app file is written, diagnostics only."),
  warningsAsErrors: z
    .boolean()
    .default(false)
    .describe("Passes /warnaserror+ to alc when true."),
  continueOnError: z
    .boolean()
    .default(false)
    .describe("Passes /continuebuildonerror+ so alc keeps emitting diagnostics after the first error."),
});

export type CompileInputT = z.infer<typeof CompileInput>;

// ---------------------------------------------------------------------------
// Result shape
// ---------------------------------------------------------------------------

export interface CompileDiagnostic {
  severity: "error" | "warning" | "info" | "hint" | "unknown";
  code?: string;
  file?: string;
  startLine?: number;
  startChar?: number;
  endLine?: number;
  endChar?: number;
  message: string;
  category?: string;
}

export interface CompileResult {
  succeeded: boolean;
  exitCode: number;
  alcPath: string;
  projectPath: string;
  appPath?: string;
  diagnostics: CompileDiagnostic[];
  counts: { error: number; warning: number; info: number; hint: number };
  stdoutTail: string;
  stderrTail: string;
  message: string;
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

class CompileError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "CompileError";
  }
}

export function createCompile(config: BridgeConfig) {
  const alcPath = resolveAlcPath(config.languageServerPath);
  return async (input: CompileInputT): Promise<CompileResult> => {
    if (!existsSync(alcPath)) {
      throw new CompileError(
        `AL compiler not found at ${alcPath}. The bridge derives this from AL_LS_PATH; point AL_LS_PATH at the EditorServices host that ships alongside alc.`,
      );
    }

    const projectPath = resolve(input.projectPath ?? config.workspaceRoot);
    if (!existsSync(join(projectPath, "app.json"))) {
      throw new CompileError(`No app.json at ${projectPath}; not an AL project.`);
    }

    const analyzers = input.analyzers ?? config.codeAnalyzers;
    // Precedence for the symbol cache:
    //   1. explicit input.packageCachePath (caller override)
    //   2. bridge config (AL_PACKAGE_CACHE env or al.packageCachePaths)
    //   3. convention: <projectPath>/.alpackages if present
    //   4. undefined — alc emits AL1021 "The package cache path has not been specified"
    // The convention is the default the AL extension itself uses.
    const packageCachePaths = resolvePackageCachePaths(input.packageCachePath, config.packageCachePaths, projectPath);
    const ruleSet = input.ruleSet ?? config.ruleSetPath;
    // /assemblyprobingpaths is for resolving .NET assemblies referenced from
    // AL code (via 'using' directives / DotNet type declarations). It does NOT
    // affect how alc resolves dependencies of Roslyn analyzer DLLs — those are
    // handled by Roslyn's AssemblyLoadContext, which is why the bridge instead
    // prepends common helper DLLs (Analyzers.Common, ALCops.Common, …) as
    // explicit /analyzer: entries via augmentWithAnalyzerSiblings in config.ts.
    const probingPaths = config.assemblyProbingPaths;

    const tmpDir = mkdtempSync(join(tmpdir(), "al-compile-"));
    const errorLogPath = join(tmpDir, "errors.json");

    const args: string[] = [`/project:${projectPath}`, `/errorlog:${errorLogPath}`];
    if (input.outputPath) {
      args.push(`/out:${resolve(input.outputPath)}`);
    }
    for (const p of packageCachePaths) {
      args.push(`/packagecachepath:${p}`);
    }
    for (const p of probingPaths) {
      args.push(`/assemblyprobingpaths:${p}`);
    }
    if (analyzers && analyzers.length > 0) {
      // alc accepts one /analyzer:<path> per DLL.
      for (const a of analyzers) args.push(`/analyzer:${a}`);
    }
    if (ruleSet) {
      args.push(`/ruleset:${ruleSet}`);
    }
    if (input.enableExternalRulesets) {
      // Presence switch — alc treats the flag itself as opt-in.
      args.push("/enableexternalrulesets");
    }
    if (input.generateCode === false) {
      args.push("/generatecode-");
    }
    if (input.warningsAsErrors) {
      args.push("/warnaserror+");
    }
    if (input.continueOnError) {
      args.push("/continuebuildonerror+");
    }

    let stdout = "";
    let stderr = "";
    let exitCode = -1;
    try {
      const res = await runAlc(alcPath, args);
      stdout = res.stdout;
      stderr = res.stderr;
      exitCode = res.exitCode;
    } finally {
      // We still need the errorlog after spawn finishes, so read before
      // cleanup. If alc crashed we may not have one.
    }

    const diagnostics = existsSync(errorLogPath) ? parseErrorLog(readFileSync(errorLogPath, "utf8")) : [];
    try {
      rmSync(tmpDir, { recursive: true, force: true });
    } catch {
      // best-effort cleanup — tmpdir entries expire on reboot anyway
    }

    const counts = countBySeverity(diagnostics);
    const appPath = input.generateCode === false ? undefined : locateAppOutput(projectPath, input.outputPath);

    const succeeded = exitCode === 0 && counts.error === 0;
    const message = succeeded
      ? `Compilation succeeded (${counts.warning} warnings, ${counts.info} info).${appPath ? ` Output: ${appPath}` : ""}`
      : `Compilation failed with ${counts.error} error(s), ${counts.warning} warning(s) (alc exit ${exitCode}).`;

    return {
      succeeded,
      exitCode,
      alcPath,
      projectPath,
      appPath,
      diagnostics,
      counts,
      stdoutTail: tail(stdout, 4000),
      stderrTail: tail(stderr, 4000),
      message,
    };
  };
}

// ---------------------------------------------------------------------------
// Package cache resolution
// ---------------------------------------------------------------------------

/**
 * Resolve the symbol cache directories to pass to alc. Follows the same
 * order of precedence the AL extension uses, then falls back to the
 * `<projectPath>/.alpackages` convention (which is what `AL: Download
 * symbols` populates and what the AL extension defaults to).
 */
function resolvePackageCachePaths(
  override: string | undefined,
  configured: string[],
  projectPath: string,
): string[] {
  if (override) return [resolve(override)];
  if (configured.length > 0) return configured;
  const conventional = join(projectPath, ".alpackages");
  return existsSync(conventional) ? [conventional] : [];
}

// ---------------------------------------------------------------------------
// alc location
// ---------------------------------------------------------------------------

function resolveAlcPath(languageServerPath: string): string {
  const dir = dirname(languageServerPath);
  // Linux: `alc`; Windows: `alc.exe`. The EditorServices host folder
  // always carries both the host and alc at the same level.
  const candidates = [join(dir, "alc"), join(dir, "alc.exe")];
  for (const c of candidates) {
    if (existsSync(c)) return c;
  }
  // Return the Linux form as a best guess so the downstream error names
  // the path we actually tried.
  return candidates[0]!;
}

// ---------------------------------------------------------------------------
// Subprocess
// ---------------------------------------------------------------------------

interface AlcRunResult {
  exitCode: number;
  stdout: string;
  stderr: string;
}

function runAlc(alcPath: string, args: string[]): Promise<AlcRunResult> {
  return new Promise((resolvePromise, rejectPromise) => {
    const child = spawn(alcPath, args, { stdio: ["ignore", "pipe", "pipe"] });
    let stdout = "";
    let stderr = "";
    child.stdout.on("data", (b) => (stdout += b.toString("utf8")));
    child.stderr.on("data", (b) => (stderr += b.toString("utf8")));
    child.on("error", rejectPromise);
    child.on("close", (code) => {
      resolvePromise({ exitCode: code ?? -1, stdout, stderr });
    });
  });
}

// ---------------------------------------------------------------------------
// SARIF-ish error log parsing
// ---------------------------------------------------------------------------

interface SarifIssue {
  ruleId?: string;
  fullMessage?: string;
  shortMessage?: string;
  locations?: Array<{
    analysisTarget?: Array<{
      uri?: string;
      region?: {
        startLine?: number;
        startColumn?: number;
        endLine?: number;
        endColumn?: number;
      };
    }>;
  }>;
  properties?: {
    severity?: string;
    defaultSeverity?: string;
    category?: string;
  };
}

function parseErrorLog(raw: string): CompileDiagnostic[] {
  let parsed: unknown;
  try {
    parsed = JSON.parse(raw);
  } catch {
    return [];
  }
  const issues = (parsed as { issues?: SarifIssue[] })?.issues;
  if (!Array.isArray(issues)) return [];
  const out: CompileDiagnostic[] = [];
  for (const issue of issues) {
    const severity = mapSeverity(issue.properties?.severity ?? issue.properties?.defaultSeverity);
    const message = issue.fullMessage ?? issue.shortMessage ?? "(no message)";
    const loc = issue.locations?.[0]?.analysisTarget?.[0];
    const region = loc?.region;
    out.push({
      severity,
      code: issue.ruleId,
      file: loc?.uri,
      // SARIF regions are 1-based; normalize to 0-based to match LSP/VS Code.
      startLine: typeof region?.startLine === "number" ? Math.max(0, region.startLine - 1) : undefined,
      startChar: typeof region?.startColumn === "number" ? Math.max(0, region.startColumn - 1) : undefined,
      endLine: typeof region?.endLine === "number" ? Math.max(0, region.endLine - 1) : undefined,
      endChar: typeof region?.endColumn === "number" ? Math.max(0, region.endColumn - 1) : undefined,
      message,
      category: issue.properties?.category,
    });
  }
  return out;
}

function mapSeverity(raw: string | undefined): CompileDiagnostic["severity"] {
  switch ((raw ?? "").toLowerCase()) {
    case "error":
      return "error";
    case "warning":
      return "warning";
    case "info":
    case "informational":
      return "info";
    case "hidden":
    case "hint":
      return "hint";
    default:
      return "unknown";
  }
}

function countBySeverity(diags: CompileDiagnostic[]): CompileResult["counts"] {
  const counts = { error: 0, warning: 0, info: 0, hint: 0 };
  for (const d of diags) {
    if (d.severity === "error") counts.error++;
    else if (d.severity === "warning") counts.warning++;
    else if (d.severity === "info") counts.info++;
    else if (d.severity === "hint") counts.hint++;
  }
  return counts;
}

// ---------------------------------------------------------------------------
// Output path resolution
// ---------------------------------------------------------------------------

/**
 * alc's default output is `<Publisher>_<Name>_<Version>.app` written into
 * the working directory at spawn time — we spawn with the project as CWD
 * effectively, but the process actually writes to the current working
 * directory of the Node process unless /outfolder or /out is supplied.
 * We always pass either explicit /out or let alc default; for the default
 * case we locate the newest .app that matches the publisher/name/version
 * in the project folder.
 */
function locateAppOutput(projectPath: string, explicitOut: string | undefined): string | undefined {
  if (explicitOut) {
    const abs = isAbsolute(explicitOut) ? explicitOut : resolve(projectPath, explicitOut);
    return existsSync(abs) ? abs : undefined;
  }
  const appJson = safeReadJson(join(projectPath, "app.json"));
  if (!appJson) return undefined;
  const pub = typeof appJson.publisher === "string" ? appJson.publisher : undefined;
  const name = typeof appJson.name === "string" ? appJson.name : undefined;
  const version = typeof appJson.version === "string" ? appJson.version : undefined;
  if (!pub || !name || !version) return undefined;
  const expected = `${pub}_${name}_${version}.app`;

  const candidateDirs = [projectPath, process.cwd()];
  for (const dir of candidateDirs) {
    const exact = join(dir, expected);
    if (existsSync(exact)) return exact;
    // Fallback: the newest *.app in the directory that matches publisher+name prefix.
    const prefix = `${pub}_${name}_`;
    const entries = safeReadDir(dir).filter(
      (e) => e.endsWith(".app") && basename(e).startsWith(prefix),
    );
    if (entries.length > 0) {
      return join(dir, entries.sort().reverse()[0]!);
    }
  }
  return undefined;
}

function safeReadJson(path: string): Record<string, unknown> | undefined {
  try {
    return JSON.parse(readFileSync(path, "utf8")) as Record<string, unknown>;
  } catch {
    return undefined;
  }
}

function safeReadDir(path: string): string[] {
  try {
    return readdirSync(path);
  } catch {
    return [];
  }
}

function tail(s: string, max: number): string {
  if (!s) return "";
  return s.length <= max ? s : "…" + s.slice(s.length - max);
}
