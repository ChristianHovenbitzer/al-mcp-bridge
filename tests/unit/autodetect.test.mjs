/**
 * Unit coverage for AL language-server path resolution.
 *
 * Regression: the AL VS Code extension auto-updates, deleting the old
 * `ms-dynamics-smb.al-<version>` folder. A hardcoded `AL_LS_PATH` (or an
 * autodetect that only looks under `~/.vscode/extensions`) then points at a
 * binary that no longer exists, `spawn` fails ENOENT, and the bridge dies
 * with "-32000 Connection closed". Autodetect must (a) search the
 * `.vscode-server` extensions root used by remote/server installs and
 * (b) pick the newest version by numeric comparison so a bare update keeps
 * working without touching config.
 */
import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { autodetectLanguageServer } from "../../dist/config.js";

/** Create `<root>/ms-dynamics-smb.al-<version>/bin/<sub>/<binary>` and return its path. */
function makeExtension(root, version, sub, binary) {
  const binDir = sub ? join(root, `ms-dynamics-smb.al-${version}`, "bin", sub)
                     : join(root, `ms-dynamics-smb.al-${version}`, "bin");
  mkdirSync(binDir, { recursive: true });
  const p = join(binDir, binary);
  writeFileSync(p, "");
  return p;
}

test("finds the linux host binary under a .vscode-server-style root", () => {
  const root = mkdtempSync(join(tmpdir(), "al-ext-"));
  try {
    const expected = makeExtension(root, "18.0.2293710", "linux", "Microsoft.Dynamics.Nav.EditorServices.Host");
    const got = autodetectLanguageServer([root]);
    assert.equal(got, expected);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});

test("picks the newest version numerically, not lexicographically", () => {
  const root = mkdtempSync(join(tmpdir(), "al-ext-"));
  try {
    // Lexicographic sort would rank "9.x" above "18.x"; numeric must not.
    makeExtension(root, "9.0.999999", "linux", "Microsoft.Dynamics.Nav.EditorServices.Host");
    const newest = makeExtension(root, "18.0.2293710", "linux", "Microsoft.Dynamics.Nav.EditorServices.Host");
    makeExtension(root, "16.1.1860725", "linux", "Microsoft.Dynamics.Nav.EditorServices.Host");
    const got = autodetectLanguageServer([root]);
    assert.equal(got, newest);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});

test("prefers the current platform's binary when several ship side by side", () => {
  const root = mkdtempSync(join(tmpdir(), "al-ext-"));
  try {
    // The extension ships every platform's host; on Linux the win32 .exe is
    // present but not executable, so picking it yields spawn EACCES.
    const exe = makeExtension(root, "18.0.2293710", "win32", "Microsoft.Dynamics.Nav.EditorServices.Host.exe");
    const linux = makeExtension(root, "18.0.2293710", "linux", "Microsoft.Dynamics.Nav.EditorServices.Host");
    const darwin = makeExtension(root, "18.0.2293710", "darwin", "Microsoft.Dynamics.Nav.EditorServices.Host");
    assert.equal(autodetectLanguageServer([root], "linux"), linux);
    assert.equal(autodetectLanguageServer([root], "win32"), exe);
    assert.equal(autodetectLanguageServer([root], "darwin"), darwin);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});

test("searches multiple roots and returns null when none match", () => {
  const empty = mkdtempSync(join(tmpdir(), "al-ext-empty-"));
  const real = mkdtempSync(join(tmpdir(), "al-ext-real-"));
  try {
    const expected = makeExtension(real, "16.1.1860725", "win32", "Microsoft.Dynamics.Nav.EditorServices.Host.exe");
    assert.equal(autodetectLanguageServer([empty, real]), expected);
    assert.equal(autodetectLanguageServer([empty]), null);
  } finally {
    rmSync(empty, { recursive: true, force: true });
    rmSync(real, { recursive: true, force: true });
  }
});
