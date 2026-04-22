#!/usr/bin/env node
// Ad-hoc probe: boot the bridge against a fixture and dump all diagnostic
// codes per file. Used to discover which rule IDs a fixture actually
// triggers so we can assert on them.
import { startBridge, fixturePath } from "../tests/helpers/bridge.mjs";
import { readdirSync, statSync } from "node:fs";
import { join } from "node:path";

const fx = fixturePath(process.argv[2] ?? "analyzers-sanity");
const srcDir = join(fx, "src");
const files = readdirSync(srcDir)
  .filter((n) => n.endsWith(".al"))
  .map((n) => join(srcDir, n));

const bridge = await startBridge({ workspace: fx });
try {
  // Prime every file so the LS opens + analyzes them.
  for (const file of files) {
    await bridge.callTool("al_get_diagnostics", { file, waitForFresh: true });
  }
  // Poll a few times to let background analyzers finish.
  for (let i = 0; i < 5; i++) {
    await new Promise((r) => setTimeout(r, 2000));
  }
  // Final collect.
  console.log(`\n===== FINAL DIAGNOSTICS =====\n`);
  for (const file of files) {
    const res = await bridge.callTool("al_get_diagnostics", {
      file,
      waitForFresh: false,
      debugCache: true,
    });
    const diags = res.parsed?.diagnostics ?? [];
    console.log(`${file}  (${diags.length} diagnostics)`);
    for (const d of diags) {
      console.log(`  ${d.severity.padEnd(7)} ${(d.code ?? "").padEnd(8)} L${d.startLine}  ${d.message}`);
    }
  }
  // App.json is where AD0001 lives.
  const appJsonRes = await bridge.callTool("al_get_diagnostics", {
    file: join(fx, "app.json"),
    waitForFresh: false,
  });
  const appDiags = appJsonRes.parsed?.diagnostics ?? [];
  console.log(`\napp.json  (${appDiags.length} diagnostics)`);
  const codeCounts = {};
  for (const d of appDiags) codeCounts[d.code ?? "?"] = (codeCounts[d.code ?? "?"] ?? 0) + 1;
  console.log("  codes:", codeCounts);
  for (const d of appDiags.slice(0, 2)) {
    console.log(`  SAMPLE ${d.code}: ${d.message.slice(0, 200)}`);
  }
} finally {
  await bridge.close();
}
