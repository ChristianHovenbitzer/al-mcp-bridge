/**
 * al_lsp_status — introspection tool that returns the full diagnostic
 * picture of the running LS in a single MCP call: process info, registered
 * workspaces with their resolved analyzers (paths + on-disk mtime/size),
 * open documents, the push-diagnostics cache, and pull-diagnostics
 * traffic counters.
 *
 * Primary use case: diagnose "VSCode shows diagnostics, the bridge
 * doesn't". The mtime of each analyzer DLL, compared against the LS
 * process start time, reveals whether the LS is running a stale
 * assembly (CLR can't unload them — only a process restart helps).
 */
import { existsSync, statSync } from "node:fs";
import { z } from "zod";
import type { BridgeConfig } from "../config.js";
import type { AlLspClient } from "../lsp/client.js";

export const LspStatusInput = z.object({}).strict();
export type LspStatusInputT = z.infer<typeof LspStatusInput>;

export interface AnalyzerStatus {
  path: string;
  exists: boolean;
  /** ISO-8601 mtime of the DLL on disk, or null if missing. */
  mtime: string | null;
  /** File size in bytes, or null if missing. */
  size: number | null;
  /**
   * True when the DLL on disk was modified after the LS process was
   * spawned. The CLR pins assemblies for the process lifetime, so a
   * "true" here means the LS is running a stale copy and won't see
   * any rules added after that mtime until restart.
   */
  newerThanLsStart: boolean;
}

export interface WorkspaceStatus {
  path: string;
  isPrimary: boolean;
  enableCodeAnalysis: boolean;
  enableCodeActions: boolean;
  backgroundCodeAnalysis: string | boolean;
  ruleSetPath?: string;
  ruleSetExists?: boolean;
  codeAnalyzers: AnalyzerStatus[];
}

export interface LspStatusResult {
  ls: {
    languageServerPath: string;
    pid: number | null;
    startedAt: string | null;
    uptimeMs: number | null;
  };
  workspaces: WorkspaceStatus[];
  openDocuments: Array<{ uri: string; version: number }>;
  diagnosticsCache: Array<{ uri: string; count: number; codes: string[] }>;
  pullDiagnostics: ReturnType<AlLspClient["getPullDiagnosticsStats"]>;
  /**
   * Diagnostic flags surfaced for quick inspection. Each entry is a short
   * machine-readable code plus a human-readable hint. Empty array means
   * nothing obviously wrong.
   */
  warnings: Array<{ code: string; message: string }>;
}

export function createLspStatus(client: AlLspClient, config: BridgeConfig) {
  return async (_input: LspStatusInputT): Promise<LspStatusResult> => {
    const startedAtMs = client.getStartedAtMs();
    const warnings: LspStatusResult["warnings"] = [];

    const workspaces: WorkspaceStatus[] = client.getWorkspaceFolders().map((folder) => {
      const s = config.workspaceSettings.get(folder);
      const ruleSetPath = s?.ruleSetPath;
      const ruleSetExists =
        ruleSetPath !== undefined ? existsSync(ruleSetPath) : undefined;

      if (ruleSetPath && ruleSetExists === false) {
        warnings.push({
          code: "ruleset-missing",
          message: `Configured ruleSetPath does not exist on disk: ${ruleSetPath} (workspace ${folder}). The LS will silently ignore the ruleset and run all enabled rules at default severity.`,
        });
      }

      if (s && s.enableCodeAnalysis === false) {
        warnings.push({
          code: "code-analysis-disabled",
          message: `al.enableCodeAnalysis is false for ${folder}; the LS will not run analyzers for this workspace.`,
        });
      }

      if (s && s.backgroundCodeAnalysis === "None") {
        warnings.push({
          code: "background-analysis-none",
          message: `al.backgroundCodeAnalysis is "None" for ${folder}; the LS will not produce diagnostics via publish/pull until a file is explicitly built.`,
        });
      }

      const analyzers: AnalyzerStatus[] = (s?.codeAnalyzers ?? []).map((p) => {
        let exists = false;
        let mtime: string | null = null;
        let size: number | null = null;
        try {
          const st = statSync(p);
          exists = true;
          mtime = new Date(st.mtimeMs).toISOString();
          size = st.size;
          if (startedAtMs !== null && st.mtimeMs > startedAtMs) {
            warnings.push({
              code: "stale-analyzer",
              message: `Analyzer DLL on disk is newer than the LS process: ${p}. The LS is still running the version it loaded at startup — restart the bridge to pick up the new rules.`,
            });
          }
        } catch {
          warnings.push({
            code: "analyzer-missing",
            message: `Configured analyzer DLL not found on disk: ${p} (workspace ${folder}). The LS will log AD0001 / FileNotFoundException and skip its rules.`,
          });
        }
        return {
          path: p,
          exists,
          mtime,
          size,
          newerThanLsStart:
            startedAtMs !== null && mtime !== null
              ? Date.parse(mtime) > startedAtMs
              : false,
        };
      });

      return {
        path: folder,
        isPrimary: folder === config.workspaceRoot,
        enableCodeAnalysis: s?.enableCodeAnalysis ?? false,
        enableCodeActions: s?.enableCodeActions ?? false,
        backgroundCodeAnalysis: s?.backgroundCodeAnalysis ?? "(unset)",
        ruleSetPath,
        ruleSetExists,
        codeAnalyzers: analyzers,
      };
    });

    const cache = client.diagnostics.snapshotSummary();
    if (cache.length === 0 && client.getOpenDocuments().length > 0) {
      warnings.push({
        code: "empty-cache-with-open-docs",
        message:
          "Documents are open but the diagnostics cache is empty. The LS has not pushed publishDiagnostics for any file — analyzer pass may not be running. Check backgroundCodeAnalysis and enableCodeAnalysis.",
      });
    }

    return {
      ls: {
        languageServerPath: config.languageServerPath,
        pid: client.getLsPid(),
        startedAt: startedAtMs !== null ? new Date(startedAtMs).toISOString() : null,
        uptimeMs: startedAtMs !== null ? Date.now() - startedAtMs : null,
      },
      workspaces,
      openDocuments: client.getOpenDocuments(),
      diagnosticsCache: cache,
      pullDiagnostics: client.getPullDiagnosticsStats(),
      warnings,
    };
  };
}
