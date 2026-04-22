#!/usr/bin/env node
/**
 * Make a real AL language server available to the test suite.
 *
 * Linux-only for now — we shell out to `unzip` for VSIX extraction. If you
 * need Windows or a machine without unzip, swap in a pure-Node extractor.
 *
 * Resolution order:
 *   1. AL_LS_PATH in env — already pointed at a binary, we just record it.
 *   2. Local VS Code / VS Code Server extension install at the pinned version.
 *      (`~/.vscode/extensions/ms-dynamics-smb.al-<version>` or the `-server`
 *      equivalent.) Contributors who already develop AL on this machine pay
 *      zero download cost.
 *   3. Download the VSIX from the marketplace once, cache it under
 *      `tests/.al-ls/vsix-cache/`, extract only `bin/linux/` plus
 *      `bin/Analyzers/` to `tests/.al-ls/<version>/`.
 *
 * In all cases we write `tests/.al-ls/current.json` with the absolute paths
 * the test helper needs. Idempotent — re-running with everything in place is
 * a no-op.
 */
import { spawnSync } from "node:child_process";
import {
  chmodSync,
  createWriteStream,
  cpSync,
  existsSync,
  mkdirSync,
  mkdtempSync,
  readFileSync,
  readdirSync,
  renameSync,
  rmSync,
  statSync,
  writeFileSync,
} from "node:fs";
import { homedir, tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_DIR = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = resolve(SCRIPT_DIR, "..");
const OUT_ROOT = join(REPO_ROOT, "tests", ".al-ls");

const pkg = JSON.parse(readFileSync(join(REPO_ROOT, "package.json"), "utf8"));
const VERSION = process.env.AL_EXT_VERSION ?? pkg.alLanguageServer?.version;
if (!VERSION) {
  console.error(
    "[install-al-ls] no version pinned. Set alLanguageServer.version in " +
      "package.json or AL_EXT_VERSION in env.",
  );
  process.exit(2);
}

if (process.platform !== "linux") {
  console.error(
    `[install-al-ls] platform ${process.platform} is not supported yet. ` +
      "Set AL_LS_PATH manually or extend this script.",
  );
  process.exit(2);
}

const PLATFORM = "linux";
const LS_BIN_NAME = "Microsoft.Dynamics.Nav.EditorServices.Host";

function log(msg) {
  process.stderr.write(`[install-al-ls] ${msg}\n`);
}

function writeCurrent(info) {
  mkdirSync(OUT_ROOT, { recursive: true });
  writeFileSync(join(OUT_ROOT, "current.json"), JSON.stringify(info, null, 2));
  log(`wrote ${join(OUT_ROOT, "current.json")}`);
}

function findLocalExtensionInstall(version) {
  const candidates = [
    join(homedir(), ".vscode", "extensions", `ms-dynamics-smb.al-${version}`),
    join(homedir(), ".vscode-server", "extensions", `ms-dynamics-smb.al-${version}`),
  ];
  for (const p of candidates) {
    const lsPath = join(p, "bin", PLATFORM, LS_BIN_NAME);
    if (existsSync(lsPath)) {
      return { root: p, lsPath, analyzersDir: join(p, "bin", "Analyzers") };
    }
  }
  return null;
}

function assertUnzipAvailable() {
  const probe = spawnSync("unzip", ["-v"], { stdio: "ignore" });
  if (probe.status !== 0) {
    throw new Error(
      "`unzip` is required to extract the VSIX. Install it " +
        "(`sudo apt install unzip` / `brew install unzip`) or set AL_LS_PATH " +
        "to point at an existing extension install.",
    );
  }
}

async function downloadVsix(version) {
  const cacheDir = join(OUT_ROOT, "vsix-cache");
  mkdirSync(cacheDir, { recursive: true });
  const target = join(cacheDir, `ms-dynamics-smb.al-${version}.vsix`);
  if (existsSync(target) && statSync(target).size > 0) {
    log(`vsix cache hit: ${target}`);
    return target;
  }

  const url = `https://marketplace.visualstudio.com/_apis/public/gallery/publishers/ms-dynamics-smb/vsextensions/al/${version}/vspackage`;
  log(`downloading ${url}`);
  const res = await fetch(url, { redirect: "follow" });
  if (!res.ok || !res.body) {
    throw new Error(`vsix download failed: ${res.status} ${res.statusText}`);
  }

  const tmp = target + ".partial";
  const sink = createWriteStream(tmp);
  const reader = res.body.getReader();
  let bytes = 0;
  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    bytes += value.length;
    if (!sink.write(value)) {
      await new Promise((r) => sink.once("drain", r));
    }
  }
  await new Promise((resolveCb, rejectCb) =>
    sink.end((err) => (err ? rejectCb(err) : resolveCb(undefined))),
  );
  renameSync(tmp, target);
  log(`downloaded ${bytes} bytes → ${target}`);
  return target;
}

