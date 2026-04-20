import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { readFileSync } from "node:fs";
import { pathToFileURL } from "node:url";
import {
  createMessageConnection,
  type MessageConnection,
  StreamMessageReader,
  StreamMessageWriter,
} from "vscode-jsonrpc/node.js";
import {
  DidChangeTextDocumentNotification,
  DidCloseTextDocumentNotification,
  DidOpenTextDocumentNotification,
  InitializedNotification,
  InitializeRequest,
  PublishDiagnosticsNotification,
  type InitializeParams,
  type InitializeResult,
} from "vscode-languageserver-protocol";
import type { BridgeConfig } from "../config.js";
import { DiagnosticsCache } from "./diagnostics.js";

/**
 * Thin LSP client. Owns the child process lifecycle, a JSON-RPC message
 * connection, and an in-memory document buffer that mirrors what has been
 * opened on the server.
 *
 * Intentionally narrow: this class is not the place for tool logic. It
 * exposes `request`/`notify`/`openDocument`/`applyTextChange` and each
 * MCP tool composes from there.
 */
export class AlLspClient {
  private proc: ChildProcessWithoutNullStreams | null = null;
  private conn: MessageConnection | null = null;
  private readonly openVersions = new Map<string, number>();
  readonly diagnostics = new DiagnosticsCache();

  constructor(private readonly config: BridgeConfig) {}

  async start(): Promise<InitializeResult> {
    if (this.conn) throw new Error("LSP client already started");

    const proc = spawn(this.config.languageServerPath, [], {
      stdio: ["pipe", "pipe", "pipe"],
      cwd: this.config.workspaceRoot,
    });
    proc.stderr.on("data", (b) => process.stderr.write(`[al-ls] ${b}`));
    this.proc = proc;

    const conn = createMessageConnection(
      new StreamMessageReader(proc.stdout),
      new StreamMessageWriter(proc.stdin),
    );
    this.conn = conn;

    conn.onNotification(PublishDiagnosticsNotification.type, (p) =>
      this.diagnostics.ingest(p),
    );
    conn.listen();

    const initParams: InitializeParams = {
      processId: process.pid,
      rootUri: pathToFileURL(this.config.workspaceRoot).toString(),
      capabilities: {
        textDocument: {
          synchronization: { dynamicRegistration: false, didSave: true },
          publishDiagnostics: { relatedInformation: true },
          rename: { dynamicRegistration: false, prepareSupport: true },
          codeAction: { dynamicRegistration: false },
          formatting: { dynamicRegistration: false },
        },
        workspace: { applyEdit: true, workspaceEdit: { documentChanges: true } },
      },
      workspaceFolders: [
        {
          uri: pathToFileURL(this.config.workspaceRoot).toString(),
          name: "workspace",
        },
      ],
      initializationOptions: {
        packageCachePaths: this.config.packageCachePaths,
      },
    };

    const result = await conn.sendRequest(InitializeRequest.type, initParams);
    await conn.sendNotification(InitializedNotification.type, {});
    return result;
  }

  async stop(): Promise<void> {
    this.conn?.dispose();
    this.conn = null;
    this.proc?.kill();
    this.proc = null;
  }

  /** Raw request passthrough — tools forward LSP calls through here. */
  request<R, E>(method: string, params: unknown): Promise<R> {
    if (!this.conn) throw new Error("LSP client not started");
    return this.conn.sendRequest<R>(method, params);
  }

  notify(method: string, params: unknown): Promise<void> {
    if (!this.conn) throw new Error("LSP client not started");
    return this.conn.sendNotification(method, params);
  }

  /**
   * Idempotently `didOpen` a file on disk. Subsequent edits use
   * `applyTextChange` which bumps the version.
   */
  async openDocument(absolutePath: string): Promise<string> {
    const uri = pathToFileURL(absolutePath).toString();
    if (this.openVersions.has(uri)) return uri;

    const text = readFileSync(absolutePath, "utf8");
    await this.notify(DidOpenTextDocumentNotification.method, {
      textDocument: { uri, languageId: "al", version: 1, text },
    });
    this.openVersions.set(uri, 1);
    return uri;
  }

  /**
   * Push a new full-document text to the server (full sync, simpler than
   * incremental for the MCP use case). Returns the new version.
   */
  async applyTextChange(uri: string, newText: string): Promise<number> {
    const version = (this.openVersions.get(uri) ?? 1) + 1;
    await this.notify(DidChangeTextDocumentNotification.method, {
      textDocument: { uri, version },
      contentChanges: [{ text: newText }],
    });
    this.openVersions.set(uri, version);
    return version;
  }

  async closeDocument(uri: string): Promise<void> {
    if (!this.openVersions.has(uri)) return;
    await this.notify(DidCloseTextDocumentNotification.method, {
      textDocument: { uri },
    });
    this.openVersions.delete(uri);
  }
}
