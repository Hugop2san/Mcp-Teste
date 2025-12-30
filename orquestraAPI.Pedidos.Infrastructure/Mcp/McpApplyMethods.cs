using orquestraAPI.Pedidos.Infrastructure.Mcp.Tools;
using System;
using System.Text.Json;
using System.Threading.Tasks;




namespace orquestraAPI.Pedidos.Infrastructure.Mcp
{
    public class McpApplyMethods
    {
        private readonly GetProdutosTool _getProdutosTool ;
        private readonly GetProdudosIdTool _getProdutosIdTool;

        public McpApplyMethods(GetProdutosTool getProdutosTool, GetProdudosIdTool getprodutosidtool)
        {
            _getProdutosTool = getProdutosTool;
            _getProdutosIdTool = getprodutosidtool;
        }


        // VERIFICAR 2 ARGUMENTOS
        //ORQUESTRA OS INPUTS VINDO DO USUARIO
        public async Task<object> DispatchAsync(McpRequest request)
        {
            // Normaliza o nome da tool
            var tool = request.Tool.Trim().ToLowerInvariant();

            return tool switch
            {
                //CASE 1
                "return_by_id" => await GetProdIdTool(request),
                // CASE 2
                "return_all" => await _getProdutosTool.ExecuteAsync(),
                // CASE 3
                _ => new { ok = false, error = $"Tool '{request.Tool}' não encontrada" }
            };
        }


        public async Task<object> GetProdIdTool( McpRequest requestid ) 
        {

            if (requestid.Args == null) return new { ok= false , error= "args obrigatório. Ex: { \"id\": 3 }" };

            var args = requestid.Args.Value;

            //  valida propriedade
            if (!args.TryGetProperty("id", out var idProp))
                return new { ok = false, error = "campo 'id' não encontrado" };

            if (idProp.ValueKind != JsonValueKind.Number)
                return new { ok = false, error = "'id' deve ser numérico" };

            // extrai valor tipado
            var id = idProp.GetInt32();

            // delega para a Tool
            return await _getProdutosIdTool.getallbyidmcp(id);



        }


    }
}
