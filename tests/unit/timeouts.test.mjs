/**
 * Unit tests for the deadline primitives. These guard the property the whole
 * anti-hang change rests on: nothing awaits forever, and a fired deadline
 * cancels the work it abandoned.
 */
import test from "node:test";
import assert from "node:assert/strict";
import { TimeoutError, loadTimeouts, withTimeout } from "../../dist/timeouts.js";

test("withTimeout resolves through when the promise wins", async () => {
  const v = await withTimeout(Promise.resolve(42), 1000, "fast op");
  assert.equal(v, 42);
});

test("withTimeout rejects with a TimeoutError naming the operation", async () => {
  const never = new Promise(() => {});
  await assert.rejects(() => withTimeout(never, 20, "al/setActiveWorkspace"), (err) => {
    assert.ok(err instanceof TimeoutError);
    assert.equal(err.timeoutMs, 20);
    assert.match(err.message, /al\/setActiveWorkspace/);
    assert.match(err.message, /al_restart_lsp/);
    return true;
  });
});

test("withTimeout invokes onTimeout exactly once, and not on success", async () => {
  let cancels = 0;
  await assert.rejects(() =>
    withTimeout(new Promise(() => {}), 10, "hung", () => cancels++),
  );
  await new Promise((r) => setTimeout(r, 30));
  assert.equal(cancels, 1);

  let cancelsOnSuccess = 0;
  await withTimeout(Promise.resolve("ok"), 50, "quick", () => cancelsOnSuccess++);
  await new Promise((r) => setTimeout(r, 80));
  assert.equal(cancelsOnSuccess, 0);
});

test("withTimeout preserves the original rejection", async () => {
  const boom = new Error("LS said no");
  await assert.rejects(() => withTimeout(Promise.reject(boom), 1000, "op"), boom);
});

test("timeoutMs <= 0 disables the deadline (debug escape hatch)", async () => {
  const v = await withTimeout(Promise.resolve(1), 0, "unbounded");
  assert.equal(v, 1);
});

test("loadTimeouts applies env overrides and ignores garbage", () => {
  const base = loadTimeouts({});
  assert.ok(base.lspRequestMs > 0);
  assert.ok(base.lspReadyMs >= base.lspInitMs);

  const overridden = loadTimeouts({
    AL_BRIDGE_LSP_REQUEST_TIMEOUT_MS: "1234",
    AL_BRIDGE_COMPILE_TIMEOUT_MS: "0",
    AL_BRIDGE_TOOL_TIMEOUT_MS: "not-a-number",
    AL_BRIDGE_PUBLISH_TIMEOUT_MS: "  ",
  });
  assert.equal(overridden.lspRequestMs, 1234);
  assert.equal(overridden.compileMs, 0, "0 must survive as 'disabled'");
  assert.equal(overridden.toolMs, base.toolMs, "garbage falls back to the default");
  assert.equal(overridden.publishMs, base.publishMs, "blank falls back to the default");
});
