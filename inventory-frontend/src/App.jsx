import { useEffect, useMemo, useState } from "react";
import { callMcp } from "./mcpClient";


function formatBRL(value) {
  const n = Number(value);
  if (Number.isNaN(n)) return value;
  return n.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

export default function App() {
  const [produtos, setProdutos] = useState([]);
  const [loading, setLoading] = useState(true);

  // Copilot
  const [prompt, setPrompt] = useState("");
  const [chat, setChat] = useState([
    { role: "system", text: "Digite um comando: 'listar', 'produto 5', 'estoque baixo 5'." },
  ]);
  const [busy, setBusy] = useState(false);

  const totalItens = useMemo(() => {
    return produtos.reduce((acc, p) => acc + Number(p.quantidade ?? 0), 0);
  }, [produtos]);

  const valorTotal = useMemo(() => {
    return produtos.reduce((acc, p) => {
      const preco = Number(p.preco ?? 0);
      const qtd = Number(p.quantidade ?? 0);
      return acc + preco * qtd;
    }, 0);
  }, [produtos]);

  const estoqueBaixo = useMemo(() => {
    // default: < 5
    return produtos.filter((p) => Number(p.quantidade ?? 0) < 5);
  }, [produtos]);

  async function loadProdutos() {
    setLoading(true);
    try {
      const r = await callMcp("get_produtos", null);
      setProdutos(r.data ?? []);
    } catch (e) {
      setChat((c) => [...c, { role: "assistant", text: `Erro ao carregar produtos: ${String(e.message || e)}` }]);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadProdutos();
  }, []);

  // Router de linguagem natural (simples e portfólio-friendly)
  function decideToolFromText(text) {
    const t = text.trim().toLowerCase();

    // "produto 5"
    const idMatch = t.match(/produto\s+(\d+)/);
    if (idMatch) return { tool: "get_produto_by_id", args: { id: Number(idMatch[1]) } };

    // "listar"
    if (t.includes("listar") || t.includes("lista") || t === "produtos") {
      return { tool: "get_produtos", args: null };
    }

    // "estoque baixo 5"
    const lowMatch = t.match(/estoque\s+baixo\s*(\d+)?/);
    if (lowMatch) {
      const limite = lowMatch[1] ? Number(lowMatch[1]) : 5;
      // Se você ainda não tem tool de estoque baixo, o front calcula local
      return { tool: "local_estoque_baixo", args: { limite } };
    }

    // "total"
    if (t.includes("total")) return { tool: "local_total_estoque", args: null };

    return null;
  }

  async function handleSend(e) {
    e.preventDefault();
    const text = prompt.trim();
    if (!text || busy) return;

    setChat((c) => [...c, { role: "user", text }]);
    setPrompt("");
    setBusy(true);

    try {
      const decision = decideToolFromText(text);

      if (!decision) {
        setChat((c) => [
          ...c,
          {
            role: "assistant",
            text:
              "Não entendi. Tente: 'listar', 'produto 5', 'estoque baixo 5' ou 'total'.",
          },
        ]);
        return;
      }

      // Tools locais (sem precisar criar tool no backend agora)
      if (decision.tool === "local_estoque_baixo") {
        const limite = decision.args?.limite ?? 5;
        const low = produtos.filter((p) => Number(p.quantidade ?? 0) < limite);
        setChat((c) => [
          ...c,
          {
            role: "assistant",
            text: `Produtos com estoque abaixo de ${limite}: ${low.length}`,
          },
          {
            role: "assistant",
            text: JSON.stringify(low, null, 2),
          },
        ]);
        return;
      }

      if (decision.tool === "local_total_estoque") {
        setChat((c) => [
          ...c,
          { role: "assistant", text: `Total de itens: ${totalItens}` },
          { role: "assistant", text: `Valor total estimado: ${formatBRL(valorTotal)}` },
        ]);
        return;
      }

      // Tools MCP (backend)
      const result = await callMcp(decision.tool, decision.args);

      // Se buscar por id, mostra só o objeto
      if (decision.tool === "get_produto_by_id") {
        if (!result.ok) {
          setChat((c) => [...c, { role: "assistant", text: result.error ?? "Não encontrado." }]);
        } else {
          setChat((c) => [
            ...c,
            { role: "assistant", text: "Resultado:" },
            { role: "assistant", text: JSON.stringify(result.data, null, 2) },
          ]);
        }
        return;
      }

      // Se listar, atualiza tabela também
      if (decision.tool === "get_produtos") {
        const data = result.data ?? [];
        setProdutos(data);
        setChat((c) => [...c, { role: "assistant", text: `OK. Carreguei ${data.length} produtos.` }]);
        return;
      }

      // fallback
      setChat((c) => [...c, { role: "assistant", text: JSON.stringify(result, null, 2) }]);
    } catch (e) {
      setChat((c) => [...c, { role: "assistant", text: `Erro: ${String(e.message || e)}` }]);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      {/* Top bar */}
      <header className="border-b border-slate-800 bg-slate-950/60 backdrop-blur">
        <div className="mx-auto max-w-6xl px-5 py-4 flex items-center justify-between">
          <div>
            <h1 className="text-xl font-semibold tracking-tight">Inventory Copilot</h1>
            <p className="text-sm text-slate-400">
              .NET (DDD) + MCP Tools + MockAPI (estoque)
            </p>
          </div>
          <button
            onClick={loadProdutos}
            className="rounded-xl bg-slate-800 hover:bg-slate-700 px-4 py-2 text-sm font-medium"
          >
            Recarregar
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-5 py-8 grid gap-6 lg:grid-cols-3">
        {/* Left: Dashboard */}
        <section className="lg:col-span-2 space-y-6">
          {/* KPIs */}
          <div className="grid gap-4 sm:grid-cols-3">
            <KpiCard title="Produtos" value={loading ? "…" : String(produtos.length)} />
            <KpiCard title="Total de itens" value={loading ? "…" : String(totalItens)} />
            <KpiCard title="Valor estimado" value={loading ? "…" : formatBRL(valorTotal)} />
          </div>

          {/* Alerts */}
          <div className="rounded-2xl border border-slate-800 bg-slate-900/40 p-5">
            <div className="flex items-center justify-between">
              <h2 className="font-semibold">Alertas</h2>
              <span className="text-xs text-slate-400">Estoque baixo &lt; 5</span>
            </div>
            <div className="mt-3 text-sm text-slate-300">
              {loading ? (
                "Carregando…"
              ) : estoqueBaixo.length === 0 ? (
                "Nenhum item com estoque baixo."
              ) : (
                <div className="flex flex-wrap gap-2">
                  {estoqueBaixo.slice(0, 8).map((p) => (
                    <span
                      key={p.id}
                      className="rounded-full bg-rose-500/10 text-rose-200 border border-rose-500/30 px-3 py-1 text-xs"
                    >
                      {p.nome} (qtd {p.quantidade})
                    </span>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Table */}
          <div className="rounded-2xl border border-slate-800 bg-slate-900/40 p-5">
            <div className="flex items-center justify-between">
              <h2 className="font-semibold">Produtos</h2>
              <span className="text-xs text-slate-400">
                Fonte: MockAPI via Repository
              </span>
            </div>

            <div className="mt-4 overflow-auto">
              <table className="w-full text-sm">
                <thead className="text-left text-slate-400">
                  <tr className="border-b border-slate-800">
                    <th className="py-2 pr-3">ID</th>
                    <th className="py-2 pr-3">Nome</th>
                    <th className="py-2 pr-3">Preço</th>
                    <th className="py-2 pr-3">Qtd</th>
                    <th className="py-2 pr-3">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    <tr>
                      <td className="py-4 text-slate-300" colSpan={5}>
                        Carregando…
                      </td>
                    </tr>
                  ) : produtos.length === 0 ? (
                    <tr>
                      <td className="py-4 text-slate-300" colSpan={5}>
                        Nenhum produto retornado.
                      </td>
                    </tr>
                  ) : (
                    produtos.map((p) => {
                      const preco = Number(p.preco ?? 0);
                      const qtd = Number(p.quantidade ?? 0);
                      return (
                        <tr key={p.id} className="border-b border-slate-900">
                          <td className="py-2 pr-3 text-slate-200">{p.id}</td>
                          <td className="py-2 pr-3">{p.nome}</td>
                          <td className="py-2 pr-3">{formatBRL(preco)}</td>
                          <td className="py-2 pr-3">{qtd}</td>
                          <td className="py-2 pr-3">{formatBRL(preco * qtd)}</td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </section>

        {/* Right: Copilot */}
        <aside className="rounded-2xl border border-slate-800 bg-slate-900/40 p-5 flex flex-col">
          <h2 className="font-semibold">Copilot</h2>
          <p className="text-sm text-slate-400 mt-1">
            Comandos: <span className="text-slate-200">listar</span>,{" "}
            <span className="text-slate-200">produto 5</span>,{" "}
            <span className="text-slate-200">estoque baixo 5</span>,{" "}
            <span className="text-slate-200">total</span>
          </p>

          <div className="mt-4 flex-1 overflow-auto rounded-xl border border-slate-800 bg-slate-950/40 p-3">
            {chat.map((m, idx) => (
              <ChatBubble key={idx} role={m.role} text={m.text} />
            ))}
          </div>

          <form onSubmit={handleSend} className="mt-4 flex gap-2">
            <input
              className="flex-1 rounded-xl border border-slate-800 bg-slate-950 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-slate-600"
              value={prompt}
              onChange={(e) => setPrompt(e.target.value)}
              placeholder={busy ? "Executando…" : "Ex: produto 5"}
            />
            <button
              type="submit"
              disabled={busy}
              className="rounded-xl bg-indigo-600 hover:bg-indigo-500 disabled:opacity-60 px-4 py-2 text-sm font-semibold"
            >
              Enviar
            </button>
          </form>
        </aside>
      </main>

      <footer className="mx-auto max-w-6xl px-5 pb-10 text-xs text-slate-500">
        Dica: esse Copilot hoje usa um roteador simples no frontend. Depois você pode trocar por OpenAI para escolher tools automaticamente.
      </footer>
    </div>
  );
}

function KpiCard({ title, value }) {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/40 p-5">
      <div className="text-xs text-slate-400">{title}</div>
      <div className="mt-2 text-2xl font-semibold">{value}</div>
    </div>
  );
}

function ChatBubble({ role, text }) {
  const isUser = role === "user";
  const isSystem = role === "system";
  const base =
    "mb-2 whitespace-pre-wrap rounded-xl px-3 py-2 text-sm border";
  const cls = isSystem
    ? `${base} border-slate-800 bg-slate-950 text-slate-300`
    : isUser
    ? `${base} border-indigo-500/30 bg-indigo-500/10 text-indigo-100 ml-6`
    : `${base} border-slate-800 bg-slate-950/60 text-slate-200 mr-6`;

  return <div className={cls}>{text}</div>;
}
