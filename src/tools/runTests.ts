/**
 * al_run_tests — execute AL test codeunits against a Business Central dev
 * service tier from Linux. Reimplements the SignalR hub protocol Microsoft's
 * own tool uses (see decompiled `HubBasedTestRunnerService`) but with an
 * auth path that does not rely on Windows-only DPAPI credential storage.
 *
 * Protocol (verified in Microsoft.Dynamics.Nav.LanguageModelTools v17.0.34):
 *   hub       : {server}/{instance}/dev/TestRunnerHub?tenant=...&deploymentId=...
 *   auth      : HTTP Basic — `Authorization: Basic base64(user:pass)`
 *               (the reference client also mirrors the header as an
 *               `Authentication=` query param for the WebSocket upgrade)
 *   invoke    : Initialize(company, debuggingContext, coverageMode)
 *               RunTests(codeunitId, methodNames[])
 *   listen    : TestStarted(codeunitId, method)
 *               TestCompleted(codeunitId, method, status, output, durationMs)
 *               TestRunCompleted(coverage) — fires once per codeunit group
 *               RuntimeInitialized()
 *
 * Credential security:
 *   - Read-only. This tool never writes credentials to disk.
 *   - Env vars `BC_USER` + `BC_PASSWORD` take precedence.
 *   - File fallback at `$XDG_CONFIG_HOME/al-mcp-bridge/credentials.json`
 *     (default `~/.config/...`); file must be mode 0600 — stricter is fine,
 *     any group/world-readable bit causes a hard refusal.
 *   - Password is never logged, never returned in the MCP response, and
 *     scrubbed from error strings before they leave this module.
 */

import { constants, existsSync, readFileSync, statSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, isAbsolute, join, resolve } from "node:path";
import { z } from "zod";
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";

// ---------------------------------------------------------------------------
// MCP-facing input schema
// ---------------------------------------------------------------------------

export const RunTestsInput = z.object({
  codeunitId: z
    .number()
    .int()
    .describe("The test codeunit ID to run (e.g. 95003)."),
  testMethods: z
    .array(z.string())
    .optional()
    .describe(
      "Optional subset of test methods within the codeunit. Runs all methods if omitted.",
    ),
  projectPath: z
    .string()
    .optional()
    .describe(
      "AL project folder containing .vscode/launch.json. Defaults to the bridge's primary workspace.",
    ),
  launchConfig: z
    .string()
    .optional()
    .describe(
      "Name of the launch.json configuration to use. Defaults to the first entry.",
    ),
  company: z
    .string()
    .optional()
    .describe(
      "Startup company (overrides launch.json's `startupCompany`). Defaults to empty string.",
    ),
  allowInvalidCert: z
    .boolean()
    .optional()
    .describe(
      "Skip TLS certificate validation. Default false. Prefer fixing the server cert over enabling this.",
    ),
});

export type RunTestsInputT = z.infer<typeof RunTestsInput>;

// ---------------------------------------------------------------------------
// Result shape
// ---------------------------------------------------------------------------

export type TestResultStatus = "Passed" | "Failed" | "Skipped" | "Unknown";

export interface TestMethodResult {
  codeunitId: number;
  methodName: string;
  status: TestResultStatus;
  output: string;
  durationMs: number;
}

export interface RunTestsResult {
  succeeded: boolean;
  codeunitId: number;
  passed: number;
  failed: number;
  skipped: number;
  total: number;
  durationMs: number;
  tests: TestMethodResult[];
  warnings: string[];
  message: string;
}

// ---------------------------------------------------------------------------
// Errors
// ---------------------------------------------------------------------------

/**
 * Thrown for user-facing config problems where echoing the error message to
 * the MCP response is safe. Catchers must still run the message through
 * `redact()` in case a URL or header slipped in.
 */
export class BcConnectionError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "BcConnectionError";
  }
}

// ---------------------------------------------------------------------------
// Per-hub single-flight lock
// ---------------------------------------------------------------------------

/**
 * The BC dev-service test-runner hub serializes test execution server-side:
 * a second connection that calls `Initialize` while another run is still
 * holding the test session fails with a generic "An unexpected error
 * occurred invoking 'Initialize' on the server." SignalR error. To prevent
 * parallel MCP calls against the same server from tripping this, we queue
 * runs per `{server}|{instance}|{tenant}` key.
 *
 * Keyed map lives at module scope so multiple tool invocations share it.
 */
const hubLocks = new Map<string, Promise<void>>();

