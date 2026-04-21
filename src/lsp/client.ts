import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { readFileSync } from "node:fs";
import { basename, normalize } from "node:path";
import { pathToFileURL } from "node:url";
import {
  createMessageConnection,
  type MessageConnection,
  StreamMessageReader,
  StreamMessageWriter,
} from "vscode-jsonrpc/node.js";
import type {
  ApplyWorkspaceEditParams,
  Diagnostic,
  InitializeParams,
  InitializeResult,
  PublishDiagnosticsParams,
  WorkspaceEdit,
} from "vscode-languageserver-protocol";
import type { BridgeConfig } from "../config.js";
import { DiagnosticsCache } from "./diagnostics.js";

/**
 * Thin LSP client. Owns the child process lifecycle, a JSON-RPC message
 * connection, and an in-memory document buffer that mirrors what has been
 * opened on the server.
 *
 * Uses string method names rather than typed LSP constructors because the
 * vscode-jsonrpc and vscode-languageserver-protocol type hierarchies
 * aren't structurally compatible across packages.
 */
export class AlLspClient {
  private proc: ChildProcessWithoutNullStreams | null = null;
  private conn: MessageConnection | null = null;
  private readonly openVersions = new Map<string, number>();
  readonly diagnostics = new DiagnosticsCache();

  // The AL LS returns code-action results via a reverse `workspace/applyEdit`
  // request keyed by the action's identifier (placed in `label`). Callers of
  // `al/runCodeAction` register here to capture the inbound edit.
  private readonly applyEditWaiters = new Map<string, (e: WorkspaceEdit) => void>();

  constructor(private readonly config: BridgeConfig) {}

