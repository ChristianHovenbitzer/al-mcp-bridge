import type { Diagnostic, PublishDiagnosticsParams } from "vscode-languageserver-protocol";

/**
 * Caches the last `publishDiagnostics` per URI and lets tools await the
 * next publish after an edit. The AL LS sends diagnostics asynchronously
 * after `didChange`, so any "edit → validate" tool has to bridge that gap.
 */
export class DiagnosticsCache {
  private readonly byUri = new Map<string, Diagnostic[]>();
  private readonly waiters = new Map<string, Array<(d: Diagnostic[]) => void>>();

  ingest(params: PublishDiagnosticsParams): void {
    this.byUri.set(params.uri, params.diagnostics);
    if (process.env.AL_BRIDGE_DEBUG_DIAGS) {
      const codes = params.diagnostics.map((d) => String(d.code ?? "?")).join(",");
      process.stderr.write(
        `[al-mcp-bridge] publishDiagnostics uri=${params.uri} n=${params.diagnostics.length} codes=[${codes}]\n`,
      );
    }
    const waiting = this.waiters.get(params.uri);
    if (waiting && waiting.length) {
      this.waiters.set(params.uri, []);
      for (const w of waiting) w(params.diagnostics);
    }
  }

  /** Debug: snapshot of every URI currently cached and its diagnostic count. */
  snapshotSummary(): Array<{ uri: string; count: number; codes: string[] }> {
    return Array.from(this.byUri.entries()).map(([uri, ds]) => ({
      uri,
      count: ds.length,
      codes: ds.map((d) => String(d.code ?? "?")),
    }));
  }

  current(uri: string): Diagnostic[] {
    return this.byUri.get(uri) ?? [];
  }

  hasPublishedFor(uri: string): boolean {
    return this.byUri.has(uri);
  }

  /**
   * Resolve with the next `publishDiagnostics` for `uri`, or with the
   * currently cached set if one doesn't arrive within `timeoutMs`.
   */
  awaitNext(uri: string, timeoutMs: number): Promise<Diagnostic[]> {
    return new Promise((resolve) => {
      const arr = this.waiters.get(uri) ?? [];
      arr.push(resolve);
      this.waiters.set(uri, arr);
      setTimeout(() => {
        const bucket = this.waiters.get(uri);
        if (bucket) {
          const i = bucket.indexOf(resolve);
          if (i >= 0) bucket.splice(i, 1);
        }
        resolve(this.current(uri));
      }, timeoutMs);
    });
  }
}
