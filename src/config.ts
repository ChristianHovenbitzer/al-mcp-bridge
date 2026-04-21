import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { homedir } from "node:os";
import { basename, dirname, isAbsolute, join, resolve, sep } from "node:path";

export interface BridgeConfig {
  /** Absolute path to the AL language server executable / DLL. */
  languageServerPath: string;
  /** Primary workspace root the LSP is initialized against (first entry of `workspaceFolders`). */
  workspaceRoot: string;
  /** All discovered AL project folders (each contains an `app.json`). */
  workspaceFolders: string[];
  /** Optional package cache paths forwarded to the LSP. */
  packageCachePaths: string[];
  /**
   * Merged probing paths handed to `al/setActiveWorkspace`: user-provided
   * `al.assemblyProbingPaths` from `.vscode/settings.json` plus every parent
   * directory of a configured analyzer DLL. The latter exists so third-party
   * analyzers (LinterCop, ALCops) can resolve sibling helper assemblies.
   */
  assemblyProbingPaths: string[];
  /**
   * Absolute paths to Roslyn analyzer DLLs (Microsoft CodeCop, LinterCop,
   * etc.) derived from `al.codeAnalyzers` in `.vscode/settings.json` with
   * VS Code placeholders expanded.
   */
  codeAnalyzers: string[];
  /** Mirrors `al.enableCodeAnalysis` — master switch for the analyzer pipeline. */
  enableCodeAnalysis: boolean;
  /** Mirrors `al.enableCodeActions`. */
  enableCodeActions: boolean;
  /** Milliseconds to wait for `publishDiagnostics` to settle after an edit. */
  diagnosticsSettleMs: number;
  /**
   * Mirrors `al.backgroundCodeAnalysis`: "None" | "File" | "Project" | true | false.
   * Forwarded verbatim to `al/setActiveWorkspace`. Without this, the AL LS
   * does not schedule the analyzer pass that drives `publishDiagnostics`
   * for third-party analyzers (LinterCop, ALCops).
   */
  backgroundCodeAnalysis: string | boolean;
  /** Absolute path to the project ruleset JSON (mirrors `al.ruleSetPath`). */
  ruleSetPath?: string;
}

/**
 * Locate the AL language server binary inside the installed VS Code
 * extension. Returns the newest matching install or null if none found.
 *
 * The AL extension host binary lives under
 *   ~/.vscode/extensions/ms-dynamics-smb.al-<version>/bin/
 * The actual entry point varies by platform and extension version — the
 * caller is expected to override this via AL_LS_PATH when autodetect
 * guesses wrong.
 */
export function autodetectLanguageServer(): string | null {
  const extensionsDir = join(homedir(), ".vscode", "extensions");
  if (!existsSync(extensionsDir)) return null;

  const candidates = readdirSync(extensionsDir)
    .filter((d) => d.startsWith("ms-dynamics-smb.al-"))
    .map((d) => join(extensionsDir, d))
    .filter((p) => statSync(p).isDirectory())
    .sort()
    .reverse();

  const entryNames = [
    "bin/Microsoft.Dynamics.Nav.EditorServices.Host.exe",
    "bin/win32/Microsoft.Dynamics.Nav.EditorServices.Host.exe",
    "bin/Microsoft.Dynamics.Nav.EditorServices.Host",
    "bin/linux/Microsoft.Dynamics.Nav.EditorServices.Host",
    "bin/darwin/Microsoft.Dynamics.Nav.EditorServices.Host",
  ];

  for (const ext of candidates) {
    for (const entry of entryNames) {
      const p = join(ext, entry);
      if (existsSync(p)) return p;
    }
  }
  return null;
}

/**
 * Walk upward from `start` looking for the nearest directory containing
 * `app.json`. Returns that directory, or null if none found before the
 * filesystem root.
 */
function findAlProjectUpward(start: string): string | null {
  let cur = resolve(start);
  // eslint-disable-next-line no-constant-condition
  while (true) {
    if (existsSync(join(cur, "app.json"))) return cur;
    const parent = dirname(cur);
    if (parent === cur) return null;
    cur = parent;
  }
}

