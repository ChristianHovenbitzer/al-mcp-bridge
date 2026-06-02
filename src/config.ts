import { createHash } from "node:crypto";
import { existsSync, mkdirSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { homedir, tmpdir } from "node:os";
import { basename, dirname, isAbsolute, join, resolve, sep } from "node:path";

/**
 * Per-workspace configuration that gets forwarded to the AL LS via
 * `al/setActiveWorkspace`. Resolved from each workspace's own
 * `.vscode/settings.json` so a multi-workspace bridge can honor
 * project-specific analyzer/ruleset choices.
 */
export interface AlWorkspaceSettings {
  /** Absolute workspace root path (folder containing `app.json`). */
  workspaceRoot: string;
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

export interface BridgeConfig {
  /** Absolute path to the AL language server executable / DLL. */
  languageServerPath: string;
  /** Primary workspace root the LSP is initialized against (first entry of `workspaceFolders`). */
  workspaceRoot: string;
  /** All discovered AL project folders (each contains an `app.json`). */
  workspaceFolders: string[];
  /** Optional package cache paths forwarded to the LSP. */
  packageCachePaths: string[];
  /** Per-workspace resolved settings, keyed by absolute workspace root. */
  workspaceSettings: Map<string, AlWorkspaceSettings>;
  /** Whether the active set of workspaces was inferred via a downward scan
   *  (true) or anchored on an upward `app.json` walk / explicit `AL_WORKSPACE`
   *  (false). Useful for emitting startup warnings: a downward fallback
   *  often means the bridge picked up the wrong project. */
  resolvedViaDownwardScan: boolean;
  /** Milliseconds to wait for `publishDiagnostics` to settle after an edit. */
  diagnosticsSettleMs: number;
  // ---- legacy mirrors of the primary workspace's settings, kept for
  // ---- backwards-compat with tools that read directly off the config.
  // ---- New code should consult `workspaceSettings.get(workspaceRoot)`.
  assemblyProbingPaths: string[];
  codeAnalyzers: string[];
  enableCodeAnalysis: boolean;
  enableCodeActions: boolean;
  backgroundCodeAnalysis: string | boolean;
  ruleSetPath?: string;
}

/**
 * Default roots under which the AL VS Code extension may be installed. The
 * extension auto-updates in place, so the bridge must rediscover the binary
 * by version rather than pin a path — a hardcoded path breaks the moment the
 * extension updates and the old `ms-dynamics-smb.al-<version>` folder is
 * deleted.
 *
 * `.vscode-server` is where Remote-SSH / dev-container / WSL / Codespaces
 * installs land (and is the only root present on headless boxes), so it must
 * be searched alongside the desktop `~/.vscode` location.
 */
function defaultExtensionRoots(): string[] {
  const home = homedir();
  return [
    join(home, ".vscode-server", "extensions"),
    join(home, ".vscode-server-insiders", "extensions"),
    join(home, ".vscode", "extensions"),
    join(home, ".vscode-insiders", "extensions"),
  ];
}

/** Compare two `ms-dynamics-smb.al-<version>` dir names by numeric version
 *  (descending), so 18.0 beats 9.x — which a plain string sort gets wrong. */
function compareExtensionDirsDesc(a: string, b: string): number {
  const ver = (name: string): number[] => {
    const m = /ms-dynamics-smb\.al-(.+)$/.exec(name);
    return m ? m[1]!.split(".").map((n) => Number.parseInt(n, 10) || 0) : [];
  };
  const va = ver(a);
  const vb = ver(b);
  const len = Math.max(va.length, vb.length);
  for (let i = 0; i < len; i++) {
    const diff = (vb[i] ?? 0) - (va[i] ?? 0);
    if (diff !== 0) return diff;
  }
  return b.localeCompare(a);
}

/**
 * Candidate entry-point paths (relative to an extension folder) ordered so
 * the CURRENT platform's binary wins. The AL extension is cross-platform and
 * ships every OS's host under `bin/<platform>/`, so on Linux the `win32`
 * `.exe` is present but not executable — picking it yields a spawn EACCES.
 * Always try the running platform's native binary first.
 */
function entryNamesForPlatform(platform: NodeJS.Platform): string[] {
  const native = "Microsoft.Dynamics.Nav.EditorServices.Host";
  const byPlatform: Record<string, string[]> = {
    win32: [`bin/win32/${native}.exe`, `bin/${native}.exe`],
    linux: [`bin/linux/${native}`, `bin/${native}`],
    darwin: [`bin/darwin/${native}`, `bin/${native}`],
  };
  const preferred = byPlatform[platform] ?? [];
  // Fall back to every other platform's entry so a misreported platform or
  // an unusual package layout still resolves *something* rather than null.
  const rest = [
    `bin/${native}.exe`,
    `bin/win32/${native}.exe`,
    `bin/${native}`,
    `bin/linux/${native}`,
    `bin/darwin/${native}`,
  ].filter((e) => !preferred.includes(e));
  return [...preferred, ...rest];
}

/**
 * Locate the AL language server binary inside the installed VS Code
 * extension. Returns the newest matching install or null if none found.
 *
 * The AL extension host binary lives under
 *   <root>/ms-dynamics-smb.al-<version>/bin/[<platform>/]Microsoft.Dynamics.Nav.EditorServices.Host[.exe]
 * Versions are compared numerically and the newest is preferred; `AL_LS_PATH`
 * still overrides this when set (see `resolveLanguageServerPath`).
 *
 * @param roots Extension roots to search, newest-version-wins within each in
 *   listed order. Defaults to the standard desktop + server locations;
 *   parameterized for testability.
 */
export function autodetectLanguageServer(
  roots: string[] = defaultExtensionRoots(),
  platform: NodeJS.Platform = process.platform,
): string | null {
  const entryNames = entryNamesForPlatform(platform);

  for (const extensionsDir of roots) {
    if (!existsSync(extensionsDir)) continue;

    const candidates = readdirSync(extensionsDir)
      .filter((d) => d.startsWith("ms-dynamics-smb.al-"))
      .filter((d) => {
        try {
          return statSync(join(extensionsDir, d)).isDirectory();
        } catch {
          return false;
        }
      })
      .sort(compareExtensionDirsDesc)
      .map((d) => join(extensionsDir, d));

    for (const ext of candidates) {
      for (const entry of entryNames) {
        const p = join(ext, entry);
        if (existsSync(p)) return p;
      }
    }
  }
  return null;
}

/**
 * Resolve the AL language server path, preferring an explicit `AL_LS_PATH`
 * but self-healing when it is stale. An extension update deletes the old
 * version folder, so a previously-correct `AL_LS_PATH` can point at a binary
 * that no longer exists; rather than hand that dead path to `spawn` (which
 * fails ENOENT and takes the whole bridge down), fall back to autodetection.
 */
export function resolveLanguageServerPath(): string | null {
  const fromEnv = process.env.AL_LS_PATH?.trim();
  if (fromEnv) {
    if (existsSync(fromEnv)) return fromEnv;
    process.stderr.write(
      `[al-mcp-bridge] AL_LS_PATH points to a missing file (likely a stale ` +
        `extension version after an update): ${fromEnv}\n` +
        `[al-mcp-bridge] falling back to autodetecting the installed AL extension.\n`,
    );
  }
  return autodetectLanguageServer();
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

export interface DiscoverResult {
  folders: string[];
  /** True when no upward `app.json` was found and we fell back to scanning
   *  down from cwd. The caller may want to warn — downward fallback often
   *  catches an unintended project when the bridge launches from a tool
   *  repo (e.g. al-mcp-bridge itself, finding tests/fixtures). */
  viaDownwardScan: boolean;
}

/**
 * Resolve which AL project folders this bridge should serve.
 *
 * Resolution order:
 *   1. `AL_WORKSPACE` (semicolon-separated list, each must exist and have `app.json`)
 *   2. Nearest `app.json` walking upward from cwd (single-project case)
 *   3. All `app.json` folders discovered by scanning subfolders of cwd (monorepo case)
 */
export function discoverAlWorkspaces(start: string): DiscoverResult {
  const upward = findAlProjectUpward(start);
  if (upward) return { folders: [upward], viaDownwardScan: false };
  return { folders: findAlProjectsDownward(start), viaDownwardScan: true };
}

/**
 * Resolve per-workspace settings: re-read this workspace's
 * `.vscode/settings.json` and expand placeholders against its own root, so
 * analyzer paths / rulesets follow the project rather than the bridge's
 * primary workspace.
 */
export function resolveWorkspaceSettings(
  workspaceRoot: string,
  lsPath: string,
): AlWorkspaceSettings {
  const settings = readWorkspaceSettings(workspaceRoot);
  const analyzerFolder = deriveAnalyzerFolder(lsPath);
  const ctx: PlaceholderCtx = {
    analyzerFolder,
    workspaceFolder: workspaceRoot,
    alWorkspaceFolder: workspaceRoot,
  };

  const enableCodeAnalysis = readBool(settings, "al.enableCodeAnalysis") ?? true;
  const enableCodeActions = readBool(settings, "al.enableCodeActions") ?? true;

  // AL_EXTRA_CODE_ANALYZERS is documented as "active regardless of what each
  // project's settings.json says" — i.e. team-wide enforcement that no
  // workspace can opt out of. Resolve it independently of the master switch
  // so a workspace with `al.enableCodeAnalysis: false` still gets the extras,
  // and force-enable the pipeline in that case (otherwise the LS won't
  // schedule analyzer runs and the DLLs sit inert).
  const fromSettings = readStringArray(settings, "al.codeAnalyzers") ?? [];
  const fromEnv = parseDelimitedList(process.env.AL_EXTRA_CODE_ANALYZERS);
  let codeAnalyzers: string[] = [];
  let effectiveEnableCodeAnalysis = enableCodeAnalysis;
  if (enableCodeAnalysis) {
    codeAnalyzers = resolveCodeAnalyzers([...fromSettings, ...fromEnv], ctx);
  } else if (fromEnv.length > 0) {
    // Workspace disabled analysis, but team policy via env wins: load only
    // the env extras (NOT the workspace's `al.codeAnalyzers` — those were
    // explicitly disabled) and turn the master switch back on.
    codeAnalyzers = resolveCodeAnalyzers(fromEnv, ctx);
    if (codeAnalyzers.length > 0) {
      effectiveEnableCodeAnalysis = true;
      process.stderr.write(
        `[al-mcp-bridge] ${workspaceRoot}: forcing al.enableCodeAnalysis=true ` +
          `because AL_EXTRA_CODE_ANALYZERS provides ${codeAnalyzers.length} ` +
          `analyzer(s) and the workspace had analysis disabled.\n`,
      );
    }
  }
  if (codeAnalyzers.length > 0) {
    codeAnalyzers = augmentWithAnalyzerSiblings(codeAnalyzers);
  }

  const backgroundCodeAnalysis =
    readString(settings, "al.backgroundCodeAnalysis") ?? "File";
  const ruleSetPath = resolveEffectiveRuleSetPath(settings, ctx);
  const assemblyProbingPaths = (readStringArray(settings, "al.assemblyProbingPaths") ?? [])
    .map((p) => resolvePlaceholders(p, ctx))
    .map((p) => (isAbsolute(p) ? p : resolve(workspaceRoot, p)))
    .filter((p) => existsSync(p));

  return {
    workspaceRoot,
    assemblyProbingPaths,
    codeAnalyzers,
    enableCodeAnalysis: effectiveEnableCodeAnalysis,
    enableCodeActions,
    backgroundCodeAnalysis,
    ruleSetPath,
  };
}

export function loadConfig(): BridgeConfig {
  const lsPath = resolveLanguageServerPath();
  if (!lsPath) {
    throw new Error(
      "Could not locate the AL language server. Install the AL VS Code " +
        "extension, or set AL_LS_PATH to the absolute path of " +
        "Microsoft.Dynamics.Nav.EditorServices.Host(.exe).",
    );
  }

  let workspaceFolders: string[];
  let resolvedViaDownwardScan = false;
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
    const discovered = discoverAlWorkspaces(cwd);
    workspaceFolders = discovered.folders;
    resolvedViaDownwardScan = discovered.viaDownwardScan;
    if (workspaceFolders.length === 0) {
      throw new Error(
        `No AL project (app.json) found at, above, or under ${cwd}. ` +
          "Set AL_WORKSPACE to a semicolon-separated list of AL project paths.",
      );
    }
  }

  const workspaceRoot = workspaceFolders[0]!;
  const workspaceSettings = new Map<string, AlWorkspaceSettings>();
  for (const folder of workspaceFolders) {
    workspaceSettings.set(folder, resolveWorkspaceSettings(folder, lsPath));
  }
  const primary = workspaceSettings.get(workspaceRoot)!;

  const packageCachePaths = (process.env.AL_PACKAGE_CACHE ?? "")
    .split(";")
    .map((s) => s.trim())
    .filter(Boolean);

  return {
    languageServerPath: lsPath,
    workspaceRoot,
    workspaceFolders,
    workspaceSettings,
    resolvedViaDownwardScan,
    packageCachePaths,
    assemblyProbingPaths: primary.assemblyProbingPaths,
    codeAnalyzers: primary.codeAnalyzers,
    enableCodeAnalysis: primary.enableCodeAnalysis,
    enableCodeActions: primary.enableCodeActions,
    diagnosticsSettleMs: Number(process.env.AL_DIAGNOSTICS_SETTLE_MS ?? 750),
    backgroundCodeAnalysis: primary.backgroundCodeAnalysis,
    ruleSetPath: primary.ruleSetPath,
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
 * Several analyzer DLLs depend on helper assemblies that ship beside them
 * in the AL extension's `Analyzers/` folder but aren't themselves
 * `DiagnosticAnalyzer` types:
 *
 *   - `Microsoft.Dynamics.Nav.CodeCop.dll` + other MS cops reach into
 *     `Microsoft.Dynamics.Nav.Analyzers.Common.dll` (and sometimes
 *     `Microsoft.Dynamics.Nav.AL.Common.dll`). Without them, specific
 *     rules (e.g. `EmailAndPhoneNoMustNotBePresentInTheSource`) crash in
 *     their `Initialize` override.
 *   - `ALCops.LinterCop.dll` depends on `ALCops.Common.dll`.
 *   - `BusinessCentral.LinterCop.dll` (community fork) depends on
 *     `Microsoft.Dynamics.Nav.Analyzers.Common.dll`.
 *
 * Roslyn's analyzer loader puts each entry from `codeAnalyzers` in its own
 * AssemblyLoadContext and does not probe the DLL's own folder for siblings.
 * The fix is to list helper DLLs explicitly in `codeAnalyzers` so the LS
 * loads them into the shared ALC where analyzer init code can resolve them.
 * Every surfaced `AD0001 … Could not load file or assembly …Common…` error
 * on `app.json` ultimately traces back to a sibling missing from this list.
 */
interface AnalyzerSiblingRule {
  /** Matched against the lowercase basename of a configured analyzer DLL. */
  match: RegExp;
  /** Helper DLL filenames to co-load from the same folder (when present). */
  siblings: string[];
}

const ANALYZER_SIBLING_RULES: AnalyzerSiblingRule[] = [
  {
    match: /^businesscentral\.lintercop\.dll$/,
    siblings: ["Microsoft.Dynamics.Nav.Analyzers.Common.dll"],
  },
  {
    match: /^alcops\..+\.dll$/,
    siblings: ["ALCops.Common.dll"],
  },
  {
    match: /^microsoft\.dynamics\.nav\.(codecop|appsourcecop|uicop|pertenantextensioncop)\.dll$/,
    siblings: [
      "Microsoft.Dynamics.Nav.Analyzers.Common.dll",
      "Microsoft.Dynamics.Nav.AL.Common.dll",
    ],
  },
  // Socitas.ReviewerCop is built on top of ALCops/Analyzers and ships both
  // its own per-analyzer Common DLL and the ALCops helpers. Distinct from
  // LinterCop because the analyzer isn't in the `alcops.` namespace even
  // though it transitively depends on `ALCops.Common`.
  {
    match: /^socitas\.reviewercop\.dll$/,
    siblings: [
      "Socitas.ReviewerCop.Common.dll",
      "ALCops.Common.dll",
      "ALCops.CompanyCop.dll",
    ],
  },
];

function augmentWithAnalyzerSiblings(codeAnalyzers: string[]): string[] {
  const seen = new Set(codeAnalyzers.map((p) => p.toLowerCase()));
  const siblings: string[] = [];
  for (const analyzer of codeAnalyzers) {
    const name = basename(analyzer).toLowerCase();
    const dir = dirname(analyzer);
    for (const rule of ANALYZER_SIBLING_RULES) {
      if (!rule.match.test(name)) continue;
      for (const sibling of rule.siblings) {
        const p = join(dir, sibling);
        const key = p.toLowerCase();
        if (seen.has(key)) continue;
        if (!existsSync(p)) continue;
        seen.add(key);
        siblings.push(p);
      }
    }
  }
  // Prepend siblings so they are loaded before the analyzers that depend on
  // them. When alc (or the language server) uses a shared AssemblyLoadContext
  // for all /analyzer: entries, loading the common helper DLLs first ensures
  // they are already in the ALC when the main analyzer types are instantiated.
  return [...siblings, ...codeAnalyzers];
}

/**
 * Merge the workspace's `al.ruleSetPath` (if any) with every entry in
 * `AL_EXTRA_RULESETS`. AL's `alResourceConfigurationSettings.ruleSetPath`
 * takes a single file, so when multiple sources exist we synthesize a
 * composite ruleset that `includedRuleSets`-chains them and point the LS at
 * that. Zero / one source cases are passed through verbatim.
 */
function resolveEffectiveRuleSetPath(
  settings: Record<string, unknown> | undefined,
  ctx: PlaceholderCtx,
): string | undefined {
  const collected: string[] = [];

  const fromSettings = readString(settings, "al.ruleSetPath");
  if (fromSettings) {
    const expanded = resolvePlaceholders(fromSettings, ctx);
    const abs = isAbsolute(expanded) ? expanded : resolve(ctx.workspaceFolder, expanded);
    if (existsSync(abs)) {
      collected.push(abs);
    } else {
      process.stderr.write(
        `[al-mcp-bridge] al.ruleSetPath not found, skipping: ${abs}\n`,
      );
    }
  }

  for (const entry of parseDelimitedList(process.env.AL_EXTRA_RULESETS)) {
    const expanded = resolvePlaceholders(entry, ctx);
    const abs = isAbsolute(expanded) ? expanded : resolve(ctx.workspaceFolder, expanded);
    if (!existsSync(abs)) {
      process.stderr.write(
        `[al-mcp-bridge] AL_EXTRA_RULESETS entry not found, skipping: ${abs}\n`,
      );
      continue;
    }
    if (collected.some((p) => p.toLowerCase() === abs.toLowerCase())) continue;
    collected.push(abs);
  }

  if (collected.length === 0) return undefined;
  if (collected.length === 1) return collected[0];
  return writeCompositeRuleSet(collected, ctx.workspaceFolder);
}

/**
 * Emit a synthesized ruleset that chains each source through
 * `includedRuleSets`. Written under the OS temp directory with a stable
 * per-workspace hash so repeated runs reuse the same file and multiple
 * workspaces don't collide. Every included path is absolute, so the
 * composite's location doesn't constrain resolution.
 */
function writeCompositeRuleSet(paths: string[], workspaceRoot: string): string {
  const hash = createHash("sha1").update(workspaceRoot).digest("hex").slice(0, 8);
  const dir = join(tmpdir(), "al-mcp-bridge");
  const file = join(dir, `${hash}.merged.ruleset.json`);
  const body = {
    name: "al-mcp-bridge merged ruleset",
    description:
      "Auto-generated. Composes al.ruleSetPath with AL_EXTRA_RULESETS entries.",
    includedRuleSets: paths.map((p) => ({ path: p, action: "Default" })),
  };
  try {
    mkdirSync(dir, { recursive: true });
    writeFileSync(file, JSON.stringify(body, null, 2), "utf8");
  } catch (err) {
    process.stderr.write(
      `[al-mcp-bridge] failed to write merged ruleset at ${file}: ${(err as Error).message}\n`,
    );
    return paths[0]!;
  }
  return file;
}

/**
 * Split an env-provided list of paths on `;` (documented) OR `,` (lenient).
 * Both delimiters appear in the wild — `;` matches Windows PATH conventions
 * and JSON-array intuition, `,` is what users reach for when typing a list
 * into a JSON string. Neither delimiter is a valid character in any AL
 * analyzer DLL filename we ship, so accepting both is safe in practice.
 */
function parseDelimitedList(v: string | undefined): string[] {
  if (!v) return [];
  return v.split(/[;,]/).map((s) => s.trim()).filter(Boolean);
}

