import { existsSync, readdirSync, statSync } from "node:fs";
import { homedir } from "node:os";
import { join, resolve } from "node:path";

export interface BridgeConfig {
  /** Absolute path to the AL language server executable / DLL. */
  languageServerPath: string;
  /** Workspace root the LSP will be initialized against. */
  workspaceRoot: string;
  /** Optional package cache paths forwarded to the LSP. */
  packageCachePaths: string[];
  /** Milliseconds to wait for `publishDiagnostics` to settle after an edit. */
  diagnosticsSettleMs: number;
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

export function loadConfig(): BridgeConfig {
  const lsPath = process.env.AL_LS_PATH ?? autodetectLanguageServer();
  if (!lsPath) {
    throw new Error(
      "Could not locate the AL language server. Set AL_LS_PATH to the " +
        "absolute path of Microsoft.Dynamics.Nav.EditorServices.Host(.exe).",
    );
  }

  const workspaceRoot = resolve(process.env.AL_WORKSPACE ?? process.cwd());

  const packageCachePaths = (process.env.AL_PACKAGE_CACHE ?? "")
    .split(";")
    .map((s) => s.trim())
    .filter(Boolean);

  return {
    languageServerPath: lsPath,
    workspaceRoot,
    packageCachePaths,
    diagnosticsSettleMs: Number(process.env.AL_DIAGNOSTICS_SETTLE_MS ?? 750),
  };
}
