/**
 * al_restart_lsp - kill the AL language server and bring a fresh one up,
 * optionally re-targeted at a different AL project.
 *
 * Why this exists: the bridge is one long-lived stdio process per Claude
 * session, launched from wherever the session started (typically a home
 * directory, not an AL repo). Two things then go wrong in practice:
 *
 *   1. The LS wedges - a faulted analyzer, an `al/setActiveWorkspace` that
 *      never answers, a half-dead child after a symbol-cache change. Every
 *      tool call then fails on its deadline until the process is replaced.
 *      The CLR also pins analyzer DLLs for the LS process lifetime, so
 *      picking up a rebuilt cop *requires* a respawn.
 *   2. The session moves to another repo under C:\git and the LS is still
 *      initialized against the previous project.
 *
 * Both used to mean "restart Claude Code". This tool handles them in place.
 */
import { existsSync, statSync } from "node:fs";
import { join, resolve } from "node:path";
import { z } from "zod";
import { retargetWorkspaces, type BridgeConfig } from "../config.js";
import type { AlLspClient } from "../lsp/client.js";

export const RestartLspInput = z.object({
  workspace: z
    .string()
    .optional()
    .describe(
      "Absolute path to the AL project root (the folder containing app.json) the restarted LSP should treat as its primary workspace. Omit to respawn against the currently loaded workspaces.",
    ),
  keepWorkspaces: z
    .boolean()
    .default(false)
    .describe(
      "When `workspace` is given: also re-register the other currently loaded projects (as secondary workspaces). Default false = restart with `workspace` alone, which is the fast, clean state for switching repos.",
    ),
});

export type RestartLspInputT = z.infer<typeof RestartLspInput>;

export interface RestartLspResult {
  restarted: true;
  /** LS generation after the restart (1 = the original process). */
  generation: number;
  previous: {
    pid: number | null;
    uptimeMs: number | null;
    workspaceFolders: string[];
    /** Why the old LS was unusable, if it had already died/wedged. */
    deadReason: string | null;
  };
  current: {
    pid: number | null;
    workspaceRoot: string;
    workspaceFolders: string[];
  };
  durationMs: number;
}

/** Throw unless `folder` is an existing directory holding an `app.json`. */
export function assertAlProjectRoot(folder: string): string {
  const abs = resolve(folder);
  if (!existsSync(abs) || !statSync(abs).isDirectory()) {
    throw new Error(`Path is not an existing directory: ${abs}`);
  }
  if (!existsSync(join(abs, "app.json"))) {
    throw new Error(
      `No app.json in ${abs}. Pass the AL project root (the folder containing app.json), not a subfolder or the git root of a multi-app repo.`,
    );
  }
  return abs;
}

export function createRestartLsp(client: AlLspClient, config: BridgeConfig) {
  return async (input: RestartLspInputT): Promise<RestartLspResult> => {
    const t0 = Date.now();
    const startedAtMs = client.getStartedAtMs();
    const previous = {
      pid: client.getLsPid(),
      uptimeMs: startedAtMs !== null ? t0 - startedAtMs : null,
      workspaceFolders: client.getWorkspaceFolders(),
      deadReason: client.getDeadReason(),
    };

    if (input.workspace) {
      const primary = assertAlProjectRoot(input.workspace);
      const others = input.keepWorkspaces
        ? [...previous.workspaceFolders, ...config.workspaceFolders].filter(
            (f) => resolve(f) !== primary,
          )
        : [];
      retargetWorkspaces(config, [primary, ...dedupe(others)]);
    }

    // Deliberately awaited: the caller's next tool call must not race the new
    // generation's `initialize`. The whole point is to hand back a usable LSP.
    await client.restart();
    await client.ready();

    return {
      restarted: true,
      generation: client.getGeneration(),
      previous,
      current: {
        pid: client.getLsPid(),
        workspaceRoot: config.workspaceRoot,
        workspaceFolders: client.getWorkspaceFolders(),
      },
      durationMs: Date.now() - t0,
    };
  };
}

function dedupe(paths: string[]): string[] {
  const seen = new Set<string>();
  const out: string[] = [];
  for (const p of paths) {
    const abs = resolve(p);
    if (seen.has(abs)) continue;
    seen.add(abs);
    out.push(abs);
  }
  return out;
}
