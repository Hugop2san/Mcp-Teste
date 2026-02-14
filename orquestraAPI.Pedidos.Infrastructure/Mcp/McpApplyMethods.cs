using orquestraAPI.Pedidos.Application.DTOs;
using orquestraAPI.Pedidos.Infrastructure.Mcp.Tools;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

namespace orquestraAPI.Pedidos.Infrastructure.Mcp
{
    public class McpApplyMethods
    {
        private readonly GetProdutosTool _getProdutosTool;
        private readonly GetProdudosIdTool _getProdutosIdTool;
        private readonly AddProdutosTool _addProdutosTool;

        public McpApplyMethods(GetProdutosTool getProdutosTool, GetProdudosIdTool getprodutosidtool, AddProdutosTool addProdutosTool)
        {
            _getProdutosTool = getProdutosTool;
            _getProdutosIdTool = getprodutosidtool;
            _addProdutosTool = addProdutosTool;
        }

        // Dispatcher MCP - direciona para a tool correta
        public async Task<object> DispatchAsync(McpRequest request)
        {
            var tool = request.Tool.Trim().ToLowerInvariant();

            return tool switch
            {
                "return_by_id" => await GetProdIdTool(request),
                "return_all" => await _getProdutosTool.ExecuteAsync(),
                "criar_produto" => await CreateProdutoTool(request),
                _ => new { ok = false, error = $"Tool '{request.Tool}' não encontrada" }
            };
        }

        // método existente: obtém produto por ID
        public async Task<object> GetProdIdTool(McpRequest requestid)
        {
            if (requestid.Args == null) return new { ok = false, error = "args obrigatório. Ex: { \"id\": 3 }" };

            var args = requestid.Args.Value;

            if (!args.TryGetProperty("id", out var idProp))
                return new { ok = false, error = "campo 'id' não encontrado" };

            if (idProp.ValueKind != JsonValueKind.Number)
                return new { ok = false, error = "'id' deve ser numérico" };

            var id = idProp.GetInt32();

            return await _getProdutosIdTool.getallbyidmcp(id);
        }

        // novo método: valida e converte preco (aceitando string)
        public async Task<object> CreateProdutoTool(McpRequest request)
        {
            if (request.Args == null)
                return new { ok = false, error = "args obrigatório. Ex: { \"nome\":\"X\",\"preco\":\"10.5\",\"quantidade\":\"2\" }" };

            var args = request.Args.Value;

            if (!args.TryGetProperty("nome", out var nomeProp) || nomeProp.ValueKind != JsonValueKind.String)
                return new { ok = false, error = "'nome' é obrigatório e deve ser string" };

            if (!args.TryGetProperty("preco", out var precoProp))
                return new { ok = false, error = "'preco' é obrigatório" };

            if (!args.TryGetProperty("quantidade", out var qtdProp))
                return new { ok = false, error = "'quantidade' é obrigatório" };

            // extrair quantidade (aceita number ou string)
            int quantidade;
            if (qtdProp.ValueKind == JsonValueKind.Number)
            {
                quantidade = qtdProp.GetInt32();
            }
            else
            {
                var qtdRaw = qtdProp.GetString()?.Trim();
                if (!int.TryParse(qtdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantidade)
                    && !int.TryParse(qtdRaw, NumberStyles.Integer, CultureInfo.CurrentCulture, out quantidade))
                {
                    return new { ok = false, error = "'quantidade' inválida" };
                }
            }

            // extrair preco (aceita number ou string) — foco desta alteração
            decimal preco;
            if (precoProp.ValueKind == JsonValueKind.Number)
            {
                // JsonElement suporta GetDecimal
                try
                {
                    preco = precoProp.GetDecimal();
                }
                catch
                {
                    return new { ok = false, error = "'preco' inválido (não foi possível ler número)" };
                }
            }
            else 
            {
                // tenta extrair string e parseá-la
                var precoRaw = precoProp.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(precoRaw))
                    return new { ok = false, error = "'preco' inválido ou vazio" };

                // tentativas de parse em ordem: InvariantCulture, CurrentCulture, replace ','->'.' + Invariant
                if (!decimal.TryParse(precoRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out preco)
                    && !decimal.TryParse(precoRaw, NumberStyles.Number, CultureInfo.CurrentCulture, out preco))
                {
                    var alt = precoRaw.Replace(',', '.');
                    if (!decimal.TryParse(alt, NumberStyles.Number, CultureInfo.InvariantCulture, out preco))
                    {
                        return new { ok = false, error = $"'preco' inválido: '{precoRaw}'" };
                    }
                }
            }

            // 
            var dto = new ProdutoDTO
            {
                Nome = nomeProp.GetString()!.Trim(),
                Preco = preco,
                Quantidade = quantidade
            };

            return await _addProdutosTool.CriarProdutosAsync(dto);
        }
    }
}