const SKIP_DIRS = new Set([
  "node_modules",
  ".git",
  ".alpackages",
  ".altemplates",
  ".snapshots",
  ".vscode",
  "bin",
  "obj",
  "out",
  "dist",
  "build",
  ".next",
  ".turbo",
]);

/**
 * Recursively scan up to `maxDepth` levels under `start` for folders
 * containing `app.json`. Stops descending once an AL project is found
 * (nested AL projects are uncommon and usually represent symlink loops).
 */
function findAlProjectsDownward(start: string, maxDepth = 4): string[] {
  const results: string[] = [];
  const root = resolve(start);

  function walk(dir: string, depth: number): void {
    if (depth > maxDepth) return;
    let entries: string[];
    try {
      entries = readdirSync(dir);
    } catch {
      return;
    }
    if (entries.includes("app.json")) {
      results.push(dir);
      return;
    }
    for (const name of entries) {
      if (SKIP_DIRS.has(name) || name.startsWith(".")) continue;
      const p = join(dir, name);
      try {
        if (statSync(p).isDirectory()) walk(p, depth + 1);
      } catch {
        // unreadable entry — ignore
      }
    }
  }

  walk(root, 0);
  return results;
}

/**
 * Resolve which AL project folders this bridge should serve.
 *
 * Resolution order:
 *   1. `AL_WORKSPACE` (semicolon-separated list, each must exist and have `app.json`)
 *   2. Nearest `app.json` walking upward from cwd (single-project case)
 *   3. All `app.json` folders discovered by scanning subfolders of cwd (monorepo case)
 */
export function discoverAlWorkspaces(start: string): string[] {
  const upward = findAlProjectUpward(start);
  if (upward) return [upward];
  return findAlProjectsDownward(start);
}

export function loadConfig(): BridgeConfig {
  const lsPath = process.env.AL_LS_PATH ?? autodetectLanguageServer();
  if (!lsPath) {
    throw new Error(
      "Could not locate the AL language server. Set AL_LS_PATH to the " +
        "absolute path of Microsoft.Dynamics.Nav.EditorServices.Host(.exe).",
    );
  }

  let workspaceFolders: string[];
  if (process.env.AL_WORKSPACE) {
    workspaceFolders = process.env.AL_WORKSPACE.split(";")
      .map((s) => s.trim())
      .filter(Boolean)
      .map((p) => resolve(p));
    for (const p of workspaceFolders) {
      if (!existsSync(p)) {
        throw new Error(`AL_WORKSPACE entry does not exist: ${p}`);
      }
    }
  } else {
    const cwd = process.cwd();
    workspaceFolders = discoverAlWorkspaces(cwd);
    if (workspaceFolders.length === 0) {
      throw new Error(
        `No AL project (app.json) found at, above, or under ${cwd}. ` +
          "Set AL_WORKSPACE to a semicolon-separated list of AL project paths.",
      );
    }
  }

  const workspaceRoot = workspaceFolders[0]!;
  const settings = readWorkspaceSettings(workspaceRoot);
  const analyzerFolder = deriveAnalyzerFolder(lsPath);
  const ctx: PlaceholderCtx = {
    analyzerFolder,
    workspaceFolder: workspaceRoot,
    alWorkspaceFolder: workspaceRoot,
  };

  const enableCodeAnalysis = readBool(settings, "al.enableCodeAnalysis") ?? true;
  const enableCodeActions = readBool(settings, "al.enableCodeActions") ?? true;

  let codeAnalyzers: string[] = [];
  if (enableCodeAnalysis) {
    const raw = readStringArray(settings, "al.codeAnalyzers") ?? [];
    codeAnalyzers = resolveCodeAnalyzers(raw, ctx);
    codeAnalyzers = augmentWithLinterCopSiblings(codeAnalyzers);
  }

  const backgroundCodeAnalysis =
    readString(settings, "al.backgroundCodeAnalysis") ?? "File";

  const ruleSetPath = resolveRuleSetPath(settings, ctx);

  const settingsProbingPaths = (readStringArray(settings, "al.assemblyProbingPaths") ?? [])
    .map((p) => resolvePlaceholders(p, ctx))
    .map((p) => (isAbsolute(p) ? p : resolve(workspaceRoot, p)))
    .filter((p) => existsSync(p));
  const analyzerDirs = uniqueDirs(codeAnalyzers);
  const assemblyProbingPaths = dedupe([...settingsProbingPaths, ...analyzerDirs]);

  const packageCachePaths = (process.env.AL_PACKAGE_CACHE ?? "")
    .split(";")
    .map((s) => s.trim())
    .filter(Boolean);

  return {
    languageServerPath: lsPath,
    workspaceRoot,
    workspaceFolders,
    packageCachePaths,
    assemblyProbingPaths,
    codeAnalyzers,
    enableCodeAnalysis,
    enableCodeActions,
    diagnosticsSettleMs: Number(process.env.AL_DIAGNOSTICS_SETTLE_MS ?? 750),
    backgroundCodeAnalysis,
    ruleSetPath,
  };
}

