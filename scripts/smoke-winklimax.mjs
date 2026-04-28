import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";

const transport = new StdioClientTransport({
  command: process.execPath,
  args: ["/home/hoch/git/al-mcp-bridge/dist/index.js"],
  env: process.env,
  stderr: "inherit",
});

const client = new Client({ name: "smoke", version: "0.0.1" }, { capabilities: {} });

async function tryTool(name, args) {
  console.error(`\n[smoke] === ${name} ${JSON.stringify(args)} ===`);
  try {
    const r = await client.callTool({ name, arguments: args });
    const txt = (r.content?.[0]?.text ?? JSON.stringify(r));
    console.log(`OK ${name}:`);
    console.log(txt.slice(0, 800));
    if (r.isError) console.log(`(isError flag set)`);
  } catch (e) {
    console.error(`FAIL ${name}:`, e?.message ?? e);
  }
}

try {
  await client.connect(transport);
  console.error("[smoke] connected");

  await tryTool("al_symbol_search", { query: "Http Client Handler" });
  await tryTool("al_symbol_search", { query: "HttpClientHandler" });
  await tryTool("al_symbol_search", { query: "Partlist" });

  const file = "/home/hoch/git/blh/win-klimaX Interface/app/src/codeunit/HttpClientHandler.Codeunit.al";
  await tryTool("al_get_symbol_at", { file, line: 13, character: 4 });
  await tryTool("al_find_references", { file, line: 13, character: 9 });
  await tryTool("al_format", { file });
  await tryTool("al_list_code_actions", { file, line: 13, character: 4 });
} catch (e) {
  console.error("[smoke] failed:", e?.stack ?? e);
  process.exitCode = 1;
} finally {
  await client.close();
}
