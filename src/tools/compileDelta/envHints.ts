/**
 * Symbol-cache diagnosis for al_compile_delta.
 *
 * Errors present on both sides are classified `environmental`, but that
 * alone does not say what to fix. These checks read only file names and
 * app.json files (no unzipping), and name the usual culprits:
 *
 *  - two symbol waves: several versions of one package in the cache (the
 *    shared LSE cache gave ~38 phantom AL0185/AL0791/AL0118 errors);
 *  - a stale sibling package: test/.alpackages holding a prebuilt copy of
 *    the repo's own app/ (GC: 138 errors, 2 real) — an identical version
 *    string is no evidence of freshness;
 *  - a platform mismatch: app.json `application` major differs from the
 *    newest Base Application in the cache (GC stage on a BC 27 cache).
 *
 * The tool only diagnoses; fixing the cache is profile-trto:al-symbols.
 */
import { existsSync, readFileSync, readdirSync } from "node:fs";
import { dirname, join, relative, resolve, sep } from "node:path";

export interface CachePackage {
  publisher: string;
  name: string;
  version: string;
  dir: string;
  file: string;
}

/** `Publisher_Name_1.2.3.4.app` → parts. Names may contain underscores; the version is the last segment. */
export function parsePackageFile(file: string, dir = ""): CachePackage | undefined {
  const m = /^(.+?)_(.+)_(\d+(?:\.\d+){1,3})\.app$/i.exec(file);
  if (!m) return undefined;
  return { publisher: m[1]!, name: m[2]!, version: m[3]!, dir, file };
}

export function listPackages(dirs: string[]): CachePackage[] {
  const out: CachePackage[] = [];
  for (const dir of dirs) {
    let entries: string[] = [];
    try {
      entries = readdirSync(dir);
    } catch {
      continue;
    }
    for (const e of entries) {
      const p = parsePackageFile(e, dir);
      if (p) out.push(p);
    }
  }
  return out;
}

export function compareVersions(a: string, b: string): number {
  const pa = a.split(".").map(Number);
  const pb = b.split(".").map(Number);
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) {
    const d = (pa[i] ?? 0) - (pb[i] ?? 0);
    if (d !== 0) return d;
  }
  return 0;
}

const major = (v: string) => Number(v.split(".")[0]);

/** Packages present in more than one version (per publisher + name). */
export function symbolWaves(pkgs: CachePackage[]): Array<{ id: string; versions: string[] }> {
  const by = new Map<string, Set<string>>();
  for (const p of pkgs) {
    const id = `${p.publisher}_${p.name}`;
    let s = by.get(id);
    if (!s) by.set(id, (s = new Set()));
    s.add(p.version);
  }
  return [...by.entries()]
    .filter(([, v]) => v.size > 1)
    .map(([id, v]) => ({ id, versions: [...v].sort(compareVersions) }))
    .sort((a, b) => a.id.localeCompare(b.id));
}

/** app.json `application` major vs the newest Base Application (or Application) major in the cache. */
export function platformMismatch(
  appJson: { application?: string },
  pkgs: CachePackage[],
): { appMajor: number; cacheMajor: number } | undefined {
  if (!appJson.application) return undefined;
  const appMajor = major(appJson.application);
  const ms = pkgs.filter((p) => p.publisher.toLowerCase() === "microsoft");
  const base = ms.filter((p) => p.name.toLowerCase() === "base application");
  const pool = base.length > 0 ? base : ms.filter((p) => p.name.toLowerCase() === "application");
  if (pool.length === 0 || !Number.isFinite(appMajor)) return undefined;
  const newest = pool.reduce((a, b) => (compareVersions(a.version, b.version) >= 0 ? a : b));
  const cacheMajor = major(newest.version);
  return cacheMajor === appMajor ? undefined : { appMajor, cacheMajor };
}

/**
 * Sibling projects in the same repo (any folder with app.json, two levels
 * deep at most, skipping dependency/build folders) whose publisher + name
 * appear as a prebuilt package in this project's cache.
 */
