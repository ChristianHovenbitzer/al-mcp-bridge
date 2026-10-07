#!/usr/bin/env node
/**
 * Make a real AL language server available to the test suite.
 *
 * Linux, macOS and Windows. VSIX extraction shells out to `unzip` on
 * Linux/macOS and to the bsdtar that ships with Windows (System32\tar.exe,
 * which reads zip archives) on Windows.
 *
 * Resolution order:
 *   1. AL_LS_PATH in env — already pointed at a binary, we just record it.
 *   2. Local VS Code / VS Code Server extension install at the pinned version.
 *      (`~/.vscode/extensions/ms-dynamics-smb.al-<version>` or the `-server`
 *      equivalent.) Contributors who already develop AL on this machine pay
 *      zero download cost.
 *   3. Download the VSIX from the marketplace once, cache it under
 *      `tests/.al-ls/vsix-cache/`, extract only `bin/<platform>/` plus
 *      `bin/Analyzers/` to `tests/.al-ls/<version>/`. The VSIX carries only
 *      Microsoft's cops, so the ALCops.Analyzers NuGet package (version pinned
 *      in package.json `alcops.version`) is added to `bin/Analyzers/`.
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
const ALCOPS_VERSION = process.env.ALCOPS_VERSION ?? pkg.alcops?.version;
if (!VERSION) {
  console.error(
    "[install-al-ls] no version pinned. Set alLanguageServer.version in " +
      "package.json or AL_EXT_VERSION in env.",
  );
  process.exit(2);
}

// The VSIX ships one host per OS under bin/<platform>/, named after Node's
// process.platform values.
const SUPPORTED_PLATFORMS = ["linux", "darwin", "win32"];
if (!SUPPORTED_PLATFORMS.includes(process.platform)) {
  console.error(
    `[install-al-ls] platform ${process.platform} is not supported. ` +
      "Set AL_LS_PATH manually or extend this script.",
  );
  process.exit(2);
}

const PLATFORM = process.platform;
const IS_WINDOWS = PLATFORM === "win32";
const LS_BIN_NAME = "Microsoft.Dynamics.Nav.EditorServices.Host" + (IS_WINDOWS ? ".exe" : "");

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

/** Windows' own bsdtar. Called by absolute path because a Git-for-Windows
 *  GNU tar earlier on PATH cannot read zip archives. */
function windowsTarPath() {
  return join(process.env.SystemRoot ?? "C:\\Windows", "System32", "tar.exe");
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

  await download(
    `https://marketplace.visualstudio.com/_apis/public/gallery/publishers/ms-dynamics-smb/vsextensions/al/${version}/vspackage`,
    target,
  );
  return target;
}

/** Extract the entries under `dirs` (archive-relative folder paths) from a
 *  zip archive (VSIX / nupkg) into `dest`. */
function extractZipDirs(zipPath, dirs, dest) {
  if (!IS_WINDOWS) assertUnzipAvailable();
  const result = IS_WINDOWS
    ? spawnSync(windowsTarPath(), ["-xf", zipPath, "-C", dest, ...dirs], { stdio: "inherit" })
    : spawnSync("unzip", ["-q", "-o", zipPath, ...dirs.map((d) => `${d}/*`), "-d", dest], {
        stdio: "inherit",
      });
  if (result.status !== 0) {
    throw new Error(`extracting ${zipPath} failed with status ${result.status ?? result.error}`);
  }
}

/** fetch with a few retries — a single transient network error should not
 *  fail a CI run. */
async function fetchWithRetry(url, attempts = 3) {
  for (let i = 1; ; i++) {
    try {
      return await fetch(url, { redirect: "follow" });
    } catch (err) {
      if (i >= attempts) throw err;
      log(`fetch failed (${err?.cause?.code ?? err?.message}), retry ${i}/${attempts - 1}`);
      await new Promise((r) => setTimeout(r, 2000 * i));
    }
  }
}

async function download(url, target) {
  log(`downloading ${url}`);
  const res = await fetchWithRetry(url);
  if (!res.ok || !res.body) {
    throw new Error(`download failed: ${res.status} ${res.statusText} (${url})`);
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
}

/** Target framework of the LS host (e.g. "net8.0"), so the matching ALCops
 *  build is picked. */
function hostTfm(lsPath) {
  const cfg = lsPath.replace(/\.exe$/, "") + ".runtimeconfig.json";
  const tfm = existsSync(cfg) ? JSON.parse(readFileSync(cfg, "utf8")).runtimeOptions?.tfm : undefined;
  if (!tfm) throw new Error(`cannot read the target framework from ${cfg}`);
  return tfm;
}

/** Add ALCops.Analyzers to a VSIX-extracted Analyzers folder. Idempotent. */
async function installAlcops(analyzersDir, lsPath) {
  if (!ALCOPS_VERSION) {
    log("no alcops.version pinned — skipping ALCops");
    return;
  }
  if (existsSync(join(analyzersDir, "ALCops.LinterCop.dll"))) {
    log(`ALCops already present in ${analyzersDir}`);
    return;
  }
  const tfm = hostTfm(lsPath);
  const cacheDir = join(OUT_ROOT, "nupkg-cache");
  mkdirSync(cacheDir, { recursive: true });
  const nupkg = join(cacheDir, `alcops.analyzers.${ALCOPS_VERSION}.nupkg`);
  if (!existsSync(nupkg) || statSync(nupkg).size === 0) {
    await download(
      `https://api.nuget.org/v3-flatcontainer/alcops.analyzers/${ALCOPS_VERSION}/alcops.analyzers.${ALCOPS_VERSION}.nupkg`,
      nupkg,
    );
  }
  const staging = mkdtempSync(join(tmpdir(), "alcops-extract-"));
  try {
    extractZipDirs(nupkg, [`lib/${tfm}`], staging);
    const libDir = join(staging, "lib", tfm);
    if (!existsSync(libDir)) {
      throw new Error(`ALCops.Analyzers ${ALCOPS_VERSION} has no lib/${tfm} build`);
    }
    for (const name of readdirSync(libDir).filter((n) => n.endsWith(".dll"))) {
      cpSync(join(libDir, name), join(analyzersDir, name));
    }
  } finally {
    rmSync(staging, { recursive: true, force: true });
  }
  log(`added ALCops.Analyzers ${ALCOPS_VERSION} (${tfm}) → ${analyzersDir}`);
}

function extractVsix(vsixPath, outDir) {
  if (existsSync(outDir)) rmSync(outDir, { recursive: true, force: true });
  mkdirSync(outDir, { recursive: true });

  // Extract just the two subtrees we need into a staging dir, then promote
  // `extension/*` contents to `outDir` so the final layout mirrors the VS
  // Code extension directory (<outDir>/bin/...).
  const staging = mkdtempSync(join(tmpdir(), "al-ls-extract-"));
  try {
    extractZipDirs(vsixPath, [`extension/bin/${PLATFORM}`, "extension/bin/Analyzers"], staging);
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
  if (!IS_WINDOWS) {
    // Zip entries carry no reliable exec bit; both the host and alc are spawned.
    chmodSync(lsPath, 0o755);
    const alcPath = join(outDir, "bin", PLATFORM, "alc");
    if (existsSync(alcPath)) chmodSync(alcPath, 0o755);
  }
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
  await installAlcops(extractedAnalyzers, extractedLs);

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
