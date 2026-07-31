/**
 * al_publish — upload a compiled .app to a Business Central on-premise
 * development service tier from Linux.
 *
 * Protocol verified against `Microsoft.Dynamics.Nav.Deployment.ApiClients
 * .AppsApiClient.PublishPackageFile` (see decompiled source):
 *   endpoint : POST {server}:{port}/{instance}/dev/apps
 *   auth     : HTTP Basic — `Authorization: Basic base64(user:pass)`
 *   body     : multipart/form-data with the .app file; field name and
 *              filename both set to the .app basename.
 *   query    : SchemaUpdateMode=synchronize|forcesync|recreate (required)
 *              DependencyPublishingOption=default|ignore|strict
 *              Tenant=<id>                (when multi-tenant)
 *              ForceUpgrade=true          (optional)
 *
 * Scope (intentional, matches runTests.ts):
 *   - On-premise only. environmentType='Sandbox'/'Production' → refuse.
 *   - authentication='UserPassword' only. AAD/Windows auth → refuse.
 *   - Credentials reuse BC_USER/BC_PASSWORD or the shared credentials
 *     file; no Windows DPAPI, no Keychain.
 */
import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { basename, isAbsolute, join, resolve } from "node:path";
import { z } from "zod";
import { loadTimeouts } from "../timeouts.js";
import {
  loadCredentials,
  normalizeServerUrl,
  readLaunchConfig,
  redact,
} from "./runTests.js";

/**
 * Deadline for the upload leg. Read from env per call rather than threaded
 * through `createPublish`, which only receives the workspace path.
 */
function publishTimeoutSignal(): AbortSignal | undefined {
  const ms = loadTimeouts().publishMs;
  return ms > 0 ? AbortSignal.timeout(ms) : undefined;
}

// ---------------------------------------------------------------------------
// Input schema
// ---------------------------------------------------------------------------

const SchemaUpdateMode = z.enum(["synchronize", "forcesync", "recreate"]);

export const PublishInput = z.object({
  appPath: z
    .string()
    .optional()
    .describe(
      "Absolute path to the .app file to publish. If omitted, the newest matching .app in projectPath is used.",
    ),
  projectPath: z
    .string()
    .optional()
    .describe(
      "AL project folder (defaults to the bridge's primary workspace). Used to find the .app if appPath is omitted, and to read .vscode/launch.json.",
    ),
  launchConfig: z
    .string()
    .optional()
    .describe("Name of the launch.json configuration to use. Defaults to the first entry."),
  schemaUpdateMode: SchemaUpdateMode.default("synchronize").describe(
    "How the server reconciles table schema changes: 'synchronize' (default), 'forcesync', or 'recreate'.",
  ),
  forceUpgrade: z
    .boolean()
    .default(false)
    .describe("Skip the version-must-change check. Use when re-publishing the same version during development."),
  dependencyPublishingOption: z
    .enum(["default", "ignore", "strict"])
    .default("default")
    .describe("Controls how BC treats dependent extensions. 'default' mirrors VS Code's publish action."),
  allowInvalidCert: z
    .boolean()
    .optional()
    .describe("Skip TLS certificate validation. Default false. Prefer fixing the server cert."),
});

export type PublishInputT = z.infer<typeof PublishInput>;

// ---------------------------------------------------------------------------
// Result shape
// ---------------------------------------------------------------------------

export interface PublishResult {
  succeeded: boolean;
  statusCode: number;
  appPath: string;
  appFileName: string;
  appBytes: number;
  server: string;
  serverInstance: string;
  tenant?: string;
  schemaUpdateMode: string;
  message: string;
  serverResponse?: string;
  warnings: string[];
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

class PublishError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "PublishError";
  }
}

export function createPublish(primaryWorkspace: string) {
  return async (input: PublishInputT): Promise<PublishResult> => {
    const warnings: string[] = [];
    const projectPath = resolve(input.projectPath ?? primaryWorkspace);

    const launchCfg = readLaunchConfig(projectPath, input.launchConfig);
    if (launchCfg.environmentType && launchCfg.environmentType !== "OnPrem") {
      throw new PublishError(
        `Only on-premise launch configurations are supported. Got environmentType='${launchCfg.environmentType}'.`,
      );
    }
    if (launchCfg.authentication && launchCfg.authentication !== "UserPassword") {
      throw new PublishError(
        `Only authentication='UserPassword' is supported. Got authentication='${launchCfg.authentication}'.`,
      );
    }

    const appPath = resolveAppPath(projectPath, input.appPath);
    const appBytes = statSync(appPath).size;

    const serverUrl = normalizeServerUrl(launchCfg.server, launchCfg.port);
    if (serverUrl.protocol === "http:") {
      warnings.push(
        `launch.json server URL uses plain HTTP — credentials and package bytes travel unencrypted.`,
      );
    }

    const creds = loadCredentials(serverUrl.origin, launchCfg.serverInstance);

    const allowInvalidCert =
      input.allowInvalidCert === true ||
      process.env.BC_ALLOW_INVALID_CERT === "1" ||
      launchCfg.validateServerCertificate === false;
    if (allowInvalidCert) {
      process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
    }

    const endpoint = buildPublishUrl(serverUrl, launchCfg.serverInstance, {
      schemaUpdateMode: input.schemaUpdateMode,
      dependencyPublishingOption: input.dependencyPublishingOption,
      forceUpgrade: input.forceUpgrade,
      tenant: launchCfg.tenant,
    });

    // Build multipart body: one part, field name = filename = basename(.app).
    const fileName = basename(appPath);
    const fileBytes = readFileSync(appPath);
    const form = new FormData();
    form.append(
      fileName,
      new Blob([fileBytes], { type: "application/octet-stream" }),
      fileName,
    );

    const authHeader =
      "Basic " +
      Buffer.from(`${creds.username}:${creds.password}`, "utf8").toString("base64");

    let response: Response;
    try {
      response = await fetch(endpoint.toString(), {
        method: "POST",
        headers: { Authorization: authHeader },
        body: form,
        // A dev service tier that accepts the connection and then stalls
        // mid-upgrade would otherwise hold this call open indefinitely.
        signal: publishTimeoutSignal(),
      });
    } catch (err) {
      const isTimeout = (err as Error)?.name === "TimeoutError";
      throw new PublishError(
        redact(
          `POST ${endpoint.pathname} failed: ${(err as Error).message ?? String(err)}` +
            (isTimeout
              ? ` (aborted after ${loadTimeouts().publishMs}ms - the service tier accepted the ` +
                `connection but never finished; check whether the app landed with a manual ` +
                `GET /dev/apps before republishing)`
              : ""),
        ),
      );
    }

    const responseText = await safeReadBody(response);
    const succeeded = response.ok;
    const message = succeeded
      ? `Published ${fileName} (${formatBytes(appBytes)}) to ${serverUrl.origin}/${launchCfg.serverInstance}.`
      : `Publish failed (HTTP ${response.status} ${response.statusText}). ${extractErrorHint(responseText)}`;

    return {
      succeeded,
      statusCode: response.status,
      appPath,
      appFileName: fileName,
      appBytes,
      server: serverUrl.origin,
      serverInstance: launchCfg.serverInstance,
      tenant: launchCfg.tenant,
      schemaUpdateMode: input.schemaUpdateMode,
      message: redact(message),
      serverResponse: responseText ? redact(trim(responseText, 2000)) : undefined,
      warnings,
    };
  };
}