function extractVsix(vsixPath, outDir) {
  assertUnzipAvailable();
  if (existsSync(outDir)) rmSync(outDir, { recursive: true, force: true });
  mkdirSync(outDir, { recursive: true });

  // Extract just the two subtrees we need into a staging dir, then promote
  // `extension/*` contents to `outDir` so the final layout mirrors the VS
  // Code extension directory (<outDir>/bin/...).
  const staging = mkdtempSync(join(tmpdir(), "al-ls-extract-"));
  try {
    const result = spawnSync(
      "unzip",
      [
        "-q",
        "-o",
        vsixPath,
        `extension/bin/${PLATFORM}/*`,
        "extension/bin/Analyzers/*",
        "-d",
        staging,
      ],
      { stdio: "inherit" },
    );
    if (result.status !== 0) {
      throw new Error(`unzip failed with status ${result.status}`);
    }
    const extRoot = join(staging, "extension");
    if (!existsSync(extRoot)) {
      throw new Error(`expected ${extRoot} after unzip; VSIX layout changed?`);
    }
    for (const name of readdirSync(extRoot)) {
      cpSync(join(extRoot, name), join(outDir, name), { recursive: true });
    }
  } finally {
    rmSync(staging, { recursive: true, force: true });
  }

  const lsPath = join(outDir, "bin", PLATFORM, LS_BIN_NAME);
  if (!existsSync(lsPath)) {
    throw new Error(`LS binary missing after extract: ${lsPath}`);
  }
  chmodSync(lsPath, 0o755);
  log(`extracted → ${outDir}`);
}

async function main() {
  if (process.env.AL_LS_PATH) {
    const lsPath = process.env.AL_LS_PATH;
    if (!existsSync(lsPath)) {
      throw new Error(`AL_LS_PATH does not exist: ${lsPath}`);
    }
    const binDir = dirname(lsPath);
    const analyzersDir = join(dirname(binDir), "Analyzers");
    writeCurrent({
      version: VERSION,
      source: "env",
      languageServerPath: lsPath,
      analyzersDir: existsSync(analyzersDir) ? analyzersDir : null,
      platform: PLATFORM,
    });
    return;
  }

  const local = findLocalExtensionInstall(VERSION);
  if (local) {
    log(`reusing local install at ${local.root}`);
    writeCurrent({
      version: VERSION,
      source: "local-extension",
      languageServerPath: local.lsPath,
      analyzersDir: local.analyzersDir,
      platform: PLATFORM,
    });
    return;
  }

  const versionDir = join(OUT_ROOT, VERSION);
  const extractedLs = join(versionDir, "bin", PLATFORM, LS_BIN_NAME);
  const extractedAnalyzers = join(versionDir, "bin", "Analyzers");

  if (!existsSync(extractedLs) || !existsSync(extractedAnalyzers)) {
    const vsix = await downloadVsix(VERSION);
    extractVsix(vsix, versionDir);
  } else {
    log(`extraction cache hit: ${versionDir}`);
  }

  writeCurrent({
    version: VERSION,
    source: "vsix",
    languageServerPath: extractedLs,
    analyzersDir: extractedAnalyzers,
    platform: PLATFORM,
  });
}

if (process.argv.includes("--status")) {
  const p = join(OUT_ROOT, "current.json");
  if (!existsSync(p)) {
    console.log("(not installed — run `npm run install:al-ls`)");
    process.exit(0);
  }
  const info = JSON.parse(readFileSync(p, "utf8"));
  console.log(JSON.stringify(info, null, 2));
  if (info.analyzersDir && existsSync(info.analyzersDir)) {
    console.log("\nanalyzer DLLs:");
    for (const f of readdirSync(info.analyzersDir).filter((n) => n.endsWith(".dll"))) {
      console.log("  " + f);
    }
  }
  process.exit(0);
}

main().catch((err) => {
  console.error("[install-al-ls] failed:", err?.stack ?? err);
  process.exit(1);
});
