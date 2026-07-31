/**
 * Central deadline policy for the bridge.
 *
 * Every path that waits on something outside this process - the AL language
 * server child, `alc`, a BC service tier - must be bounded here. An unbounded
 * await on a stdio JSON-RPC connection is invisible: the MCP client sees a
 * tool call that simply never returns, and the only recovery is killing the
 * whole Claude session. A rejected promise with a named operation and the
 * elapsed budget is always preferable.
 */

export interface BridgeTimeouts {
  /** Per LSP request (`textDocument/*`, `al/*`, workspace/symbol, ...). */
  lspRequestMs: number;
  /** `initialize` + each `al/setActiveWorkspace` during start/restart. */
  lspInitMs: number;
  /** How long a tool call may wait for LSP startup to complete. */
  lspReadyMs: number;
  /** Outer guard around any single MCP tool handler. */
  toolMs: number;
  /** `alc` child process wall clock. */
  compileMs: number;
  /** `.app` upload to the dev service tier. */
  publishMs: number;
  /** One `al_run_tests` invocation, including the wait for its hub slot. */
  runTestsMs: number;
}

const DEFAULTS: BridgeTimeouts = {
  lspRequestMs: 60_000,
  lspInitMs: 120_000,
  lspReadyMs: 150_000,
  toolMs: 180_000,
  compileMs: 600_000,
  publishMs: 300_000,
  runTestsMs: 900_000,
};

const ENV_KEYS: Record<keyof BridgeTimeouts, string> = {
  lspRequestMs: "AL_BRIDGE_LSP_REQUEST_TIMEOUT_MS",
  lspInitMs: "AL_BRIDGE_LSP_INIT_TIMEOUT_MS",
  lspReadyMs: "AL_BRIDGE_LSP_READY_TIMEOUT_MS",
  toolMs: "AL_BRIDGE_TOOL_TIMEOUT_MS",
  compileMs: "AL_BRIDGE_COMPILE_TIMEOUT_MS",
  publishMs: "AL_BRIDGE_PUBLISH_TIMEOUT_MS",
  runTestsMs: "AL_BRIDGE_RUN_TESTS_TIMEOUT_MS",
};

/**
 * Resolve the effective timeouts from env, falling back to `DEFAULTS`.
 * A value of `0` disables that particular deadline - an escape hatch for
 * debugging a genuinely slow project, not a normal configuration.
 */
export function loadTimeouts(env: NodeJS.ProcessEnv = process.env): BridgeTimeouts {
  const out = { ...DEFAULTS };
  for (const key of Object.keys(ENV_KEYS) as Array<keyof BridgeTimeouts>) {
    const raw = env[ENV_KEYS[key]];
    if (raw === undefined || raw.trim() === "") continue;
    const n = Number(raw);
    if (!Number.isFinite(n) || n < 0) continue;
    out[key] = n;
  }
  return out;
}

export class TimeoutError extends Error {
  constructor(
    readonly operation: string,
    readonly timeoutMs: number,
  ) {
    super(
      `Timed out after ${timeoutMs}ms: ${operation}. ` +
        `The AL language server did not answer in time - it is likely still ` +
        `indexing or wedged. Call al_lsp_status to inspect it, or al_restart_lsp ` +
        `to respawn it for this workspace.`,
    );
    this.name = "TimeoutError";
  }
}

/**
 * Reject with a `TimeoutError` if `promise` hasn't settled within `timeoutMs`.
 *
 * `onTimeout` runs exactly once when the deadline fires - used to cancel the
 * underlying JSON-RPC request or kill a child process, so a timeout doesn't
 * leave work running invisibly behind the caller's back.
 *
 * `timeoutMs <= 0` disables the deadline and returns `promise` unchanged.
 */
export function withTimeout<T>(
  promise: Promise<T>,
  timeoutMs: number,
  operation: string,
  onTimeout?: () => void,
): Promise<T> {
  if (!Number.isFinite(timeoutMs) || timeoutMs <= 0) return promise;
  return new Promise<T>((resolve, reject) => {
    const timer = setTimeout(() => {
      try {
        onTimeout?.();
      } catch {
        // Cancellation is best-effort; never mask the timeout itself.
      }
      reject(new TimeoutError(operation, timeoutMs));
    }, timeoutMs);
    promise.then(
      (v) => {
        clearTimeout(timer);
        resolve(v);
      },
      (e) => {
        clearTimeout(timer);
        reject(e);
      },
    );
  });
}