// ---------------------------------------------------------------------------
// .app discovery
// ---------------------------------------------------------------------------

function resolveAppPath(projectPath: string, explicit: string | undefined): string {
  if (explicit) {
    const abs = isAbsolute(explicit) ? explicit : resolve(projectPath, explicit);
    if (!existsSync(abs)) {
      throw new PublishError(`appPath does not exist: ${abs}`);
    }
    return abs;
  }
  const appJson = safeReadJson(join(projectPath, "app.json"));
  if (!appJson) {
    throw new PublishError(
      `No app.json at ${projectPath}; either compile the project first or pass appPath explicitly.`,
    );
  }
  const pub = typeof appJson.publisher === "string" ? appJson.publisher : undefined;
  const name = typeof appJson.name === "string" ? appJson.name : undefined;
  const version = typeof appJson.version === "string" ? appJson.version : undefined;
  if (pub && name && version) {
    const exact = join(projectPath, `${pub}_${name}_${version}.app`);
    if (existsSync(exact)) return exact;
  }
  if (pub && name) {
    const prefix = `${pub}_${name}_`;
    const candidates = safeReadDir(projectPath)
      .filter((e) => e.endsWith(".app") && e.startsWith(prefix))
      .sort()
      .reverse();
    if (candidates.length > 0) return join(projectPath, candidates[0]!);
  }
  const anyApps = safeReadDir(projectPath)
    .filter((e) => e.endsWith(".app"))
    .sort()
    .reverse();
  if (anyApps.length > 0) return join(projectPath, anyApps[0]!);
  throw new PublishError(
    `No .app file found in ${projectPath}. Run al_compile first, or pass appPath explicitly.`,
  );
}

function safeReadJson(path: string): Record<string, unknown> | undefined {
  try {
    return JSON.parse(readFileSync(path, "utf8")) as Record<string, unknown>;
  } catch {
    return undefined;
  }
}

function safeReadDir(path: string): string[] {
  try {
    return readdirSync(path);
  } catch {
    return [];
  }
}

// ---------------------------------------------------------------------------
// URL construction
// ---------------------------------------------------------------------------

interface PublishQueryParams {
  schemaUpdateMode: string;
  dependencyPublishingOption: string;
  forceUpgrade: boolean;
  tenant?: string;
}

function buildPublishUrl(serverUrl: URL, serverInstance: string, params: PublishQueryParams): URL {
  const base = new URL(serverUrl.toString());
  base.pathname = `/${encodeURIComponent(serverInstance)}/dev/apps`.replace(/\/+/g, "/");
  base.searchParams.set("SchemaUpdateMode", params.schemaUpdateMode);
  base.searchParams.set("DependencyPublishingOption", params.dependencyPublishingOption);
  if (params.forceUpgrade) base.searchParams.set("ForceUpgrade", "true");
  if (params.tenant) base.searchParams.set("Tenant", params.tenant);
  return base;
}

// ---------------------------------------------------------------------------
// Response helpers
// ---------------------------------------------------------------------------

async function safeReadBody(r: Response): Promise<string> {
  try {
    return await r.text();
  } catch {
    return "";
  }
}

/**
 * BC dev endpoints answer with either plain-text English messages or
 * JSON envelopes like `{"message":"..."}`. Extract a compact human hint.
 */
function extractErrorHint(body: string): string {
  if (!body) return "(empty response body)";
  try {
    const parsed = JSON.parse(body);
    if (parsed && typeof parsed === "object" && typeof (parsed as Record<string, unknown>).message === "string") {
      return trim((parsed as { message: string }).message, 500);
    }
  } catch {
    // fall through
  }
  return trim(body, 500);
}

function trim(s: string, max: number): string {
  return s.length <= max ? s : s.slice(0, max) + "…";
}

function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`;
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`;
  return `${(n / (1024 * 1024)).toFixed(2)} MB`;
}