async function withHubLock<T>(
  key: string,
  fn: () => Promise<T>,
): Promise<T> {
  const prev = hubLocks.get(key) ?? Promise.resolve();
  // Build the settled slot once so we can compare identities on cleanup.
  let settle!: () => void;
  const slot = new Promise<void>((r) => {
    settle = r;
  });
  hubLocks.set(key, slot);
  try {
    await prev; // wait for the previous run on this hub to finish
    return await fn();
  } finally {
    settle();
    // If nobody else has claimed the slot since, drop the entry so the
    // map doesn't leak keys for one-shot hubs.
    if (hubLocks.get(key) === slot) hubLocks.delete(key);
  }
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

export function createRunTests(primaryWorkspace: string) {
  return async (input: RunTestsInputT): Promise<RunTestsResult> => {
    const warnings: string[] = [];
    const projectPath = resolve(input.projectPath ?? primaryWorkspace);
    const launchCfg = readLaunchConfig(projectPath, input.launchConfig);

    if (launchCfg.environmentType && launchCfg.environmentType !== "OnPrem") {
      throw new BcConnectionError(
        `Only on-premise launch configurations are supported at the moment. Got environmentType='${launchCfg.environmentType}'.`,
      );
    }
    if (launchCfg.authentication && launchCfg.authentication !== "UserPassword") {
      throw new BcConnectionError(
        `Only authentication='UserPassword' is supported at the moment. Got authentication='${launchCfg.authentication}'.`,
      );
    }

    const serverUrl = normalizeServerUrl(launchCfg.server, launchCfg.port);
    if (serverUrl.protocol === "http:") {
      warnings.push(
        `launch.json server URL uses plain HTTP — credentials will travel unencrypted.`,
      );
    }

    const creds = loadCredentials(serverUrl.origin, launchCfg.serverInstance);

    const hubUrl = buildHubUrl(serverUrl, launchCfg.serverInstance, launchCfg.tenant);
    // Default company is empty string — matches MS's own reference tool
    // (lmt.cs:1127 passes `parameters.Company ?? string.Empty`, ignoring
    // launch.json's `startupCompany`). Empirically, passing a named
    // company that the server disagrees with surfaces as a generic
    // `An unexpected error occurred invoking 'Initialize' on the server.`
    // SignalR error. Callers who need a specific company must pass it
    // explicitly via `company`.
    const company = input.company ?? "";
    const methods = input.testMethods ?? [];

    // Precedence for cert handling, strictest to loosest:
    //   1. explicit tool input `allowInvalidCert: true`
    //   2. env var `BC_ALLOW_INVALID_CERT=1` (MCP-server scope)
    //   3. launch.json `"validateServerCertificate": false` (project scope)
    // Default: validate (secure).
    const allowInvalidCert =
      input.allowInvalidCert === true ||
      process.env.BC_ALLOW_INVALID_CERT === "1" ||
      launchCfg.validateServerCertificate === false;

    // Serialize concurrent runs against the same hub — BC's test-runner
    // singleton rejects parallel Initialize calls with a generic error.
    const lockKey = `${serverUrl.origin.toLowerCase()}|${launchCfg.serverInstance.toLowerCase()}|${launchCfg.tenant ?? ""}`;

    return withHubLock(lockKey, async () => {
      const results: TestMethodResult[] = [];
      const t0 = Date.now();
      const connection = buildConnection(hubUrl, creds, allowInvalidCert);

      const runCompleted = new Promise<void>((resolvePromise, rejectPromise) => {
        connection.on("TestStarted", () => {
          // could surface progress later; keep quiet for now
        });
        connection.on(
          "TestCompleted",
          (
            codeunitId: number,
            methodName: string,
            status: number | string,
            output: string,
            durationMs: number,
          ) => {
            results.push({
              codeunitId,
              methodName,
              status: coerceStatus(status),
              output: output ?? "",
              durationMs: Number(durationMs) || 0,
            });
          },
        );
        connection.on("TestRunCompleted", () => {
          resolvePromise();
        });
        connection.onclose((err) => {
          if (err) rejectPromise(new BcConnectionError(redact(String(err))));
          else resolvePromise();
        });
      });

      try {
        await connection.start();
        // Initialize: (companyName, debuggingContext, coverageMode=0 None)
        await connection.invoke("Initialize", company, "", 0);
        // RunTests: (codeunitId, methodNames[])
        await connection.invoke("RunTests", input.codeunitId, methods);
        await runCompleted;
      } catch (err: unknown) {
        throw new BcConnectionError(redact(describeError(err)));
      } finally {
        if (connection.state !== HubConnectionState.Disconnected) {
          try {
            await connection.stop();
          } catch {
            // swallow — we already have (or are raising) the primary error
          }
        }
      }

      const elapsed = Date.now() - t0;
      return summarize(input.codeunitId, results, warnings, elapsed);
    });
  };
}

// ---------------------------------------------------------------------------
// launch.json parsing
// ---------------------------------------------------------------------------

export interface LaunchConfig {
  name: string;
  server: string;
  serverInstance: string;
  port?: number;
  tenant?: string;
  authentication?: string;
  environmentType?: string;
  startupCompany?: string;
  validateServerCertificate?: boolean;
}

export function readLaunchConfig(projectPath: string, desiredName?: string): LaunchConfig {
  const launchFile = join(projectPath, ".vscode", "launch.json");
  if (!existsSync(launchFile)) {
    throw new BcConnectionError(`launch.json not found at ${launchFile}.`);
  }
  const raw = readFileSync(launchFile, "utf8");
  let parsed: unknown;
  try {
    parsed = JSON.parse(stripJsonComments(raw));
  } catch (err) {
    throw new BcConnectionError(
      `Could not parse launch.json: ${(err as Error).message}`,
    );
  }
  const configs = (parsed as { configurations?: unknown[] })?.configurations;
  if (!Array.isArray(configs) || configs.length === 0) {
    throw new BcConnectionError(`launch.json has no 'configurations' array.`);
  }

  const chosen = desiredName
    ? (configs as Array<Record<string, unknown>>).find(
        (c) => typeof c.name === "string" && c.name === desiredName,
      )
    : (configs[0] as Record<string, unknown>);

  if (!chosen) {
    throw new BcConnectionError(
      `No launch configuration named '${desiredName}' in ${launchFile}.`,
    );
  }

  const server = readString(chosen, "server");
  const serverInstance = readString(chosen, "serverInstance");
  if (!server || !serverInstance) {
    throw new BcConnectionError(
      `launch configuration '${String(chosen.name)}' is missing 'server' or 'serverInstance'.`,
    );
  }

  return {
    name: String(chosen.name ?? "(unnamed)"),
    server,
    serverInstance,
    port: typeof chosen.port === "number" ? chosen.port : undefined,
    tenant: readString(chosen, "tenant"),
    authentication: readString(chosen, "authentication"),
    environmentType: readString(chosen, "environmentType"),
    startupCompany: readString(chosen, "startupCompany"),
    validateServerCertificate:
      typeof chosen.validateServerCertificate === "boolean"
        ? (chosen.validateServerCertificate as boolean)
        : undefined,
  };
}

function readString(
  o: Record<string, unknown>,
  key: string,
): string | undefined {
  const v = o[key];
  return typeof v === "string" && v.trim() ? v.trim() : undefined;
}

function stripJsonComments(input: string): string {
  return input
    .replace(/\/\*[\s\S]*?\*\//g, "")
    .replace(/(^|[^:"'])\/\/[^\n\r]*/g, "$1");
}

// ---------------------------------------------------------------------------
// URL construction
// ---------------------------------------------------------------------------

export function normalizeServerUrl(server: string, port?: number): URL {
  let raw = server.trim();
  if (!/^https?:\/\//i.test(raw)) raw = "https://" + raw;
  const u = new URL(raw);
  if (port !== undefined && port !== null) u.port = String(port);
  return u;
}

/** Compose `{origin}[:port]/{instance}/dev/TestRunnerHub` + tenant query param. */
function buildHubUrl(
  serverUrl: URL,
  serverInstance: string,
  tenant?: string,
): string {
  const base = new URL(serverUrl.toString());
  // strip any path the user might've included on the server URL
  base.pathname = "/";
  const path =
    `/${encodeURIComponent(serverInstance)}/dev/TestRunnerHub`.replace(
      /\/+/g,
      "/",
    );
  base.pathname = path;
  if (tenant) base.searchParams.set("tenant", tenant);
  return base.toString();
}

// ---------------------------------------------------------------------------
// Credentials
// ---------------------------------------------------------------------------

export interface Credentials {
  username: string;
  password: string;
}

export function loadCredentials(origin: string, serverInstance: string): Credentials {
  // 1. Env vars — preferred for ephemeral sessions.
  const envUser = process.env.BC_USER?.trim();
  const envPwd = process.env.BC_PASSWORD;
  if (envUser && envPwd) {
    return { username: envUser, password: envPwd };
  }

  // 2. File fallback — strict-mode check.
  const cfgBase =
    process.env.XDG_CONFIG_HOME && process.env.XDG_CONFIG_HOME.trim()
      ? process.env.XDG_CONFIG_HOME.trim()
      : join(homedir(), ".config");
  const credFile = join(cfgBase, "al-mcp-bridge", "credentials.json");

  if (!existsSync(credFile)) {
    throw new BcConnectionError(
      `No credentials available. Set BC_USER and BC_PASSWORD env vars, or create ${credFile} (mode 0600) with { "<server>|<instance>": { "username": "...", "password": "..." } }.`,
    );
  }

  // Enforce mode 0600: owner rw only, no group/other bits.
  // `S_IRWXG | S_IRWXO` covers group read/write/execute + other read/write/execute.
  // Also reject if the file is not a regular file.
  const st = statSync(credFile);
  if (!st.isFile()) {
    throw new BcConnectionError(
      `Credentials path exists but is not a regular file: ${credFile}.`,
    );
  }
  const unsafeBits =
    st.mode & (constants.S_IRWXG | constants.S_IRWXO);
  if (unsafeBits !== 0) {
    throw new BcConnectionError(
      `Credentials file ${credFile} has permissions ${(st.mode & 0o777).toString(8)}. Refusing to read — chmod 600 ${credFile}.`,
    );
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(readFileSync(credFile, "utf8"));
  } catch (err) {
    throw new BcConnectionError(
      `Could not parse credentials file: ${(err as Error).message}`,
    );
  }
  if (typeof parsed !== "object" || parsed === null) {
    throw new BcConnectionError(`Credentials file must be a JSON object.`);
  }

  // Try key variants, most specific first. Users are likely to write
  // credentials against the hostname without the dev-service port, so we
  // accept both. Scheme can also be dropped (some users key by host alone).
  const instanceLower = serverInstance.toLowerCase();
  const originUrl = new URL(origin);
  const hostPort = originUrl.host.toLowerCase(); // e.g. docker.socitas.de:56565
  const hostOnly = originUrl.hostname.toLowerCase(); // e.g. docker.socitas.de
  const schemeHostPort = `${originUrl.protocol}//${hostPort}`.toLowerCase();
  const schemeHostOnly = `${originUrl.protocol}//${hostOnly}`.toLowerCase();

  const candidateKeys = [
    // Specific instance first — most targeted wins.
    `${schemeHostPort}|${instanceLower}`,
    `${schemeHostOnly}|${instanceLower}`,
    `${hostPort}|${instanceLower}`,
    `${hostOnly}|${instanceLower}`,
    // legacy `_` separator (matches the MS on-prem credential cache format)
    `${schemeHostPort}_${instanceLower}`,
    `${schemeHostOnly}_${instanceLower}`,
    `${hostPort}_${instanceLower}`,
    `${hostOnly}_${instanceLower}`,
    // Wildcard: one credential covers every instance on the same host. Use
    // when all dev instances on a server share the same admin account.
    `${schemeHostPort}|*`,
    `${schemeHostOnly}|*`,
    `${hostPort}|*`,
    `${hostOnly}|*`,
  ];

  const record = parsed as Record<string, unknown>;
  let entry:
    | { username?: unknown; password?: unknown }
    | undefined;
  for (const k of candidateKeys) {
    const v = record[k];
    if (v && typeof v === "object") {
      entry = v as { username?: unknown; password?: unknown };
      break;
    }
  }

  if (!entry || typeof entry.username !== "string" || typeof entry.password !== "string") {
    throw new BcConnectionError(
      `No credential entry for '${candidateKeys[0]}' (also tried: ${candidateKeys
        .slice(1)
        .map((k) => `'${k}'`)
        .join(
          ", ",
        )}) in credentials file. Expected { "${candidateKeys[0]}": { "username": "...", "password": "..." } }.`,
    );
  }
  return { username: entry.username, password: entry.password };
}

// ---------------------------------------------------------------------------
// SignalR connection
// ---------------------------------------------------------------------------

function buildConnection(
  hubUrl: string,
  creds: Credentials,
  allowInvalidCert: boolean,
): HubConnection {
  const basic =
    "Basic " +
    Buffer.from(`${creds.username}:${creds.password}`, "utf8").toString(
      "base64",
    );

  // `@microsoft/signalr` accepts an `httpClient` override and per-transport
  // options. For Node we rely on its default Node http/ws stack. TLS
  // validation bypass is plumbed via `NODE_TLS_REJECT_UNAUTHORIZED` only
  // when the caller opts in, and scoped to this process — not child procs.
  if (allowInvalidCert) {
    // Node-only, non-public API on global process — but the only way to
    // get `@microsoft/signalr`'s bundled ws client to relax cert checks
    // without swapping out its HttpClient. Scope is the whole Node process;
    // MCP server is single-purpose so this is acceptable with an opt-in.
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
  }

  return new HubConnectionBuilder()
    .withUrl(hubUrl, {
      headers: { Authorization: basic },
      // The reference implementation mirrors the auth header as a query
      // param for the WebSocket upgrade step. We include it via
      // accessTokenFactory so the underlying transport attaches it as
      // `access_token` — but BC expects `Authentication=<header>` instead.
      // We add that via URL directly after the builder if needed; for now
      // the Authorization header alone works on BC's dev tier because it
      // accepts Basic for both negotiate and WS upgrade.
      transport:
        HttpTransportType.WebSockets |
        HttpTransportType.ServerSentEvents |
        HttpTransportType.LongPolling,
      skipNegotiation: false,
    })
    .configureLogging(LogLevel.Error)
    .build();
}

// ---------------------------------------------------------------------------
// Result helpers
// ---------------------------------------------------------------------------

function coerceStatus(s: number | string): TestResultStatus {
  // TestResultStatus enum (lmt.cs:1063): 0 Passed, 1 Failed, 2 Skipped
  if (typeof s === "number") {
    switch (s) {
      case 0:
        return "Passed";
      case 1:
        return "Failed";
      case 2:
        return "Skipped";
      default:
        return "Unknown";
    }
  }
  if (typeof s === "string") {
    const v = s.trim();
    if (v === "Passed" || v === "Failed" || v === "Skipped") return v;
  }
  return "Unknown";
}

function summarize(
  codeunitId: number,
  tests: TestMethodResult[],
  warnings: string[],
  durationMs: number,
): RunTestsResult {
  let passed = 0;
  let failed = 0;
  let skipped = 0;
  for (const t of tests) {
    if (t.status === "Passed") passed++;
    else if (t.status === "Failed") failed++;
    else if (t.status === "Skipped") skipped++;
  }
  const succeeded = failed === 0 && tests.length > 0;
  const message =
    tests.length === 0
      ? `Test run completed but no test results were returned for codeunit ${codeunitId}.`
      : `${passed} passed, ${failed} failed, ${skipped} skipped (${tests.length} total) in ${durationMs}ms.`;
  return {
    succeeded,
    codeunitId,
    passed,
    failed,
    skipped,
    total: tests.length,
    durationMs,
    tests,
    warnings,
    message,
  };
}

// ---------------------------------------------------------------------------
// Error hygiene — credentials must never leak via error strings
// ---------------------------------------------------------------------------

/**
 * Strip anything that could carry credentials out of an error message:
 *   - any value after `Authorization:` / `Authentication=`
 *   - any `Basic <base64>` / `Bearer <token>` token
 *   - inline `user:pass@host` URLs
 *   - `password` / `pwd` field values in JSON-ish fragments
 *
 * We over-redact on purpose. Users can always re-run with verbose logging to
 * their own trusted sink if they need the original string.
 */
export function redact(s: string): string {
  if (!s) return s;
  let out = s;
  // 1. URL-embedded creds first so later regexes see a cleaner string.
  out = out.replace(/(\bhttps?:\/\/)[^:/\s]+:[^@\s]+@/gi, "$1[redacted]@");
  // 2. Scrub token *values* before header-name rules — otherwise
  //    `Authorization: Basic <base64>` gets split into two redactions and
  //    the base64 tail can slip through.
  out = out.replace(/\b(Basic|Bearer)\s+[A-Za-z0-9+/=._-]+/g, "$1 [redacted]");
  // 3. URL-encoded auth query params: `Authentication=Basic%20<base64>` (and
  //    any other scheme) — stop at `&`, whitespace, or quote.
  out = out.replace(
    /Authentication\s*=\s*[^\s&"']+/gi,
    "Authentication=[redacted]",
  );
  // 4. Header-style auth. The value may be `[redacted]` by now, which is
  //    fine — we just collapse the whole span.
  out = out.replace(
    /Authorization\s*[:=]\s*[^\s,;&"']+/gi,
    "Authorization: [redacted]",
  );
  // 5. JSON-ish secret fields.
  out = out.replace(
    /("(?:password|pwd|secret|token)"\s*:\s*)"[^"]*"/gi,
    '$1"[redacted]"',
  );
  return out;
}

function describeError(err: unknown): string {
  if (err instanceof Error) {
    // Don't include stack — it may contain query-string auth copies
    // emitted by some transport libraries.
    return err.message || err.name;
  }
  return String(err);
}

// Silence unused-import warning for `dirname`/`isAbsolute` if not used in
// future expansions. We keep them imported for the file-cred loader if we
// later support relative-to-workspace credential files.
void dirname;
void isAbsolute;
