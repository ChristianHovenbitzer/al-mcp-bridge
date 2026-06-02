import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { readFileSync, statSync } from "node:fs";
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
import type { AlWorkspaceSettings, BridgeConfig } from "../config.js";
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
  /**
   * Text the LS currently holds for each open URI — i.e. what we last sent
   * via didOpen/didChange. The LS computes diagnostics against this buffer,
   * not the file on disk, so we compare against it on every `openDocument`
   * to detect out-of-band edits (Claude's Edit/Write tool, the user's
   * editor, git checkout) and re-sync. Without this the buffer freezes at
   * first open and diagnostics go stale after any edit not routed through
   * `applyTextChange`.
   */
  private readonly syncedText = new Map<string, string>();
  /**
   * Live list of workspace roots known to the LSP. Mirrors the LS-side
   * registration: every path here has been the subject of a successful
   * `workspace/didChangeWorkspaceFolders` (added) and `al/setActiveWorkspace`
   * pair. Initialized from `config.workspaceFolders` during `start()` and
   * mutated by `addWorkspace()`.
   */
  private readonly activeWorkspaces: string[] = [];
  readonly diagnostics = new DiagnosticsCache();

  /** Epoch ms when the LS child process was spawned. Null before `start()`. */
  private startedAtMs: number | null = null;
  /** Counters for pull-diagnostics traffic; exposed via `getStats()`. */
  private pullStats = {
    calls: 0,
    nullResults: 0,
    methodNotFound: 0,
    errors: 0,
    lastErrorCode: undefined as number | undefined,
    lastErrorMessage: undefined as string | undefined,
  };

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
    this.startedAtMs = Date.now();
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
      const settings = this.config.workspaceSettings.get(folder);
      if (!settings) {
        // Defensive: loadConfig() always populates this map, but guard so
        // a future caller passing a hand-built BridgeConfig fails loudly.
        throw new Error(`No workspaceSettings entry for ${folder}`);
      }
      await this.sendActiveWorkspaceRequest(conn, settings, i === 0);
      this.activeWorkspaces.push(folder);
    }
    return result;
  }

  /** Snapshot of LS-known workspace roots (absolute paths). */
  getWorkspaceFolders(): string[] {
    return [...this.activeWorkspaces];
  }

  /**
   * Register a new AL workspace at runtime. Idempotent: re-adding an existing
   * folder is a no-op. The new folder is announced to the LS via
   * `workspace/didChangeWorkspaceFolders`, then primed with
   * `al/setActiveWorkspace` carrying its own analyzer / ruleset settings.
   *
   * `setActive` controls the `setActiveWorkspace` flag on the AL request:
   * when true, this folder becomes the LS's "active" project; when false,
   * it is registered alongside the existing active workspace.
   */
  async addWorkspace(
    folder: string,
    settings: AlWorkspaceSettings,
    setActive: boolean,
  ): Promise<{ added: boolean }> {
    if (!this.conn) throw new Error("LSP client not started");
    if (this.activeWorkspaces.includes(folder)) {
      return { added: false };
    }

    const conn = this.conn;
    await conn.sendNotification("workspace/didChangeWorkspaceFolders", {
      event: {
        added: [
          {
            uri: pathToFileURL(folder).toString(),
            name: basename(folder) || "workspace",
          },
        ],
        removed: [],
      },
    });

    await this.sendActiveWorkspaceRequest(conn, settings, setActive);
    this.activeWorkspaces.push(folder);
    return { added: true };
  }

  private async sendActiveWorkspaceRequest(
    conn: MessageConnection,
    settings: AlWorkspaceSettings,
    setActive: boolean,
  ): Promise<void> {
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
    logActiveWorkspacePayload(settings, setActive);

    await conn.sendRequest("al/setActiveWorkspace", {
      settings: {
        workspacePath: settings.workspaceRoot,
        setActiveWorkspace: setActive,
        alResourceConfigurationSettings: {
          packageCachePaths: this.config.packageCachePaths,
          assemblyProbingPaths: settings.assemblyProbingPaths,
          enableCodeAnalysis: settings.enableCodeAnalysis,
          enableCodeActions: settings.enableCodeActions,
          incrementalBuild: true,
          codeAnalyzers: settings.codeAnalyzers,
          backgroundCodeAnalysis: settings.backgroundCodeAnalysis,
          ...(settings.ruleSetPath ? { ruleSetPath: settings.ruleSetPath } : {}),
        },
      },
    });
  }

  /** Epoch ms when the LS child was spawned, or null if not started. */
  getStartedAtMs(): number | null {
    return this.startedAtMs;
  }

  /** PID of the LS child process, or null if not started. */
  getLsPid(): number | null {
    return this.proc?.pid ?? null;
  }

  /** Snapshot of currently opened document URIs and their LSP versions. */
  getOpenDocuments(): Array<{ uri: string; version: number }> {
    return Array.from(this.openVersions.entries()).map(([uri, version]) => ({
      uri,
      version,
    }));
  }

  /** Snapshot of pull-diagnostics traffic counters since LS start. */
  getPullDiagnosticsStats(): {
    calls: number;
    nullResults: number;
    methodNotFound: number;
    errors: number;
    lastErrorCode?: number;
    lastErrorMessage?: string;
  } {
    return { ...this.pullStats };
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
    this.pullStats.calls++;
    const request = conn
      .sendRequest<{ kind?: string; items?: Diagnostic[] } | null>("textDocument/diagnostic", {
        textDocument: { uri },
      })
      .then((report) => {
        if (!report || report.kind !== "full" || !Array.isArray(report.items)) {
          this.pullStats.nullResults++;
          return null;
        }
        return report.items;
      })
      .catch((err) => {
        // MethodNotFound (-32601), InvalidRequest (-32600) — LS doesn't
        // support pull. Any other error also degrades to "no pull data"
        // rather than failing the whole tool call; the push cache still
        // works.
        const code = (err as { code?: number }).code;
        const message = (err as Error).message;
        if (code === -32601 || code === -32600) {
          this.pullStats.methodNotFound++;
          this.pullStats.lastErrorCode = code;
          this.pullStats.lastErrorMessage = message;
          return null;
        }
        this.pullStats.errors++;
        this.pullStats.lastErrorCode = code;
        this.pullStats.lastErrorMessage = message;
        process.stderr.write(
          `[al-mcp-bridge] pullDiagnostics error code=${code ?? "?"} msg=${message}\n`,
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

  /**
   * Open a file on the LS, re-syncing from disk if it's already open.
   *
   * Every tool calls this on entry, so it's the natural place to keep the
   * LS buffer in step with disk. First call sends `didOpen`; later calls
   * read disk and, if the content diverged from what the LS holds, push a
   * `didChange`. This is what makes diagnostics reflect edits made outside
   * the bridge (Claude's Edit/Write tool, the editor, git) rather than
   * freezing at the text seen at first open.
   */
  async openDocument(absolutePath: string): Promise<string> {
    const uri = pathToFileURL(absolutePath).toString();
    const text = readFileSync(absolutePath, "utf8");

    if (!this.openVersions.has(uri)) {
      await this.notify("textDocument/didOpen", {
        textDocument: { uri, languageId: "al", version: 1, text },
      });
      this.openVersions.set(uri, 1);
      this.syncedText.set(uri, text);
      return uri;
    }

    if (this.syncedText.get(uri) !== text) {
      await this.applyTextChange(uri, text);
    }
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
    this.syncedText.set(uri, newText);
    return version;
  }

  async closeDocument(uri: string): Promise<void> {
    if (!this.openVersions.has(uri)) return;
    await this.notify("textDocument/didClose", { textDocument: { uri } });
    this.openVersions.delete(uri);
    this.syncedText.delete(uri);
  }
}

/**
 * Emit a stderr summary of the analyzer DLLs about to be loaded for this
 * workspace. The CLR pins these assemblies for the LS process lifetime, so
 * `mtimeMs` here is the moment of truth: if the analyzer DLL on disk is
 * newer than the LS process start time (and especially newer than this
 * log line), the LS is still running the older copy and won't see new
 * rules until restart. Always-on (not gated by AL_BRIDGE_DEBUG_DIAGS)
 * because this is the single most useful line for diagnosing "VSCode sees
 * a finding, the bridge doesn't".
 */
function logActiveWorkspacePayload(
  settings: AlWorkspaceSettings,
  setActive: boolean,
): void {
  const flag = setActive ? "active" : "secondary";
  process.stderr.write(
    `[al-mcp-bridge] al/setActiveWorkspace ${flag} workspace=${settings.workspaceRoot} ` +
      `enableCodeAnalysis=${settings.enableCodeAnalysis} ` +
      `backgroundCodeAnalysis=${String(settings.backgroundCodeAnalysis)} ` +
      `ruleSet=${settings.ruleSetPath ?? "(none)"} ` +
      `analyzers=${settings.codeAnalyzers.length}\n`,
  );
  for (const dll of settings.codeAnalyzers) {
    let info = "MISSING";
    try {
      const st = statSync(dll);
      info = `mtime=${new Date(st.mtimeMs).toISOString()} size=${st.size}`;
    } catch {
      // info stays "MISSING"
    }
    process.stderr.write(`[al-mcp-bridge]   analyzer: ${dll} ${info}\n`);
  }
}
