const API_BASE = "https://localhost:7262"; 
// ou https://localhost:7262

export async function callMcp(tool, args = null) {
  console.log("[MCP] calling:", API_BASE, tool, args);

  let res;
  try {
    res = await fetch(`${API_BASE}/mcp`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ tool, args }),
    });
  } catch (e) {
    console.error("[MCP] fetch failed:", e);
    throw e;
  }

  const text = await res.text();
  console.log("[MCP] status:", res.status, "body:", text);

  let data;
  try {
    data = JSON.parse(text);
  } catch {
    data = { ok: false, error: text };
  }

  if (!res.ok) {
    throw new Error(data?.error || `HTTP ${res.status}`);
  }

  return data;
}