export function staleSiblingPackages(
  projectPath: string,
  repoTop: string,
  pkgs: CachePackage[],
): Array<{ sibling: string; file: string; packageVersion: string; siblingVersion?: string }> {
  const siblings = findAppJsons(repoTop).filter((p) => resolve(dirname(p)) !== resolve(projectPath));
  const out: Array<{ sibling: string; file: string; packageVersion: string; siblingVersion?: string }> = [];
  for (const appJsonPath of siblings) {
    const j = readJson(appJsonPath);
    if (!j?.publisher || !j?.name) continue;
    const hit = pkgs.find(
      (p) => p.publisher.toLowerCase() === j.publisher!.toLowerCase() && p.name.toLowerCase() === j.name!.toLowerCase(),
    );
    if (hit) {
      out.push({
        sibling: relative(repoTop, dirname(appJsonPath)).split(sep).join("/") || ".",
        file: hit.file,
        packageVersion: hit.version,
        siblingVersion: j.version,
      });
    }
  }
  return out;
}

const SKIP = new Set([".git", ".alpackages", ".alcache", "node_modules", ".snapshots", "graphify-out"]);

function findAppJsons(top: string): string[] {
  const out: string[] = [];
  const walk = (dir: string, depth: number) => {
    if (existsSync(join(dir, "app.json"))) out.push(join(dir, "app.json"));
    if (depth === 0) return;
    let entries: import("node:fs").Dirent[] = [];
    try {
      entries = readdirSync(dir, { withFileTypes: true });
    } catch {
      return;
    }
    for (const e of entries) {
      if (e.isDirectory() && !SKIP.has(e.name.toLowerCase()) && !e.name.startsWith(".")) walk(join(dir, e.name), depth - 1);
    }
  };
  walk(top, 2);
  return out;
}

function readJson(p: string): { publisher?: string; name?: string; application?: string; version?: string } | undefined {
  try {
    return JSON.parse(readFileSync(p, "utf8").replace(/^﻿/, ""));
  } catch {
    return undefined;
  }
}

/** One-line hints for the result; empty when the cache looks consistent. */
export function environmentHints(projectPath: string, repoTop: string, packageCachePaths: string[]): string[] {
  const pkgs = listPackages(packageCachePaths);
  const hints: string[] = [];
  if (packageCachePaths.length === 0) {
    hints.push("No symbol cache path resolved — alc reports AL1021. Download symbols (profile-trto:al-symbols) or pass packageCachePath.");
    return hints;
  }
  const mismatch = platformMismatch(readJson(join(projectPath, "app.json")) ?? {}, pkgs);
  if (mismatch) {
    hints.push(
      `Platform mismatch: app.json application is ${mismatch.appMajor}.x but the newest Base Application in the cache is ${mismatch.cacheMajor}.x — ` +
        "refresh the symbols for the right major (profile-trto:al-symbols).",
    );
  }
  const waves = symbolWaves(pkgs);
  if (waves.length > 0) {
    const shown = waves.slice(0, 3).map((w) => `${w.id} (${w.versions.join(", ")})`).join("; ");
    hints.push(
      `Symbol cache holds several waves: ${shown}${waves.length > 3 ? ` and ${waves.length - 3} more` : ""}. ` +
        "Mixed waves cause phantom AL0185/AL0791/AL0118 errors; keep one wave per major.",
    );
  }
  for (const s of staleSiblingPackages(projectPath, repoTop, pkgs)) {
    const behind = s.siblingVersion && compareVersions(s.packageVersion, s.siblingVersion) !== 0;
    hints.push(
      behind
        ? `The cache holds ${s.file}, but sibling project '${s.sibling}' is at ${s.siblingVersion} — that package is stale. ` +
            `Rebuild '${s.sibling}' and replace it before trusting errors that reference its objects.`
        : `The cache holds a prebuilt ${s.file} of sibling project '${s.sibling}'. It may be stale even with the same version string — ` +
            `rebuild '${s.sibling}' and replace it before trusting errors that reference its objects.`,
    );
  }
  return hints;
}
