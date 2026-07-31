import { existsSync, statSync } from "node:fs";
import { join, resolve, sep } from "node:path";
import { z } from "zod";
import { resolveWorkspaceSettings } from "../config.js";
import type { BridgeConfig } from "../config.js";
import type { AlLspClient } from "../lsp/client.js";

export const LoadWorkspaceInput = z.object({
  path: z
    .string()
    .describe(
      "Absolute path to an AL project folder (the directory containing `app.json`).",
    ),
  setActive: z
    .boolean()
    .default(false)
    .describe(
      "If true, request that the LSP make this the active workspace. Default false adds the folder alongside the existing active workspace.",
    ),
});

export type LoadWorkspaceInputT = z.infer<typeof LoadWorkspaceInput>;

export interface LoadWorkspaceResult {
  path: string;
  added: boolean;
  alreadyLoaded: boolean;
  workspaceFolders: string[];
  resolvedSettings: {
    codeAnalyzers: string[];
    assemblyProbingPaths: string[];
    enableCodeAnalysis: boolean;
    enableCodeActions: boolean;
    backgroundCodeAnalysis: string | boolean;
    ruleSetPath?: string;
  };
}

export function createLoadWorkspace(client: AlLspClient, config: BridgeConfig) {
  return async (input: LoadWorkspaceInputT): Promise<LoadWorkspaceResult> => {
    const folder = resolve(input.path);
    if (!existsSync(folder) || !statSync(folder).isDirectory()) {
      throw new Error(`Path is not an existing directory: ${folder}`);
    }
    if (!existsSync(join(folder, "app.json"))) {
      throw new Error(
        `No app.json in ${folder}. al_load_workspace expects the AL project root, not a subfolder.`,
      );
    }

    const alreadyLoaded = client.getWorkspaceFolders().includes(folder);
    const settings = resolveWorkspaceSettings(folder, config.languageServerPath);
    const { added } = await client.addWorkspace(folder, settings, input.setActive);

    if (added) {
      // Mirror into the bridge-side state so guards / list tools see it.
      if (!config.workspaceFolders.includes(folder)) {
        config.workspaceFolders.push(folder);
      }
      config.workspaceSettings.set(folder, settings);
    }

    return {
      path: folder,
      added,
      alreadyLoaded,
      workspaceFolders: client.getWorkspaceFolders(),
      resolvedSettings: {
        codeAnalyzers: settings.codeAnalyzers,
        assemblyProbingPaths: settings.assemblyProbingPaths,
        enableCodeAnalysis: settings.enableCodeAnalysis,
        enableCodeActions: settings.enableCodeActions,
        backgroundCodeAnalysis: settings.backgroundCodeAnalysis,
        ruleSetPath: settings.ruleSetPath,
      },
    };
  };
}

export const ListWorkspacesInput = z.object({}).strict();
export type ListWorkspacesInputT = z.infer<typeof ListWorkspacesInput>;

export interface ListWorkspacesResult {
  workspaceFolders: Array<{
    path: string;
    isPrimary: boolean;
    codeAnalyzers: string[];
    ruleSetPath?: string;
  }>;
}

export function createListWorkspaces(client: AlLspClient, config: BridgeConfig) {
  return async (): Promise<ListWorkspacesResult> => {
    const folders = client.getWorkspaceFolders();
    return {
      workspaceFolders: folders.map((f) => {
        const s = config.workspaceSettings.get(f);
        return {
          path: f,
          isPrimary: f === config.workspaceRoot,
          codeAnalyzers: s?.codeAnalyzers ?? [],
          ruleSetPath: s?.ruleSetPath,
        };
      }),
    };
  };
}

/**
 * True when `filePath` lives under (or equals) one of the loaded workspace
 * folders. Comparison is path-segment-aware so e.g. `/a/b` doesn't match
 * `/a/bb`.
 */
export function isPathInWorkspace(filePath: string, folders: string[]): boolean {
  const file = resolve(filePath);
  for (const folder of folders) {
    const root = resolve(folder);
    if (file === root) return true;
    if (file.startsWith(root + sep)) return true;
  }
  return false;
}

/**
 * Throws a descriptive error when `file` isn't inside any loaded workspace.
 * The error names the loaded folders and points the caller at the
 * `al_load_workspace` tool — the typical recovery path.
 */
export function assertFileInWorkspace(file: string, folders: string[]): void {
  if (isPathInWorkspace(file, folders)) return;
  const list = folders.length
    ? folders.map((f) => `  - ${f}`).join("\n")
    : "  (none)";
  throw new Error(
    `File is not inside any loaded AL workspace:\n` +
      `  file: ${file}\n` +
      `Loaded workspaces:\n${list}\n` +
      `The LSP only resolves diagnostics, symbols, and edits for files inside a loaded workspace. ` +
      `Call al_load_workspace with the absolute path to the AL project root (the folder containing app.json) and retry.`,
  );
}