  awaitApplyEdit(label: string, timeoutMs: number): Promise<WorkspaceEdit> {
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        this.applyEditWaiters.delete(label);
        reject(new Error(`Timed out waiting for workspace/applyEdit label='${label}'`));
      }, timeoutMs);
      this.applyEditWaiters.set(label, (edit) => {
        clearTimeout(timer);
        this.applyEditWaiters.delete(label);
        resolve(edit);
      });
    });
  }

  async start(): Promise<InitializeResult> {
    if (this.conn) throw new Error("LSP client already started");

    const proc = spawn(normalize(this.config.languageServerPath), [], {
      stdio: ["pipe", "pipe", "pipe"],
      cwd: normalize(this.config.workspaceRoot),
    });
    proc.stderr.on("data", (b) => process.stderr.write(`[al-ls] ${b}`));
    proc.on("error", (err) => {
      process.stderr.write(
        `[al-mcp-bridge] LS process error: ${err.message}\n`,
      );
      process.exit(1);
    });
    proc.on("exit", (code, signal) => {
      if (this.conn) {
        process.stderr.write(
          `[al-mcp-bridge] LS exited (code=${code} signal=${signal})\n`,
        );
      }
    });
    this.proc = proc;

    const conn = createMessageConnection(
      new StreamMessageReader(proc.stdout),
      new StreamMessageWriter(proc.stdin),
    );
    this.conn = conn;

    conn.onNotification("textDocument/publishDiagnostics", (p: PublishDiagnosticsParams) =>
      this.diagnostics.ingest(p),
    );
    // The LS uses the LSP reverse-request channel for code-action payloads —
    // `al/runCodeAction` responds with an empty message, then sends
    // `workspace/applyEdit` back at us. Always ack with applied:true so the
    // LS considers the action complete; dispatch by `label` to any waiter.
    conn.onRequest("workspace/applyEdit", (p: ApplyWorkspaceEditParams) => {
      const label = p.label ?? "";
      const waiter = this.applyEditWaiters.get(label);
      if (waiter) waiter(p.edit);
      return { applied: true };
    });
    conn.listen();

    const initParams: InitializeParams = {
      processId: process.pid,
      rootUri: pathToFileURL(this.config.workspaceRoot).toString(),
      capabilities: {
        textDocument: {
          synchronization: { dynamicRegistration: false, didSave: true },
          publishDiagnostics: { relatedInformation: true },
          diagnostic: { dynamicRegistration: false, relatedDocumentSupport: false },
          rename: { dynamicRegistration: false, prepareSupport: true },
          codeAction: { dynamicRegistration: false },
          formatting: { dynamicRegistration: false },
        },
        workspace: { applyEdit: true, workspaceEdit: { documentChanges: true } },
      },
      workspaceFolders: this.config.workspaceFolders.map((p) => ({
        uri: pathToFileURL(p).toString(),
        name: basename(p) || "workspace",
      })),
      initializationOptions: {
        packageCachePaths: this.config.packageCachePaths,
      },
    };

    const result = await conn.sendRequest<InitializeResult>("initialize", initParams);
    await conn.sendNotification("initialized", {});

    // Critical: stock initialize doesn't load the AL project — the LS expects
    // a follow-up `al/setActiveWorkspace` with Settings/ALResourceConfiguration
    // for every AL project we care about. The first call is marked as the
    // active one; subsequent calls register additional projects so
    // workspace/symbol, al/getApplicationObjects, etc. see them.
    //
    // Also auto-populate `assemblyProbingPaths` with the parent directory of
    // every configured analyzer DLL. Third-party analyzers (LinterCop, ALCops)
    // commonly depend on sibling assemblies shipped in the same folder (e.g.
    // `Microsoft.Dynamics.Nav.Analyzers.Common.dll`). Without a probing path
    // the CLR fails to resolve those at first use, the `DiagnosticAnalyzer`
    // throws `FileNotFoundException`, and the LS surfaces it as `AD0001` on
    // `app.json` — diagnostics then never arrive for the analyzer's rules.
    for (let i = 0; i < this.config.workspaceFolders.length; i++) {
      const folder = this.config.workspaceFolders[i]!;
      await conn.sendRequest("al/setActiveWorkspace", {
        settings: {
          workspacePath: folder,
          setActiveWorkspace: i === 0,
          alResourceConfigurationSettings: {
            packageCachePaths: this.config.packageCachePaths,
            assemblyProbingPaths: this.config.assemblyProbingPaths,
            enableCodeAnalysis: this.config.enableCodeAnalysis,
            enableCodeActions: this.config.enableCodeActions,
            incrementalBuild: true,
            codeAnalyzers: this.config.codeAnalyzers,
            backgroundCodeAnalysis: this.config.backgroundCodeAnalysis,
            ...(this.config.ruleSetPath ? { ruleSetPath: this.config.ruleSetPath } : {}),
          },
        },
      });
    }
    return result;
  }

  async stop(): Promise<void> {
    this.conn?.dispose();
    this.conn = null;
    this.proc?.kill();
    this.proc = null;
  }

  /** Raw request passthrough — tools forward LSP calls through here. */
  request<R>(method: string, params: unknown): Promise<R> {
    if (!this.conn) throw new Error("LSP client not started");
    return this.conn.sendRequest<R>(method, params);
  }

  /**
   * LSP 3.17 pull diagnostics. Returns the `items` of a full
   * `DocumentDiagnosticReport`, or `null` if the LS doesn't implement
   * `textDocument/diagnostic` (older LS builds) or returned an "unchanged"
   * report. Needed because the MS AL LS appears to route third-party
   * analyzer findings (LinterCop, ALCops) exclusively through pull — they
   * never arrive via `textDocument/publishDiagnostics`, even though their
   * CodeFixProviders are exposed through `textDocument/codeAction`.
   */
  async pullDiagnostics(uri: string, timeoutMs = 2000): Promise<Diagnostic[] | null> {
    if (!this.conn) throw new Error("LSP client not started");
    const conn = this.conn;
    const request = conn
      .sendRequest<{ kind?: string; items?: Diagnostic[] } | null>("textDocument/diagnostic", {
        textDocument: { uri },
      })
      .then((report) => {
        if (!report || report.kind !== "full" || !Array.isArray(report.items)) return null;
        return report.items;
      })
      .catch((err) => {
        // MethodNotFound (-32601), InvalidRequest (-32600) — LS doesn't
        // support pull. Any other error also degrades to "no pull data"
        // rather than failing the whole tool call; the push cache still
        // works.
        const code = (err as { code?: number }).code;
        if (code === -32601 || code === -32600) return null;
        process.stderr.write(
          `[al-mcp-bridge] pullDiagnostics error code=${code ?? "?"} msg=${(err as Error).message}\n`,
        );
        return null;
      });
    const timeout = new Promise<null>((resolve) => setTimeout(() => resolve(null), timeoutMs));
    return Promise.race([request, timeout]);
  }

  notify(method: string, params: unknown): Promise<void> {
    if (!this.conn) throw new Error("LSP client not started");
    return this.conn.sendNotification(method, params);
  }

  /** Idempotently `didOpen` a file on disk. */
  async openDocument(absolutePath: string): Promise<string> {
    const uri = pathToFileURL(absolutePath).toString();
    if (this.openVersions.has(uri)) return uri;

    const text = readFileSync(absolutePath, "utf8");
    await this.notify("textDocument/didOpen", {
      textDocument: { uri, languageId: "al", version: 1, text },
    });
    this.openVersions.set(uri, 1);
    return uri;
  }

  /** Full-document sync (simplest for the MCP use case). Returns new version. */
  async applyTextChange(uri: string, newText: string): Promise<number> {
    const version = (this.openVersions.get(uri) ?? 1) + 1;
    await this.notify("textDocument/didChange", {
      textDocument: { uri, version },
      contentChanges: [{ text: newText }],
    });
    this.openVersions.set(uri, version);
    return version;
  }

  async closeDocument(uri: string): Promise<void> {
    if (!this.openVersions.has(uri)) return;
    await this.notify("textDocument/didClose", { textDocument: { uri } });
    this.openVersions.delete(uri);
  }
}