// ---------------------------------------------------------------------------
// settings.json ingestion
// ---------------------------------------------------------------------------

function readWorkspaceSettings(
  workspaceRoot: string,
): Record<string, unknown> | undefined {
  const settingsFile = join(workspaceRoot, ".vscode", "settings.json");
  if (!existsSync(settingsFile)) return undefined;
  try {
    const raw = readFileSync(settingsFile, "utf8");
    return JSON.parse(stripJsonComments(raw)) as Record<string, unknown>;
  } catch {
    return undefined;
  }
}

function readString(
  s: Record<string, unknown> | undefined,
  key: string,
): string | undefined {
  const v = s?.[key];
  return typeof v === "string" && v.trim() ? v.trim() : undefined;
}

function readBool(
  s: Record<string, unknown> | undefined,
  key: string,
): boolean | undefined {
  const v = s?.[key];
  return typeof v === "boolean" ? v : undefined;
}

function readStringArray(
  s: Record<string, unknown> | undefined,
  key: string,
): string[] | undefined {
  const v = s?.[key];
  if (!Array.isArray(v)) return undefined;
  return v.filter((x): x is string => typeof x === "string");
}

/** Minimal comment stripper — VS Code `settings.json` allows `//` and block comments. */
function stripJsonComments(input: string): string {
  return input
    .replace(/\/\*[\s\S]*?\*\//g, "")
    .replace(/(^|[^:"'])\/\/[^\n\r]*/g, "$1");
}

// ---------------------------------------------------------------------------
// VS Code placeholder + analyzer path resolution
// ---------------------------------------------------------------------------

/**
 * Walk up from the LS entry point to find its sibling `Analyzers` folder.
 * Typical layouts:
 *   .../bin/win32/Microsoft.Dynamics.Nav.EditorServices.Host.exe → .../bin/Analyzers/
 *   .../bin/Microsoft.Dynamics.Nav.EditorServices.Host.exe      → .../bin/Analyzers/
 */
function deriveAnalyzerFolder(lsPath: string): string | undefined {
  let dir = dirname(lsPath);
  const leaf = basename(dir).toLowerCase();
  if (leaf === "win32" || leaf === "linux" || leaf === "darwin") {
    dir = dirname(dir);
  }
  const analyzers = join(dir, "Analyzers");
  return existsSync(analyzers) ? analyzers : undefined;
}

interface PlaceholderCtx {
  /** Absolute path to the AL extension's `Analyzers` folder, if found. */
  analyzerFolder?: string;
  /** VS Code's `${workspaceFolder}` — the AL project root. */
  workspaceFolder: string;
  /** VS Code's `${alWorkspaceFolder}` — same as workspaceFolder for single-project setups. */
  alWorkspaceFolder: string;
}

/**
 * Expand the VS Code placeholders used in `al.codeAnalyzers`,
 * `al.ruleSetPath`, and `al.assemblyProbingPaths`. Unknown placeholders are
 * left in place so downstream `existsSync` checks can catch misconfigurations.
 */
function resolvePlaceholders(value: string, ctx: PlaceholderCtx): string {
  const af = ctx.analyzerFolder;
  const afPrefix = af ? af + sep : "";
  const asCop = af ? join(af, "Microsoft.Dynamics.Nav.AppSourceCop.dll") : "";
  const cCop = af ? join(af, "Microsoft.Dynamics.Nav.CodeCop.dll") : "";
  const pCop = af ? join(af, "Microsoft.Dynamics.Nav.PerTenantExtensionCop.dll") : "";
  const uCop = af ? join(af, "Microsoft.Dynamics.Nav.UICop.dll") : "";
  return value
    .replace(/\$\{analyzerFolder\}/g, afPrefix)
    .replace(/\$\{workspaceFolder\}/g, ctx.workspaceFolder)
    .replace(/\$\{alWorkspaceFolder\}/g, ctx.alWorkspaceFolder)
    .replace(/\$\{AppSourceCop\}/g, asCop)
    .replace(/\$\{CodeCop\}/g, cCop)
    .replace(/\$\{PerTenantExtensionCop\}/g, pCop)
    .replace(/\$\{UICop\}/g, uCop);
}

function resolveCodeAnalyzers(raw: string[], ctx: PlaceholderCtx): string[] {
  const out: string[] = [];
  const seen = new Set<string>();
  for (const entry of raw) {
    const expanded = resolvePlaceholders(entry, ctx);
    if (!expanded) continue;
    const abs = isAbsolute(expanded) ? expanded : resolve(ctx.workspaceFolder, expanded);
    if (!existsSync(abs)) {
      process.stderr.write(
        `[al-mcp-bridge] al.codeAnalyzers entry not found, skipping: ${abs}\n`,
      );
      continue;
    }
    const key = abs.toLowerCase();
    if (seen.has(key)) continue;
    seen.add(key);
    out.push(abs);
  }
  return out;
}

/**
 * LinterCop depends on `Microsoft.Dynamics.Nav.Analyzers.Common.dll`, shipped
 * alongside it in the AL extension's Analyzers folder but not itself a
 * `DiagnosticAnalyzer`. Roslyn's analyzer loader isolates each entry in its
 * own ALC and does not probe the analyzer's folder, so LinterCop's
 * constructors throw `FileNotFoundException` (surfaced as `AD0001` on
 * `app.json`) unless the Common assembly is also in the analyzer list.
 * Co-listing forces the LS to load it into the shared ALC.
 */
function augmentWithLinterCopSiblings(codeAnalyzers: string[]): string[] {
  const hasLinterCop = codeAnalyzers.some(
    (p) => basename(p).toLowerCase() === "businesscentral.lintercop.dll",
  );
  if (!hasLinterCop) return codeAnalyzers;
  const out = [...codeAnalyzers];
  const seen = new Set(out.map((p) => p.toLowerCase()));
  for (const p of codeAnalyzers) {
    if (basename(p).toLowerCase() !== "businesscentral.lintercop.dll") continue;
    const common = join(dirname(p), "Microsoft.Dynamics.Nav.Analyzers.Common.dll");
    const key = common.toLowerCase();
    if (existsSync(common) && !seen.has(key)) {
      seen.add(key);
      out.push(common);
    }
  }
  return out;
}

function resolveRuleSetPath(
  settings: Record<string, unknown> | undefined,
  ctx: PlaceholderCtx,
): string | undefined {
  const value = readString(settings, "al.ruleSetPath");
  if (!value) return undefined;
  const expanded = resolvePlaceholders(value, ctx);
  return isAbsolute(expanded) ? expanded : resolve(ctx.workspaceFolder, expanded);
}

function uniqueDirs(filePaths: string[]): string[] {
  const seen = new Set<string>();
  const out: string[] = [];
  for (const p of filePaths) {
    const d = dirname(p);
    if (!d) continue;
    const key = d.toLowerCase();
    if (seen.has(key)) continue;
    seen.add(key);
    out.push(d);
  }
  return out;
}

function dedupe(paths: string[]): string[] {
  const seen = new Set<string>();
  const out: string[] = [];
  for (const p of paths) {
    const key = p.toLowerCase();
    if (seen.has(key)) continue;
    seen.add(key);
    out.push(p);
  }
  return out;
}
